namespace OmoriSandbox.Modding;

internal record ModMetadata(string Id, string Name, string Description, string Author, string Version, string Icon)
{
	internal bool Validate(out string error)
	{
		if (string.IsNullOrWhiteSpace(Id)) { error = "Missing required field 'id'"; return false; }
		if (string.IsNullOrWhiteSpace(Name)) { error = "Missing required field 'name'"; return false; }
		if (string.IsNullOrWhiteSpace(Author)) { error = "Missing required field 'author'"; return false; }
		error = null;
		return true;
	}
}