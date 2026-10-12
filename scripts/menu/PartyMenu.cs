using Godot;
using OmoriSandbox.Battle;
using OmoriSandbox.Editor;

namespace OmoriSandbox.Menu;
internal partial class PartyMenu : Menu, ISkinnableMenu
{
	[Export] private Sprite2D FightSprite;
	[Export] private Sprite2D RunSprite;
	
	protected override Vector2 OpenPosition => new(320, 480);
	protected override Vector2 ClosedPosition => new(320, 572);
	
	public override void _Ready()
	{
		Options = ["Fight", "Run"];
		CursorPositions = [new Vector2I(-98, -70), new Vector2I(-78, -31)];
	}

	public void SetSkinMode(MenuSkinMode mode)
	{
		ApplySkin(Database.GetMenuSkin(mode.ToString()));
	}

	public void ApplySkin(MenuSkin skin)
	{
		FightSprite.Texture = skin.PartyCommandsTexture;
		FightSprite.RegionRect = skin.Fight;
		RunSprite.Texture = skin.PartyCommandsTexture;
		RunSprite.RegionRect = skin.Run;
	}

	protected override void MoveCursor(Vector2I direction, bool wrap)
	{
		int old = CursorIndex;
		int next = CursorIndex + direction.Y;
		CursorIndex = wrap ? (next + Options.Count) % Options.Count : Mathf.Clamp(next, 0, Options.Count - 1);
		UpdateCursor();
		// only play a sound if the cursor actually moved
		if (old != CursorIndex)
			AudioManager.Instance.PlaySFX("SYS_move", volume: 0.9f);
	}

	public override void OnOpen(SelectionMemory memory)
	{
		CursorSprite.StartBounce();
		WaitForSecondRun = false;
		base.OnOpen(memory);
	}

	private bool WaitForSecondRun = false;

	protected override void OnSelect()
	{
		CursorSprite.StopBounce();
		AudioManager.Instance.PlaySFX("SYS_select", volume: 0.9f);
		if (CursorIndex == 0)
		{
			WaitForSecondRun = false;
			BattleManager.Instance.OnFightSelected();
		}
		else
		{
			if (SettingsMenuManager.Instance.PreventAccidentalRun && !WaitForSecondRun)
			{
				WaitForSecondRun = true;
				CursorSprite.StartBounce();
				return;
			}

			WaitForSecondRun = false;
			BattleManager.Instance.Reset();
			MainMenuManager.Instance.ReturnToTitle();
		}
	}
}
