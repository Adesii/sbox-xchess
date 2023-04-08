namespace Chess;

[Prefab]
public class RookMove : ChessMoveComponent
{

	public override List<MoveInfo> GetPossibleMoves( bool CheckCheck = false, bool CheckMate = false )
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
				if ( tile.CurrentPiece.IsValid() )
				{
					if ( CheckMate || tile.CurrentPiece.Team != Entity.Team )
					{
						if ( CheckMate || !IsKing( tile.CurrentPiece ) )
							moves.Add( new MoveInfo() { To = current, IsEnemy = true } );
					}
					if ( !CheckMate )
						break;
				}
				moves.Add( new MoveInfo() { To = current } );
			}
		}


		if ( !CheckCheck && KingIsInCheck() )
		{
			moves = moves.Where( x => PositionSavesKing( x.To ) ).ToList();
		}
		return moves;
	}

	public override bool BlocksCheck( Vector2Int position, Vector2Int kingPosition )
	{
		List<MoveInfo> moves = new List<MoveInfo>();
		//make a queen move for now but stop at the first enemy
		var listofDirections = new List<Vector2Int>(){
			new Vector2Int(1,0),
			new Vector2Int(-1,0),
			new Vector2Int(0,1),
			new Vector2Int(0,-1),
		};
		bool kingFound = false;
		foreach ( var dir in listofDirections )
		{
			if ( kingFound )
				break;
			var current = Entity.MapPosition;
			while ( true )
			{
				current += dir;
				if ( current == kingPosition )
				{
					kingFound = true;
					break;
				}
				if ( current == position )
				{
					return true;
				}
				var tile = Chessboard.Instance.GetTile( current );
				if ( tile is null )
					break;
				if ( tile.CurrentPiece.IsValid() )
				{
					break;
				}
			}
		}

		if ( kingFound )
		{
			moves = moves.Where( x => x.To == position ).ToList();
		}
		else
		{
			return false;
		}
		return moves.Count > 0;

	}
}
