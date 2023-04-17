namespace Chess.StateSystem;

public static class StateMachineExtensions
{
	public static StateMachine GetStateMachine( this IClient cl )
	{
		return cl.GetBoard()?.GetStateMachine<StateMachine>();
	}

	public static StateMachine GetStateMachine( this Chessboard gm )
	{
		return gm.GetStateMachine<StateMachine>();
	}

	public static T GetStateMachine<T>( this IClient cl ) where T : StateMachine
	{
		return cl.GetBoard()?.GetStateMachine<T>();
	}

	public static T GetStateMachine<T>( this Chessboard gm ) where T : StateMachine
	{
		if ( gm is IStateMachine<T> gamemode )
			return gamemode.StateMachine;
		if ( gm.StateMachine.IsValid() )
			return gm.StateMachine as T;
		return null;
	}
}
