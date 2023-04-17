using Chess.StateSystem;

namespace Chess;

public partial class TurnStateMachine : StateMachine
{

	[Net]
	public IList<IClient> TurnOrder { get; set; }

	[Net, Predicted]
	public bool TurnFinished { get; set; } = false;
	public IClient CurrentTurn
	{
		get
		{
			if ( TurnOrder == null || TurnOrder.Count == 0 )
				return null;
			if ( TurnIndex >= TurnOrder.Count )
			{
				TurnIndex = 0;
			}
			return TurnOrder[TurnIndex];
		}
	}

	[Net, Predicted] public int TurnIndex { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		PreSpawnEntities<TurnStateMachine>();
		SetState<LobbyState>();
	}

	public override void OnGamemodeStart()
	{
		base.OnGamemodeStart();

		TurnOrder.Clear();
		foreach ( var item in Gamemode.Players )
		{
			TurnOrder.Add( item );
		}
		TurnIndex = 0;
		SetState<WaitState>();
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
		DebugOverlay.ScreenText( $"TurnStateMachine: {CurrentTurn?.Name}, State: {CurrentState}", 15 );
	}

	public virtual void EndTurn()
	{
		TurnFinished = true;
	}

}
