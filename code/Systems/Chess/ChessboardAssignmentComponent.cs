namespace Chess;

public partial class ChessboardAssignmentComponent : EntityComponent, ISingletonComponent
{
	[Net]
	public Chessboard Chessboard { get; set; }

	[Net]
	public bool Ready { get; set; }

	[Net]
	public PlayerTeam Team { get; set; }
}
