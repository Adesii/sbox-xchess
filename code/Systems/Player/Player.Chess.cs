namespace Chess;

public enum PlayerTeam
{
	White,
	Black
}

public partial class Player
{
	[Net]
	public PlayerTeam Team { get; set; }

	[Net]
	public bool DebugFly { get; set; } = false;

	[ClientInput]
	public Ray MouseRay { get; set; }

	[Net, Predicted]
	public ChessPiece SelectedPiece { get; set; }

	[SkipHotload]
	private IList<MoveInfo> CachedMoves { get; set; }
	[SkipHotload]
	public ChessPiece CachedMovesPiece { get; set; }

	ChessboardAssignmentComponent ChessboardAssignment => Client.GetAssignment();

	Chessboard CurrentBoard => ChessboardAssignment?.Chessboard;



	public void SimulateChess( IClient cl )
	{
		if ( ChessboardAssignment == null || !ChessboardAssignment.Chessboard.IsValid() || !cl.HasTurn() ) return;
		Log.Info( $"Simulating chess for {cl}" );
		//trace mouseray and see if we hit a chesspiece
		Plane chessplane = new( CurrentBoard.Position, CurrentBoard.Rotation.Up );
		var position = chessplane.Trace( MouseRay );
		if ( position.HasValue )
		{
			DebugOverlay.Sphere( position.Value, 10, Color.Red, 0 );
			var tile = CurrentBoard.GetChessTile( position.Value );

			bool wantsToattack = false;
			if ( SelectedPiece.IsValid() )
			{
				if ( CachedMoves is not null )
				{
					foreach ( var move in CachedMoves )
					{
						if ( Game.IsServer )
							DebugOverlay.Sphere( CurrentBoard.ToWorld( move.To ), 20, move.IsEnemy ? Color.Red : Color.Green, 0 );
						if ( move.IsEnemy && tile.IsValid() && move.To == tile.MapPosition )
							wantsToattack = true;
					}

				}

				//Log.Info( $"Possible moves: {CachedMoves?.Count}" );
				DebugOverlay.Sphere( SelectedPiece.Position, 40, Color.Blue, 0 );
			}
			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{

				if ( tile != null && tile.CurrentPiece.IsValid() && !wantsToattack && tile.CurrentPiece.Team == Team )
				{
					SelectedPiece = tile.CurrentPiece;
					SelectedPiece.Owner = this;
					SelectedPiece.Predictable = true;
					CachedMoves = SelectedPiece.MoveComponent.GetPossibleMoves( new() { CheckForMovedCheck = true } );
					CachedMovesPiece = SelectedPiece;

				}
				else if ( SelectedPiece.IsValid() )
				{
					if ( CachedMoves is not null )
					{

						foreach ( var move in CachedMoves )
						{
							if ( !tile.IsValid() ) break;
							if ( move.To == tile.MapPosition )
							{
								SelectedPiece.MoveComponent.MoveTo( move );
								SelectedPiece = null;
								CachedMoves = null;
								CachedMovesPiece = null;
								Client.EndTurn();
								return;
							}
						}
						SelectedPiece = null;
					}
					else
					{
						Log.Info( "No cached moves" );
					}
				}


			}

			if ( Input.Pressed( InputButton.SecondaryAttack ) )
			{
				SelectedPiece = null;
				CachedMoves = null;
				CachedMovesPiece = null;
			}

		}
	}

}
