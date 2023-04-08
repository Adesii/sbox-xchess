using Chess.Systems.Chess;
using Editor;

namespace Chess;

[HammerEntity]
public partial class Chessboard : Entity
{
	public static Chessboard Instance => ChessGame.Instance.Chessboard;
	[Net, Property]
	public int Size { get; set; } = 8;

	[Net, Property]
	public float TileSize { get; set; } = 64.0f;

	[Net]
	public IDictionary<Vector2Int, ChessTile> Map { get; set; }
	[Net]
	public LastMove LastMove { get; set; }

	public Dictionary<PlayerTeam, ChessPiece> Kings
	{
		get
		{
			if ( _kings == null )
			{
				GatherPieces();
			}
			return _kings;
		}
	}

	private Dictionary<PlayerTeam, ChessPiece> _kings;

	private Dictionary<PlayerTeam, List<ChessPiece>> Pieces { get; set; }
	public IEnumerable<ChessPiece> GetEnemyPieces( ChessPiece piece )
	{
		if ( Pieces == null || Pieces.Count == 0 )
		{
			GatherPieces();
			Log.Info( "Gathered pieces" + Pieces.Count );
		}
		return Pieces.Where( x => x.Key != piece.Team ).SelectMany( x => x.Value ).Where( x => x.IsValid() );
	}

	public IEnumerable<ChessPiece> GetTeamPieces( ChessPiece piece )
	{
		if ( Pieces == null || Pieces.Count == 0 )
		{
			GatherPieces();
			Log.Info( "Gathered pieces" + Pieces.Count );
		}
		return Pieces[piece.Team].Where( x => x != piece && x.IsValid() );
	}



