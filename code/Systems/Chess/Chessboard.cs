using Chess.Systems.Chess;
using Editor;

namespace Chess;

public partial class Chessboard : Entity
{
	public static Chessboard Instance => ChessGame.Instance.Chessboard;


	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}

	[ConCmd.Server]
	public static void ClearBoard()
	{
		Instance?.Clear();
	}

	protected virtual void Clear()
	{
		var alltiles = Entity.All.OfType<ChessTile>();
		foreach ( var tile in alltiles )
		{
			tile.Delete();
		}
		var allpieces = Entity.All.OfType<ChessPiece>();
		foreach ( var piece in allpieces )
		{
			piece.Delete();
		}
	}


	[Event.Tick.Server]

	private static void Draw()
	{
		Instance?.DebugDraw();
	}
	public virtual void DebugDraw()
	{

	}

	public virtual Transform GetTransformForTeam( PlayerTeam team ) { return new(); }



	[ConCmd.Server( "Move" )]
	public static void MakeMove( string from, string to )
	{
		var board = Instance;
		var fromPos = DecodeMove( from.ToUpper() );
		var toPos = DecodeMove( to.ToUpper() );
		board.Move( fromPos, toPos );
	}

	public static Vector2Int DecodeMove( string letternumber )
	{
		//decode A1 to 0,0
		//decode H8 to 7,7
		Vector2Int result = new Vector2Int();
		result.x = letternumber[0] - 'A';
		result.y = letternumber[1] - '1';
		return result;
	}

	public virtual void Move( Vector2Int from, Vector2Int to ) { }

	public virtual void Move( ChessPiece entity, Vector2Int goal ) { }

	public virtual void CreateBoard() { }

	public virtual void SetPiece( Vector2Int position, ChessPiece piece ) { }

	public virtual ChessTile GetChessTile( Vector3 value ) { return null; }

	public virtual Vector3 ToWorld( Vector2Int goal )
	{
		var tile = GetTile( goal );
		if ( tile == null )
		{
			return Vector3.Zero;
		}
		return tile.Position;
	}

	public virtual ChessTile GetTile( Vector2Int goal ) { return null; }
}
