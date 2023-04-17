using Chess.UI;

namespace Chess;

public partial class Hud : HudEntity<RootPanel>
{
	public Hud()
	{
		if ( !Game.IsClient )
			return;

		RootPanel.StyleSheet.Load( "/UI/Hud.scss" );
		RootPanel.AddChild<Chat>();
		RootPanel.AddChild<Players>();
		RootPanel.AddChild<DefaultHud>();

		RootPanel.AddChild<Cursor>();
	}

	[ClientRpc]
	public static void ShowLobby( int spotid )
	{
		DefaultHud.Instance.SetNewLobbyPanel( new DefaultLobby( Entity.FindByIndex( spotid ) as ChessboardSpot ) );
	}
}
