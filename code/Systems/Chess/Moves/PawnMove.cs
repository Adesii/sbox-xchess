namespace Chess;

[Prefab]
public partial class PawnMove : ChessMoveComponent
{
	[Net]
	public bool DoubleJumped { get; set; } = false;

	[Net]
	public Vector2Int StartPosition { get; set; }


	int TeamAmount => Entity.Team == PlayerTeam.White ? 1 : -1;
	public override List<MoveInfo> GetPossibleMoves( bool CheckCheck = false, bool CheckMate = false )
	{
		List<MoveInfo> moves = new List<MoveInfo>();

		//make a queen move for now but stop at the first enemy
		if ( !HasMoved )
		{
			var nottile = Chessboard.Instance.GetTile( Entity.MapPosition + new Vector2Int( 0, TeamAmount ) );
			if ( nottile.IsValid() && !nottile.CurrentPiece.IsValid() )
			{
				var jumptile = Chessboard.Instance.GetTile( Entity.MapPosition + new Vector2Int( 0, TeamAmount * 2 ) );
				if ( jumptile is not null && jumptile.CurrentPiece.IsValid() == false )
				{
					moves.Add( new MoveInfo() { To = Entity.MapPosition + new Vector2Int( 0, TeamAmount * 2 ) } );
				}
			}
		}
		var tile = Chessboard.Instance.GetTile( Entity.MapPosition + new Vector2Int( 0, TeamAmount ) );
		if ( tile is not null && tile.CurrentPiece.IsValid() == false )
		{
			moves.Add( new MoveInfo() { To = Entity.MapPosition + new Vector2Int( 0, TeamAmount ) } );
		}
		tile = Chessboard.Instance.GetTile( Entity.MapPosition + new Vector2Int( 1, TeamAmount ) );
		if ( tile is not null && tile.CurrentPiece.IsValid() && (CheckMate || tile.CurrentPiece.Team != Entity.Team) )
		{
			moves.Add( new MoveInfo() { To = Entity.MapPosition + new Vector2Int( 1, TeamAmount ), IsEnemy = true } );
		}
		tile = Chessboard.Instance.GetTile( Entity.MapPosition + new Vector2Int( -1, TeamAmount ) );
		if ( tile is not null && tile.CurrentPiece.IsValid() && (CheckMate || tile.CurrentPiece.Team != Entity.Team) )
		{
			moves.Add( new MoveInfo() { To = Entity.MapPosition + new Vector2Int( -1, TeamAmount ), IsEnemy = true } );
		}

		//en passant
		if ( HasMoved )
		{
			var lastMove = Chessboard.Instance.LastMove;
			if ( lastMove is not null && lastMove.PieceMoved is not null && lastMove.PieceMoved.Team != Entity.Team && lastMove.PieceMoved.MoveComponent is PawnMove pawn && pawn.DoubleJumped )
			{
				if ( lastMove.Goal.y == Entity.MapPosition.y && Math.Abs( lastMove.Goal.x - Entity.MapPosition.x ) == 1 )
				{
					moves.Add( new MoveInfo() { To = new Vector2Int( lastMove.Goal.x, Entity.MapPosition.y + TeamAmount ), IsEnemy = true, EventToCall = "Chess.EnPassant" } );
				}
			}
		}


		if ( !CheckCheck && KingIsInCheck() )
		{
			moves = moves.Where( x => PositionSavesKing( x.To ) ).ToList();
		}




		return moves;
	}

	[Event( "Chess.EnPassant" )]
	public void EnPassant( ChessMoveComponent component, MoveInfo info )
	{
		if ( component != this ) return;
		Chessboard.Instance.LastMove.PieceMoved?.Delete();
	}

	public override void MoveTo( MoveInfo goal )
	{
		if ( !HasMoved )
			StartPosition = Entity.MapPosition;
		if ( Math.Abs( StartPosition.y - goal.To.y ) == 2 )
			DoubleJumped = true;
		else
			DoubleJumped = false;
		base.MoveTo( goal );
	}
}
