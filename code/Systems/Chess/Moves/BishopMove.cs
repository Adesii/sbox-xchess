namespace Chess;

[Prefab]
public class BishopMove : ChessMoveComponent
{
	public override List<MoveInfo> GetPossibleMoves( MoveSearchRequest request = default )
	{
		List<MoveInfo> moves = new();
		//make a queen move for now but stop at the first enemy
		var listofDirections = new List<Vector2Int>(){
			new Vector2Int(1,1),
			new Vector2Int(-1,1),
			new Vector2Int(-1,-1),
			new Vector2Int(1,-1),
		};
		foreach ( var dir in listofDirections )
		{
			var current = Entity.MapPosition;
			if ( request.OverrideFrom )
			{
				current = request.From;
			}

			var foundking = false;
			if ( request.CheckForCheckOverlap )
			{
				moves.Clear();
			}
			while ( true )
			{
				current += dir;
				var tile = Chessboard.Instance.GetTile( current );
				if ( tile is null )
					break;
				var code = ClassifyMove( request, current, tile, ref moves );

				if ( request.CheckForCheckOverlap )
				{
					if ( code == ReturnCode.Check )
					{
						foundking = true;
						break;
					}
				}

				if ( code == ReturnCode.Break || code == ReturnCode.Check )
					break;
				if ( code == ReturnCode.Return )
					return moves;

			}

			if ( request.CheckForCheckOverlap && foundking )
			{
				moves = moves.Where( x => x.To == request.CheckForCheckOverlapPosition ).ToList();
				//Log.Info( $"Found king at {request.CheckForCheckOverlapPosition}" );
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
