
namespace Chess;
public partial class DefaultHud : Panel
{
	public static DefaultHud Instance { get; set; }

	public DefaultHud()
	{
		Instance = this;
	}
	public Panel CurrentGameHud { get; set; }
	public Panel CurrentLobbyPanel { get; set; }


	public void SetNewGameHud( Panel panel )
	{
		Game.AssertClient();
		CurrentGameHud?.DeleteChildren();
		CurrentGameHud.AddChild( panel );
	}

	public void SetNewLobbyPanel( Panel panel )
	{
		Game.AssertClient();
		CurrentLobbyPanel?.DeleteChildren();
		CurrentLobbyPanel.AddChild( panel );
	}

}
