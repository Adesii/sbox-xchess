namespace Chess;

public partial class ChessTile : Entity
{
	public ChessPiece CurrentPiece
	{
		get
		{
			return _CurrentPiece;
		}
		set
		{
			_CurrentPiece = value;
			if ( _CurrentPiece.IsValid() && Game.IsServer )
			{
				_CurrentPiece.Transform = Transform;
				_CurrentPiece.Rotation = Rotation.LookAt( _CurrentPiece.Team == PlayerTeam.White ? Vector3.Right : Vector3.Left );
				_CurrentPiece.MapPosition = MapPosition;
				//Log.Info( $"Placed piece at {MapPosition} with team {_CurrentPiece.Team}" );
			}
		}
	}
	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}
	[Net]
	private ChessPiece _CurrentPiece { get; set; }

	public Color Color { get; set; }
	public Vector2Int MapPosition { get; set; }

	public virtual void DebugDraw()
	{
	}
}
