using Chess.Addons.Classic;

namespace Chess;

public enum ChessSize
{
	Classic,
	Extended,
	ExtraExtended,
	Unlimited
};
[HammerEntity]
public partial class ChessboardSpot : Entity
{
	[Property]
	public ChessSize BoardSize { get; set; } = ChessSize.Classic;

	[Net]
	public Chessboard Chessboard { get; set; }

	[Net]
	public IList<IClient> Players { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}

	public void AddPlayer( IClient client )
	{
		if ( !Players.Contains( client ) )
		{
			Players.Add( client );
			Log.Info( $"Added player To lobby {client}" );
			Hud.ShowLobby( To.Single( client ), NetworkIdent );
		}
	}
}
