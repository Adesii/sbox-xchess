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

	[BindComponent]
	public Glow Glow { get; }

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
		Tick();
	}

	[Event.Hotload]
	public void Tick()
	{
		foreach ( var child in Children.OfType<ModelEntity>() )
		{
			child.Components.GetOrCreate<Glow>().Color = Team == PlayerTeam.White ? Color.White : Color.FromBytes( 0x8B, 0x45, 0x13 );
			child.Components.GetOrCreate<Glow>().Width = 0.25f;
		}
	}
}
