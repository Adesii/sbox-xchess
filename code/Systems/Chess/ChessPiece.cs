namespace Chess;

[Prefab]
public partial class ChessPiece : Entity
{
	[Net]
	public Chessboard CurrentBoard { get; set; }
	[Net]
	public PlayerTeam Team { get; set; }

	[Net]
	public Vector2Int MapPosition { get; set; }

	[BindComponent]
	public ClassicChessMoveComponent MoveComponent { get; }

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
		Predictable = true;
		if ( MoveComponent == null )
		{
			Components.Create<ClassicChessMoveComponent>();
			Log.Warning( $"Piece {this} has no move component, creating one" );
		}
	}

	public void UpdateGlow()
	{
		foreach ( var item in Children.OfType<ModelEntity>() )
		{
			var glowcomponent = item.Components.GetOrCreate<Glow>();
			glowcomponent.Color = Team == PlayerTeam.White ? Color.White : Color.FromBytes( 139, 69, 19 );
			glowcomponent.Width = 0.25f;

		}

	}
}
