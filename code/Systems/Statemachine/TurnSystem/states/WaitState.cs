namespace Chess.StateSystem;

[Library]
public partial class WaitState : PredictedBaseState<TurnStateMachine>
{
	[Net, Predicted]
	public TimeSince CreationTime { get; set; }

	public override void OnEnter()
	{
		CreationTime = 0;
	}

	public override void CheckSwitchState()
	{
		if ( CreationTime > 0 )
		{
			StateMachine.TurnIndex += 1 % StateMachine.TurnOrder.Count;
			StateMachine.TurnFinished = false;
			StateMachine.SetState<TurnState>();
		}
	}

	public override void OnTick()
	{
		DebugOverlay.ScreenText( $"{(Game.IsClient ? "[CL]" : "[SV]")}WaitState: {CreationTime}", (Game.IsClient ? 0 : 1) );
	}
}

