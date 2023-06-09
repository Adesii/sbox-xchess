namespace Chess.StateSystem;

[Library]
public class TurnState : BaseState<TurnStateMachine>
{
	public override void CheckSwitchState()
	{
		if ( StateMachine.TurnFinished )
		{
			StateMachine.TurnFinished = false;
			StateMachine.SetState<WaitState>();
		}
	}
}
