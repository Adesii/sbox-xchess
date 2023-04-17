using Chess.Addons.Classic;
using Sandbox;

namespace Chess;

[Prefab]
public partial class KingMove : ClassicChessMoveComponent
{
	[Net]
	public bool IsCurrentlyChecked { get; set; }

	[Net]
	public IList<ChessPiece> PiecesChecking { get; set; } = new List<ChessPiece>();

	public override List<MoveInfo> GetPossibleMoves( MoveSearchRequest request = default )
	{
		if ( request.CheckCheck ) return new List<MoveInfo>();
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
			var tile = CurrentBoard.GetTile( current );
			if ( tile is null )
				continue;
			if ( tile.CurrentPiece.IsValid() )
			{
				if ( tile.CurrentPiece.Team != Entity.Team )
				{
					if ( request.CheckCheck )
					{
						continue;
					}
					//check if the piece is Protected

					bool protectedpiece = false;
					foreach ( var piece in _EnemyPiecesOnboard.Where( x => x.MoveComponent is not KingMove ) )
					{
						var moves2 = piece.MoveComponent.GetPossibleMoves( new MoveSearchRequest() { CheckProtection = true } );
						if ( moves2.Any( x => x.To == current ) )
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
			moves.Add( new MoveInfo() { To = current } );
		}

		//castling
		if ( !HasMoved && !IsCurrentlyChecked )

			//do a loop for each side
			foreach ( var dir in new List<Vector2Int>() { new Vector2Int( 1, 0 ), new Vector2Int( -1, 0 ) } )
			{

				var current = Entity.MapPosition;
				while ( true )
				{
					current += dir;
					var tile = CurrentBoard.GetTile( current );
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
		IsCurrentlyChecked = IsInCheck();
		moves = moves.Where( x => !WouldBeInCheck( x.To ) ).ToList();

		if ( IsCurrentlyChecked )
			moves = moves.Where( x => !WouldRemainInCheck( x.To ) ).ToList();
		//Checkmate
		if ( moves.Count == 0 && Game.IsServer )
		{
			if ( IsCurrentlyChecked )
			{
				Log.Info( "Possible Checkmate" );

				//Check all other team pieces to see if they can block the check
				bool canBlock = false;
				foreach ( var piece in CurrentBoard.GetTeamPieces( Entity ) )
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




		//Log.Info( "King has " + moves.Count + " moves" );

		return moves;
	}

	private bool WouldRemainInCheck( Vector2Int position )
	{
		foreach ( var item in PiecesChecking )
		{
			if ( item.MoveComponent.GetPossibleMoves( new() { IgnorePiece = Entity } ).Any( x => x.To == position ) )
				return true;
		}
		return false;
	}

	public bool IsInCheck()
	{

		return WouldBeInCheck( Entity.MapPosition, true );
	}




	[Event( "Chess.Castling" )]
	public void Castling( ClassicChessMoveComponent component, MoveInfo CurrentMove )
	{
		Log.Info( $"Castling {component} from: {CurrentMove.From} to {CurrentMove.To}" );
		if ( component != this ) return;
		Log.Info( "Castling" );
		var direction = (CurrentMove.To - CurrentMove.From).Normal;
		var rook = CurrentBoard.GetTile( Entity.MapPosition + direction );
		if ( !rook.IsValid() )
		{
			Log.Error( "Rook not found at " + (Entity.MapPosition - direction) + "" );
			return;
		}
		CurrentBoard.Move( rook.CurrentPiece, direction + CurrentMove.From );
	}

	[Event( "Chess.PostGlobalMove" )]
	public void PostGlobalMove( ClassicChessMoveComponent ComponentThatMoved, MoveInfo CurrentMove )
	{

		/* if ( IsCurrentlyChecked )
			GetPossibleMoves();

		IsCurrentlyChecked = IsInCheck(); */
	}

}
