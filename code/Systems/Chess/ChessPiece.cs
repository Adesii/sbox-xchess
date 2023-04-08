namespace Chess;

[Prefab]
public partial class ChessPiece : Entity
{
	[Net]
	public PlayerTeam Team { get; set; }

	[Net]
	public Vector2Int MapPosition { get; set; }

	[BindComponent]
	public ChessMoveComponent MoveComponent { get; }

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
		Predictable = true;
		if ( MoveComponent == null )
		{
			Components.Create<ChessMoveComponent>();
			Log.Warning( $"Piece {this} has no move component, creating one" );
		}
	}
}
