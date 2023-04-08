namespace Chess.UI;
public partial class Cursor : Panel
{
	[Event.Client.BuildInput]
	private void buildinput()
	{
		//FakeMousePosition -= RealMouse - Sandbox.Mouse.Position;
		//FakeMousePosition = FakeMousePosition.Clamp( new Vector2( 0, 0 ), new( Screen.Width, Screen.Height ) );
		RealMouse = Sandbox.Mouse.Position;
		FakeMousePosition = RealMouse;


	}
}
