namespace Chess;
public interface IBoardComponent
{
}

public interface IBoardComponent<T> where T : Chessboard
{
	T CurrentBoard { get; }
}
