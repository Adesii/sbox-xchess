using Chess.Addons.Classic;
namespace Chess;

public partial class ClassicChessMoveComponent : ChessComponent<ClassicBoard>
{

	protected IEnumerable<ChessPiece> _EnemyPiecesOnboard => CurrentBoard.GetEnemyPieces( Entity );


	[Net]
	public bool HasMoved { get; set; }

	public virtual List<MoveInfo> GetPossibleMoves( MoveSearchRequest request = default )
	{
		List<MoveInfo> moves = new List<MoveInfo>();
		return moves;
	}

	public virtual void MoveTo( MoveInfo goal )
	{
		HasMoved = true;
		goal.From = Entity.MapPosition;
		CurrentBoard.Move( Entity, goal.To );
		if ( goal.EventToCall != null )
			Event.Run( goal.EventToCall, this, goal );
		CurrentBoard.LastMove = new LastMove()
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
		return CurrentBoard.Kings[Entity.Team].MoveComponent is KingMove king && king.IsCurrentlyChecked;
	}

	public bool PositionSavesKing( MoveInfo info )
	{
		var king = CurrentBoard.Kings[Entity.Team];
		var kingMove = king.MoveComponent as KingMove;
		int checksBlocked = 0;
		Dictionary<ChessPiece, int> blocksByPiece = new Dictionary<ChessPiece, int>();
		var simulatedKingPosition = king.MapPosition;

		// Iterate over all checking pieces
		foreach ( var checkingPiece in kingMove.PiecesChecking.Where( x => x.IsValid() ) )
		{
			// Get the possible moves for the checking piece
			var checkingPieceMove = checkingPiece.MoveComponent as ClassicChessMoveComponent;
			var checkingPieceMoves = checkingPieceMove.GetPossibleMoves( new() { SimulateMove = true, SimulatedPosition = info.To } );

			// Check if the checking piece can be blocked by any piece at the given position
			bool canBlock = false;
			foreach ( var move in checkingPieceMoves )
			{
				if ( move.To == info.To && checkingPieceMove.BlocksCheck( info.To, simulatedKingPosition ) || (checkingPieceMove.Entity.MapPosition == info.To) )
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

				// If the checking piece cannot be blocked or captured, the king is in checkmate
				return false;

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
		if ( checksBlocked == kingMove.PiecesChecking.Count )
		{
			return true;
		}
		else if ( checksBlocked == 1 && blocksByPiece.Count == 0 )
		{
			return false;
		}
		else
		{
			return false;
		}
	}

	protected bool WouldBeInCheck( Vector2Int position, bool addpieces = false )
	{
		if ( addpieces && CurrentBoard.Kings[Entity.Team].MoveComponent is KingMove kingmove )
			kingmove.PiecesChecking.Clear();

		bool ischecking = false;
		foreach ( var piece in _EnemyPiecesOnboard.Where( x => x.MoveComponent is not KingMove && x.IsValid() ) )
		{
			MoveSearchRequest request = new() { CheckNewKingPosition = true, KingPosition = position };
			if ( piece.MoveComponent.GetPossibleMoves( request ).Any( x => x.To == position ) )
			{
				if ( addpieces && CurrentBoard.Kings[Entity.Team].MoveComponent is KingMove kingmoves )
				{
					kingmoves.PiecesChecking.Add( piece );
					Log.Info( "Added piece to checking list" );
				}
				ischecking = true;
				continue;
			}

		}
		return ischecking;
	}

	public virtual bool BlocksCheck( Vector2Int position, Vector2Int kingPosition )
	{
		if ( position == Entity.MapPosition ) return true;
		return GetPossibleMoves( new() { CheckForCheckOverlap = true, CheckForCheckOverlapPosition = position, CheckCheck = true } ).Any();
	}

	protected enum ReturnCode
	{
		Continue,
		Break,
		Return,
		Check
	}

	protected virtual ReturnCode ClassifyMove( MoveSearchRequest request, Vector2Int Current, ChessTile Tile, ref List<MoveInfo> moves )
	{

		/* if ( request.SimulateMove )
		{
			if ( Current == request.SimulatedPosition || Entity.MapPosition == request.SimulatedPosition )
			{
				Log.Info( request );
				moves.Clear();
				moves.Add( new MoveInfo() { To = Current } );
				Log.Info( "Simulated move would be invalid" );
				return ReturnCode.Return;
			}
		} */
		if ( request.CheckNewKingPosition )
		{
			if ( Current == request.KingPosition )
			{
				moves.Clear();
				moves.Add( new MoveInfo() { To = Current } );
				//Log.Info( $"King would be in check at: {request.KingPosition} by piece: {Entity}" );
				return ReturnCode.Check;
			}

		}
		if ( Tile.CurrentPiece.IsValid() )
		{
			if ( request.CheckCheck && IsKing( Tile.CurrentPiece ) )
			{
				if ( !request.CheckForCheckOverlap )
					moves.Clear();
				moves.Add( new MoveInfo() { To = Current, IsEnemy = true } );
				//Log.Info( "King is in check" );
				if ( request.IgnorePiece != Tile.CurrentPiece )
					return ReturnCode.Check;
			}
			if ( request.CheckProtection && Tile.CurrentPiece.Team == Entity.Team && Tile.CurrentPiece != Entity )
			{
				moves.Add( new MoveInfo() { To = Current, IsAlly = true } );
				return ReturnCode.Break;
			}
			else if ( Tile.CurrentPiece.Team == Entity.Team )
			{
				return ReturnCode.Break;
			}
			if ( Tile.CurrentPiece.Team != Entity.Team )
			{
				if ( !IsKing( Tile.CurrentPiece ) )
					moves.Add( new MoveInfo() { To = Current, IsEnemy = true } );
				if ( request.IgnorePiece != Tile.CurrentPiece )
					return ReturnCode.Break;
			}
		}
		if ( request.FakePiece.IsValid() && request.FakePiecePosition == Current )
		{
			if ( request.CheckCheck && IsKing( request.FakePiece ) )
			{
				if ( !request.CheckForCheckOverlap )
					moves.Clear();
				moves.Add( new MoveInfo() { To = Current, IsEnemy = true } );
				//Log.Info( "King is in check" );
				return ReturnCode.Check;
			}
			if ( request.CheckProtection && request.FakePiece.Team == Entity.Team && request.FakePiece != Entity )
			{
				moves.Add( new MoveInfo() { To = Current, IsAlly = true } );
				return ReturnCode.Break;
			}
			else if ( request.FakePiece.Team == Entity.Team )
			{
				return ReturnCode.Break;
			}
			if ( request.FakePiece.Team != Entity.Team )
			{
				if ( !IsKing( request.FakePiece ) )
					moves.Add( new MoveInfo() { To = Current, IsEnemy = true } );
				return ReturnCode.Break;
			}
		}
		moves.Add( new MoveInfo() { To = Current } );
		return ReturnCode.Continue;

	}


	private static IEnumerable<IEnumerable<T>> GetCombinations<T>( IEnumerable<T> elements, int k )
	{
		return k == 0 ? new[] { Array.Empty<T>() } :
		  elements.SelectMany( ( e, i ) =>
			GetCombinations( elements.Skip( i + 1 ), k - 1 ).Select( c => (new[] { e }).Concat( c ) ) );
	}


	public bool WouldPutKingInCheckIfMovedTo( MoveInfo position )
	{
		if ( CurrentBoard.Kings[Entity.Team].MoveComponent is KingMove kingmove )
		{
			foreach ( var item in CurrentBoard.GetEnemyPieces( Entity ) )
			{
				var moves = item.MoveComponent.GetPossibleMoves( new() { CheckCheck = true, IgnorePiece = Entity, FakePiece = Entity, FakePiecePosition = position.To } );
				if ( moves.Any( x => x.To == kingmove.Entity.MapPosition ) && position.To != item.MapPosition )
				{
					//Log.Info( $"Moves: {moves.Count} {moves[0]}" );
					//Log.Info( "Would put king in check" );
					return true;
				}
			}
		}

		return false;
	}


}


