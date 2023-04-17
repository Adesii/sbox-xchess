using Chess.StateSystem;

namespace Chess;

public static class TurnManagerExtensions
{
	public static bool HasTurn( this IClient cl )
	{
		return cl.GetBoard()?.HasTurn( cl ) ?? false;
	}

	public static bool HasTurn( this Chessboard gm, IClient cl )
	{
		TurnStateMachine turnmachine = gm?.GetStateMachine<TurnStateMachine>();
		if ( turnmachine == null )
			return false;
		return turnmachine.CurrentState.GetType() == typeof( TurnState ) && turnmachine?.CurrentTurn == cl;
	}

	private static TurnStateMachine GetTurnStateMachine( this IClient cl )
	{
		return cl.GetBoard()?.GetStateMachine<TurnStateMachine>();
	}

	public static void EndTurn( this IClient cl )
	{
		if ( !cl.HasTurn() )
			return;
		cl.GetTurnStateMachine().EndTurn();
	}
}
