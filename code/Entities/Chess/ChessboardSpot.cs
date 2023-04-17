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

	public override void Spawn()
	{
		base.Spawn();
		Chessboard = new ClassicBoard()
		{
			Transform = Transform,
		};
	}

	public void AddPlayer( IClient client )
	{
		if ( Chessboard.IsValid() )
		{
			Chessboard.AddPlayer( client );
		}
	}
}
