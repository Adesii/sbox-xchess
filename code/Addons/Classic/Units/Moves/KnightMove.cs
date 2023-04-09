using Chess.Addons.Classic;

namespace Chess;

[Prefab]
public class KnightMove : ClassicChessMoveComponent
{

	public override List<MoveInfo> GetPossibleMoves( MoveSearchRequest request = default )
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
			var tile = ClassicBoard.Instance.GetTile( current );
			if ( tile is null )
				continue;
			var code = ClassifyMove( request, current, tile, ref moves );
			if ( code == ReturnCode.Return )
				return moves;
		}

		if ( KingIsInCheck() )
		{
			moves = moves.Where( x => PositionSavesKing( x ) ).ToList();
		}
		return moves;
	}
}
