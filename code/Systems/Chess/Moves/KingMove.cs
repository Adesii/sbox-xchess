using Sandbox;

namespace Chess;

[Prefab]
public partial class KingMove : ChessMoveComponent
{
	[Net]
	public bool IsCurrentlyChecked { get; set; }

	[Net]
	public List<ChessPiece> PiecesChecking { get; set; } = new List<ChessPiece>();

	public override List<MoveInfo> GetPossibleMoves( bool CheckCheck = false, bool CheckMate = false )
	{
		if ( CheckCheck ) return new List<MoveInfo>();
		List<MoveInfo> moves = new List<MoveInfo>();
		var listofDirections = new List<Vector2Int>(){
			new Vector2Int(1,0),
			new Vector2Int(1,1),
			new Vector2Int(0,1),
			new Vector2Int(-1,1),
			new Vector2Int(-1,0),
			new Vector2Int(-1,-1),
			new Vector2Int(0,-1),
			new Vector2Int(1,-1),
		};

		foreach ( var dir in listofDirections )
		{
			var current = Entity.MapPosition;
			current += dir;
			var tile = Chessboard.Instance.GetTile( current );
			if ( tile is null )
				continue;
			if ( tile.CurrentPiece.IsValid() )
			{
				if ( CheckMate || tile.CurrentPiece.Team != Entity.Team )
				{
					if ( !CheckCheck && WouldBeInCheck( current ) )
					{
						continue;
					}
					//check if the piece is Protected

					bool protectedpiece = false;
					foreach ( var piece in _EnemyPiecesOnboard.Where( x => x.MoveComponent is not KingMove && x != tile.CurrentPiece ) )
					{
						if ( piece.MoveComponent.GetPossibleMoves( true, true ).Any( x => x.To == tile.MapPosition ) )
						{
							protectedpiece = true;
							break;
						}
					}
					if ( protectedpiece )
						continue;

					moves.Add( new MoveInfo() { To = current, IsEnemy = true } );
				}
				continue;
			}
			if ( !CheckCheck )
				if ( WouldBeInCheck( current ) )
				{
					continue;
				}
			moves.Add( new MoveInfo() { To = current } );
		}

		//castling
		if ( !HasMoved )

			//do a loop for each side
			foreach ( var dir in new List<Vector2Int>() { new Vector2Int( 1, 0 ), new Vector2Int( -1, 0 ) } )
			{

				var current = Entity.MapPosition;
				while ( true )
				{
					current += dir;
					if ( WouldBeInCheck( current ) )
					{
						Log.Info( "Can't castle because of check" );
						break;
					}
					var tile = Chessboard.Instance.GetTile( current );
					if ( tile is null )
						break;
					if ( tile.CurrentPiece.IsValid() )
					{
						if ( tile.CurrentPiece.Team == Entity.Team && tile.CurrentPiece.MoveComponent is RookMove rook && !rook.HasMoved )
						{
							moves.Add( new MoveInfo() { To = current - dir, EventToCall = "Chess.Castling" } );
							Log.Info( "CanCastle" );
						}
						break;
					}
				}

			}

		//Checkmate
		if ( moves.Count == 0 )
		{
			if ( IsInCheck() )
			{
				Log.Info( "Possible Checkmate" );

				//Check all other team pieces to see if they can block the check
				bool canBlock = false;
				foreach ( var piece in Chessboard.Instance.GetTeamPieces( Entity ) )
				{
					if ( piece.MoveComponent.GetPossibleMoves().Count > 0 )
					{
						Log.Info( "Piece can move" );
						canBlock = true;
					}
				}
				if ( !canBlock )
				{
					Log.Error( "Checkmate" );
					Event.Run( "Chess.Checkmate", Entity.Team );
				}

			}
		}

		Log.Info( "King has " + moves.Count + " moves" );

		moves = moves.Where( x => !WouldBeInCheck( x.To ) ).ToList();

		return moves;
	}

	private bool IsInCheck()
	{
		PiecesChecking.Clear();
		foreach ( var piece in _EnemyPiecesOnboard.Where( x => x.MoveComponent is not KingMove ) )
		{
			if ( piece.MoveComponent.GetPossibleMoves( true ).Any( x => x.To == Entity.MapPosition ) )
			{
				PiecesChecking.Add( piece );
				IsCurrentlyChecked = true;
				return true;
			}
		}
		IsCurrentlyChecked = false;

		Log.Info( "King is not checked" );
		return false;
	}




	[Event( "Chess.Castling" )]
	public void Castling( ChessMoveComponent component, MoveInfo CurrentMove )
	{
		Log.Info( $"Castling {component} from: {CurrentMove.From} to {CurrentMove.To}" );
		if ( component != this ) return;
		Log.Info( "Castling" );
		var direction = (CurrentMove.To - CurrentMove.From).Normal;
		var rook = Chessboard.Instance.GetTile( Entity.MapPosition + direction );
		if ( !rook.IsValid() )
		{
			Log.Error( "Rook not found at " + (Entity.MapPosition - direction) + "" );
			return;
		}
		Chessboard.Instance.Move( rook.CurrentPiece, direction + CurrentMove.From );
	}

	[Event( "Chess.PostGlobalMove" )]
	public void PostGlobalMove( ChessMoveComponent ComponentThatMoved, MoveInfo CurrentMove )
	{
		IsCurrentlyChecked = IsInCheck();
		Log.Info( "King is checked: " + IsCurrentlyChecked );

		if ( IsCurrentlyChecked )
			GetPossibleMoves();
	}

}
