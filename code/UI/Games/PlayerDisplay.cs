namespace Chess;

public partial class PlayerDisplay
{
	public IClient Player { get; set; }
	public Image Avatar { get; set; }
	public Label Ready { get; set; }

	public string IsReady => Player.GetAssignment().Ready ? "Ready" : "Not Ready";

	public override void Tick()
	{
		if ( Avatar.IsValid() )
		{
			Avatar.Texture = Texture.LoadAvatar( Player.Client.SteamId );
		}
		if ( Ready.IsValid() )
		{
			Ready.SetClass( "Ready", Player.GetAssignment()?.Ready ?? false );
		}
	}

	protected override int BuildHash()
	{
		return HashCode.Combine( Player.SteamId, IsReady );
	}
}
