namespace Chess;

public partial class LastMove : BaseNetworkable
{

	[Net]
	public Vector2Int Start { get; set; }
	[Net]
	public Vector2Int Goal { get; set; }
	[Net]
	public ChessPiece PieceMoved { get; set; }
}
