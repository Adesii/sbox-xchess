namespace Chess;

public partial class ChessMoveComponent : ChessComponent
{

	protected IEnumerable<ChessPiece> _EnemyPiecesOnboard => Chessboard.Instance.GetEnemyPieces( Entity );
	public struct MoveInfo
	{
		public Vector2Int From;
		public Vector2Int To;
		public bool IsEnemy;

		public string EventToCall;
	}

	[Net]
	public bool HasMoved { get; set; }

	public virtual List<MoveInfo> GetPossibleMoves( bool CheckCheck = false, bool CheckMate = false )
	{
		List<MoveInfo> moves = new List<MoveInfo>();
		return moves;
	}

	public virtual void MoveTo( MoveInfo goal )
	{
		HasMoved = true;
		goal.From = Entity.MapPosition;
		Chessboard.Instance.Move( Entity, goal.To );
		if ( goal.EventToCall != null )
			Event.Run( goal.EventToCall, this, goal );
		Chessboard.Instance.LastMove = new LastMove()
		{
			Start = Entity.MapPosition,
			Goal = goal.To,
			PieceMoved = Entity
		};


		Event.Run( "Chess.PostGlobalMove", this, goal );
	}

	public bool IsKing( ChessPiece piece )
	{
		return piece.MoveComponent is KingMove;
	}

	public bool KingIsInCheck()
	{
		return Chessboard.Instance.Kings[Entity.Team].MoveComponent is KingMove king && king.IsCurrentlyChecked;
	}

	public bool PositionSavesKing( Vector2Int position )
	{
		var king = Chessboard.Instance.Kings[Entity.Team];
		var kingMove = king.MoveComponent as KingMove;
		int checksBlocked = 0;
		Dictionary<ChessPiece, int> blocksByPiece = new Dictionary<ChessPiece, int>();

		// Iterate over all checking pieces
		foreach ( var checkingPiece in kingMove.PiecesChecking.Where( x => x.IsValid() ) )
		{
			// Get the possible moves for the checking piece
			var checkingPieceMove = checkingPiece.MoveComponent as ChessMoveComponent;
			var checkingPieceMoves = checkingPieceMove.GetPossibleMoves( true );

			// Check if the checking piece can be blocked by any piece at the given position
			bool canBlock = false;
			foreach ( var move in checkingPieceMoves )
			{
				if ( move.To == position && checkingPieceMove.BlocksCheck( position, king.MapPosition ) )
				{
					canBlock = true;
					if ( blocksByPiece.ContainsKey( checkingPiece ) )
					{
						blocksByPiece[checkingPiece]++;
					}
					else
					{
						blocksByPiece[checkingPiece] = 1;
					}
					break;
				}
			}

			if ( !canBlock )
			{
				// If the checking piece cannot be blocked, check if the king can capture it
				if ( kingMove.BlocksCheck( checkingPiece.MapPosition, king.MapPosition ) )
				{
					checksBlocked++;
				}
				else
				{
					// If the checking piece cannot be blocked or captured, the king is in checkmate
					return false;
				}
			}
		}

		// Check if all checks can be blocked by the same piece
		if ( blocksByPiece.Count == 1 && blocksByPiece.First().Value == kingMove.PiecesChecking.Count )
		{
			return true;
		}

		// Check if the checks can be blocked by a combination of moves from different pieces
		var pieces = blocksByPiece.Keys.ToList();
		for ( int i = 1; i <= pieces.Count; i++ )
		{
			var combinations = GetCombinations( pieces, i );
			foreach ( var combination in combinations )
			{
				int count = 0;
				foreach ( var piece in combination )
				{
					count += blocksByPiece[piece];
				}
				if ( count == kingMove.PiecesChecking.Count )
				{
					return true;
				}
			}
		}

		// If none of the above conditions are met, the king is in checkmate
		return false;
	}

	protected bool WouldBeInCheck( Vector2Int position )
	{
		foreach ( var piece in _EnemyPiecesOnboard.Where( x => x.MoveComponent is not KingMove ) )
		{
			if ( piece.MoveComponent.GetPossibleMoves( true ).Any( x => x.To == position ) )
			{
				return true;
			}
		}
		return false;
	}

	protected bool WouldBeInCheckMate( Vector2Int position )
	{
		foreach ( var piece in _EnemyPiecesOnboard.Where( x => x.MoveComponent is not KingMove ) )
		{
			if ( piece.MoveComponent.GetPossibleMoves( true, true ).Any( x => x.To == position ) )
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool BlocksCheck( Vector2Int position, Vector2Int kingPosition )
	{
		return false;
	}


	private static IEnumerable<IEnumerable<T>> GetCombinations<T>( IEnumerable<T> elements, int k )
	{
		return k == 0 ? new[] { new T[0] } :
		  elements.SelectMany( ( e, i ) =>
			GetCombinations( elements.Skip( i + 1 ), k - 1 ).Select( c => (new[] { e }).Concat( c ) ) );
	}

}


