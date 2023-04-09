namespace Chess;

[Prefab]
public class RookMove : ChessMoveComponent
{

	public override List<MoveInfo> GetPossibleMoves( MoveSearchRequest request = default )
	{

		List<MoveInfo> moves = new List<MoveInfo>();
		//make a queen move for now but stop at the first enemy
		var listofDirections = new List<Vector2Int>(){
			new Vector2Int(1,0),
			new Vector2Int(-1,0),
			new Vector2Int(0,1),
			new Vector2Int(0,-1),
		};

		foreach ( var dir in listofDirections )
		{
			var current = Entity.MapPosition;
			while ( true )
			{
				current += dir;
				var tile = Chessboard.Instance.GetTile( current );
				if ( tile is null )
					break;
				var code = ClassifyMove( request, current, tile, ref moves );

				if ( code == ReturnCode.Break )
					break;
				if ( code == ReturnCode.Return )
					return moves;
			}
		}


		if ( KingIsInCheck() )
		{
			moves = moves.Where( x => PositionSavesKing( x ) ).ToList();
		}
		else if ( request.CheckForMovedCheck )
		{
			moves = moves.Where( x => !WouldPutKingInCheckIfMovedTo( x ) ).ToList();
		}
		return moves;
	}
}
