using Godot;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;

namespace OmoriSandbox.Modding;

// Godot loads the game and 0Harmony into an isolated AssemblyLoadContext. MonoMod generates its ILGeneratorProxy
// assembly with Assembly.Load(byte[]), whose dependency lookup skips that context and falls back to the default one.
// exported builds list 0Harmony.dll among the trusted platform assemblies, so the default context silently loads a
// second copy of Harmony (AssemblyResolve is never raised because probing succeeds) and the proxy's generic constraint
// then rejects the game's CecilILGenerator with "violates the constraint of type parameter 'TTarget'". the editor's
// assembly list has no 0Harmony.dll, which is why the AssemblyResolve workaround in ModManager only ever helped there.
// this builds the same proxy with Reflection.Emit inside Harmony's own load context and stores it in MonoMod's private
// ILGeneratorBuilder.ProxyType cache before any mod patches, so MonoMod never generates its own.
// this depends on private MonoMod internals bundled in Lib.Harmony 2.4.2 and must be re-verified when upgrading Harmony.
// see https://github.com/pardeike/Harmony/issues/642 and https://github.com/MonoMod/MonoMod/pull/207
// TLDR: stop Harmony from injecting two instances of itself
internal static class HarmonyLoadContextFix
{
	public static void Install()
	{
		try
		{
			InstallProxy(typeof(HarmonyLib.Harmony).Assembly);
		}
		catch (Exception ex)
		{
			GD.PushWarning($"Failed to install the Harmony load context fix, Harmony patches will fail to apply in exported builds! {ex}");
		}
	}

	private static void InstallProxy(Assembly harmony)
	{
		Type shim = harmony.GetType("MonoMod.Utils.Cil.ILGeneratorShim", true);
		Type builder = shim.GetNestedType("ILGeneratorBuilder", BindingFlags.NonPublic | BindingFlags.Public);
		FieldInfo cache = builder.GetField("ProxyType", BindingFlags.NonPublic | BindingFlags.Static);
		Type cecil = harmony.GetType("MonoMod.Utils.Cil.CecilILGenerator", true);
		if (cache.GetValue(null) is Type existing)
		{
			Type constraint = existing.GetGenericArguments()[0].GetGenericParameterConstraints().Single();
			if (constraint.IsAssignableFrom(cecil))
				return;
		}

		AssemblyLoadContext context = AssemblyLoadContext.GetLoadContext(harmony);
		using var scope = context.EnterContextualReflection();
		AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(
			new AssemblyName("MonoMod.Utils.Cil.ILGeneratorProxy"), AssemblyBuilderAccess.Run);
		Type ignoresAccess = harmony.GetType("System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute", true);
		ConstructorInfo ignoresCtor = ignoresAccess.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
			null, [typeof(string)], null);
		assembly.SetCustomAttribute(new CustomAttributeBuilder(ignoresCtor, [harmony.GetName().Name]));
		ModuleBuilder module = assembly.DefineDynamicModule("MonoMod.Utils.Cil.ILGeneratorProxy");
		TypeBuilder proxy = module.DefineType("MonoMod.Utils.Cil.ILGeneratorProxy", TypeAttributes.Public, typeof(ILGenerator));
		GenericTypeParameterBuilder targetType = proxy.DefineGenericParameters("TTarget")[0];
		targetType.SetBaseTypeConstraint(shim);
		FieldBuilder targetField = proxy.DefineField("Target", targetType, FieldAttributes.Public);
		ConstructorBuilder ctor = proxy.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, [targetType]);
		ILGenerator ctorIl = ctor.GetILGenerator();
		ConstructorInfo baseCtor = typeof(ILGenerator).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
			null, Type.EmptyTypes, null);
		ctorIl.Emit(OpCodes.Ldarg_0);
		ctorIl.Emit(OpCodes.Call, baseCtor);
		ctorIl.Emit(OpCodes.Ldarg_0);
		ctorIl.Emit(OpCodes.Ldarg_1);
		ctorIl.Emit(OpCodes.Stfld, targetField);
		ctorIl.Emit(OpCodes.Ret);

		foreach (MethodInfo method in typeof(ILGenerator).GetMethods(BindingFlags.Instance | BindingFlags.Public))
		{
			Type[] parameters = method.GetParameters().Select(p => p.ParameterType).ToArray();
			MethodInfo target = shim.GetMethod(method.Name, parameters);
			if (target == null)
				continue;
			MethodBuilder forward = proxy.DefineMethod(method.Name,
				MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
				method.ReturnType, parameters);
			ILGenerator il = forward.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldfld, targetField);
			for (int i = 0; i < parameters.Length; i++)
				il.Emit(OpCodes.Ldarg, (short)(i + 1));
			il.Emit(target.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, target);
			il.Emit(OpCodes.Ret);
		}

		Type generated = proxy.CreateType();
		// fail here rather than on the first patch if the proxy is somehow incompatible
		generated.MakeGenericType(cecil);
		cache.SetValue(null, generated);
		GD.Print($"Installed Harmony ILGeneratorProxy in {context}");
	}
}