	private void GatherPieces()
	{
		Pieces = new Dictionary<PlayerTeam, List<ChessPiece>>();
		foreach ( var piece in Entity.All.OfType<ChessPiece>() )
		{
			if ( !Pieces.ContainsKey( piece.Team ) )
			{
				Pieces.Add( piece.Team, new List<ChessPiece>() );
			}
			Pieces[piece.Team].Add( piece );
		}

		_kings = new Dictionary<PlayerTeam, ChessPiece>();
		foreach ( var piece in Entity.All.OfType<ChessPiece>().Where( x => x.MoveComponent is KingMove ) )
		{
			Kings.Add( piece.Team, piece );
		}
	}

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}

	[ConCmd.Server]
	public static void RecreateChessboard()
	{
		ClearBoard();
		var board = Instance;
		board.CreateBoard();
		board.PlacePiecesFromAN( "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR" );
		board.Pieces?.Clear();
	}

	[ConCmd.Server]
	public static void ClearBoard()
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
	public void CreateBoard()
	{
		Map.Clear();
		/* for ( int x = 0; x < Size; x++ )
		{
			for ( int y = 0; y < Size; y++ )
			{
				var tile = new ChessTile
				{
					Transform = Transform
				};
				tile.Position -= new Vector3( Size / 2, Size / 2, 0 ) * TileSize;
				tile.Position += new Vector3( x, y, 0 ) * TileSize;
				tile.CurrentPiece = null;
				tile.MapPosition = new Vector2Int( x, y );
				//black is brown
				tile.Color = (x + y) % 2 == 0 ? Color.White : Color.FromBytes( 0x8B, 0x45, 0x13 );
				Map.Add( tile.MapPosition, tile );
			}
		} */

		// Invert the initial board so it's white on the bottom
		for ( int y = Size - 1; y >= 0; y-- )
		{
			for ( int x = 0; x < Size; x++ )
			{
				var tile = new ChessTile
				{
					Transform = Transform
				};
				tile.Position += new Vector3( Size / 2, Size / 2, 0 ) * TileSize;
				tile.Position -= new Vector3( x, y, 0 ) * TileSize;
				if ( Size % 2 == 0 )
					tile.Position -= new Vector3( TileSize / 2, TileSize / 2, 0 );

				tile.CurrentPiece = null;
				tile.MapPosition = new Vector2Int( x, y );
				tile.Color = (x + y) % 2 == 0 ? Color.White : Color.FromBytes( 0x8B, 0x45, 0x13 );
				Map.Add( tile.MapPosition, tile );
			}
		}




	}

	private void SetPiece( Vector2Int position, ChessPiece piece )
	{
		if ( Map.TryGetValue( position, out ChessTile value ) )
		{
			value.CurrentPiece = piece;
		}
		else
		{
			Log.Error( $"Position {position} does not exist on the board" );
		}
	}

	[ConCmd.Server( "CreateBoardFromAN" )]
	public static void CreateBoardFromAN( string fen )
	{
		ClearBoard();
		var board = Instance;
		board.CreateBoard();
		board.PlacePiecesFromAN( fen );
	}
	public void PlacePiecesFromAN( string fenString )
	{
		string[] parts = fenString.Split( ' ' );

		string[] rows = parts[0].Split( '/' );
		for ( int y = 0; y < rows.Length; y++ )
		{
			string row = rows[y];
			int x = 0;
			foreach ( char c in row )
			{
				if ( char.IsDigit( c ) )
				{
					x += (int)char.GetNumericValue( c );
				}
				else
				{
					ChessPiece piece = ChessPieceLibraryHelper.GetPiece( c );
					if ( piece != null )
					{
						// adjust the x and y coordinates based on the board's orientation
						int adjustedX = x;
						int adjustedY = Size - 1 - y;
						SetPiece( new Vector2Int( adjustedX, adjustedY ), piece );
					}
					x++;
				}
			}
		}
	}


	[Event.Tick.Server]
	public void DebugDraw()
	{
		if ( Map == null )
		{
			return;
		}
		foreach ( var item in Map )
		{
			item.Value?.DebugDraw();
		}
	}

	public Transform GetTransformForTeam( PlayerTeam team )
	{
		Transform final = Transform;
		float white = team == PlayerTeam.White ? 1 : -1;
		var TeamPosition = Transform.Position;
		TeamPosition += new Vector3( 0, white * ((Size / 2) * TileSize), 600 );
		final.Position = TeamPosition;
		final.Rotation = Rotation.LookAt( Transform.Position - final.Position );
		return final;
	}

	public ChessTile GetTile( Vector2Int goal )
	{
		if ( Map.TryGetValue( goal, out ChessTile value ) )
		{
			return value;
		}
		return null;
	}

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

	public void Move( Vector2Int from, Vector2Int to )
	{
		var tile = GetTile( from );
		if ( tile == null )
		{
			Log.Error( $"Tile {from} does not exist on the board" );
			return;
		}
		if ( tile.CurrentPiece == null )
		{
			Log.Error( $"Tile {from} does not have a piece" );
			return;
		}
		Move( tile.CurrentPiece, to );
	}

	public void Move( ChessPiece entity, Vector2Int goal )
	{
		var oldTile = GetTile( entity.MapPosition );
		if ( oldTile == null )
		{
			Log.Error( $"Tile {entity.MapPosition} does not exist on the board" );
			return;
		}
		oldTile.CurrentPiece = null;
		var tile = GetTile( goal );
		if ( tile == null )
		{
			Log.Error( $"Tile {goal} does not exist on the board" );
			return;
		}
		if ( tile.CurrentPiece != null )
		{
			if ( !Game.IsClient )
				tile.CurrentPiece.Delete();
		}
		tile.CurrentPiece = entity;


	}

	public ChessPiece GetChessPieceAt( Vector3 value )
	{
		return GetChessTile( value )?.CurrentPiece;
	}

	public ChessTile GetChessTile( Vector3 value )
	{
		var localposition = Transform.PointToLocal( value );
		var index = new Vector2Int(
			Size - (localposition.x / (TileSize - 1)),
			Size - (localposition.y / (TileSize - 1)) );

		index -= new Vector2Int( Size / 2, Size / 2 );

		var tile = GetTile( index );
		if ( !tile.IsValid() )
		{
			return null;
		}
		DebugOverlay.Sphere( tile.Position, 10, Color.Red, 0, true );
		return tile;
	}

	public bool IsTileFree( Vector2Int goal )
	{
		var tile = GetTile( goal );
		if ( tile == null )
		{
			return false;
		}
		return tile.CurrentPiece == null;
	}

	public Vector3 ToWorld( Vector2Int goal )
	{
		var tile = GetTile( goal );
		if ( tile == null )
		{
			return Vector3.Zero;
		}
		return tile.Position;
	}

	public bool IsInBounds( Vector2Int goal )
	{
		return goal.x >= 0 && goal.x < Size && goal.y >= 0 && goal.y < Size;
	}
}
