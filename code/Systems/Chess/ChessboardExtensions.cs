namespace Chess;

public static class ChessboardExtensions
{
	public static ChessboardAssignmentComponent GetAssignment( this IClient cl )
	{
		if ( Game.IsServer )
			return cl.Components.GetOrCreate<ChessboardAssignmentComponent>();
		return cl.Components.Get<ChessboardAssignmentComponent>();
	}

	public static Chessboard GetBoard( this IClient cl )
	{
		return GetAssignment( cl )?.Chessboard;
	}

	public static bool IsInBoard( this IClient cl )
	{
		return GetBoard( cl ).IsValid();
	}
}
