namespace Chess;

public class ChessComponent<T> : EntityComponent<ChessPiece>, IBoardComponent<T> where T : Chessboard
{
	public T CurrentBoard => Entity.CurrentBoard as T;
}
