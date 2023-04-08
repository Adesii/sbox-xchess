namespace Chess;

[Prefab]
public class KnightMove : ChessMoveComponent
{

	public override List<MoveInfo> GetPossibleMoves( bool CheckCheck = false, bool CheckMate = false )
	{
		List<MoveInfo> moves = new List<MoveInfo>();
		var listofDirections = new List<Vector2Int>(){
			new Vector2Int(1,2),
			new Vector2Int(2,1),
			new Vector2Int(2,-1),
			new Vector2Int(1,-2),
			new Vector2Int(-1,-2),
			new Vector2Int(-2,-1),
			new Vector2Int(-2,1),
			new Vector2Int(-1,2),
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
					if ( !CheckMate | !IsKing( tile.CurrentPiece ) )
						moves.Add( new MoveInfo() { To = current, IsEnemy = true } );
				}
				if ( !CheckMate )
					continue;
			}
			moves.Add( new MoveInfo() { To = current } );
		}

		if ( !CheckCheck && KingIsInCheck() )
		{
			moves = moves.Where( x => PositionSavesKing( x.To ) ).ToList();
		}
		return moves;
	}
}
