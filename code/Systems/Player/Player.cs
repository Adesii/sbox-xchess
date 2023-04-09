using System;
using Chess.UI;

namespace Chess;

public enum PlayerTeam
{
	White,
	Black
}

public partial class Player : AnimatedEntity
{
	[Net]
	public PlayerTeam Team { get; set; }

	[Net]
	public bool DebugFly { get; set; } = false;

	[ClientInput]
	Angles LookInput { get; set; }
	[ClientInput]
	Vector3 LookPosition { get; set; }

	[ClientInput]
	public Ray MouseRay { get; set; }


	/// <summary>
	/// When the player is first created. This isn't called when a player respawns.
	/// </summary>
	public override void Spawn()
	{
		Predictable = true;

		// Default properties
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableLagCompensation = true;
		EnableHitboxes = true;

		Tags.Add( "player" );
	}

	public override void BuildInput()
	{
		base.BuildInput();
		var lookInput = (LookInput + Input.AnalogLook).Normal;

		// Since we're a FPS game, let's clamp the player's pitch between -90, and 90.
		LookInput = lookInput.WithPitch( lookInput.pitch.Clamp( -90f, 90f ) );
		LookPosition += Input.AnalogMove * 10 * LookInput.ToRotation();

		MouseRay = new( Camera.Position, Screen.GetDirection( Cursor.FakeMousePosition ) );
	}

	/// <summary>
	/// Called when a player respawns, think of this as a soft spawn - we're only reinitializing transient data here.
	/// </summary>
	public void Respawn()
	{
	}

	[Net, Predicted]
	public ChessPiece SelectedPiece { get; set; }

	[SkipHotload]
	private IList<ChessMoveComponent.MoveInfo> CachedMoves { get; set; }
	[SkipHotload]
	public ChessPiece CachedMovesPiece { get; set; }


	/// <summary>
	/// Called every server and client tick.
	/// </summary>
	/// <param name="cl"></param>
	public override void Simulate( IClient cl )
	{
		//trace mouseray and see if we hit a chesspiece
		Plane chessplane = new( Chessboard.Instance.Position, Chessboard.Instance.Rotation.Up );
		var position = chessplane.Trace( MouseRay );
		if ( position.HasValue )
		{
			DebugOverlay.Sphere( position.Value, 10, Color.Red, 0 );
			var tile = Chessboard.Instance.GetChessTile( position.Value );

			bool wantsToattack = false;
			if ( SelectedPiece.IsValid() )
			{
				if ( CachedMoves is not null )
				{
					foreach ( var move in CachedMoves )
					{
						if ( Game.IsServer )
							DebugOverlay.Sphere( Chessboard.Instance.ToWorld( move.To ), 20, move.IsEnemy ? Color.Red : Color.Green, 0 );
						if ( move.IsEnemy && tile.IsValid() && move.To == tile.MapPosition )
							wantsToattack = true;
					}

				}

				//Log.Info( $"Possible moves: {CachedMoves?.Count}" );
				DebugOverlay.Sphere( SelectedPiece.Position, 40, Color.Blue, 0 );
			}
			if ( Input.Pressed( InputButton.PrimaryAttack ) )
			{

				if ( tile != null && tile.CurrentPiece.IsValid() && !wantsToattack /* && tile.CurrentPiece.Team == Team  */)
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

	/// <summary>
	/// Called every frame clientside.
	/// </summary>
	/// <param name="cl"></param>
	public override void FrameSimulate( IClient cl )
	{
		if ( Chessboard.Instance is not Chessboard chessboard )
			return;

		if ( DebugFly )
		{
			Camera.Position = LookPosition;
			Camera.Rotation = LookInput.ToRotation();
			return;
		}
		var transform = chessboard.GetTransformForTeam( Team );
		Camera.Position = transform.Position;
		Camera.Rotation = transform.Rotation;
	}

}
