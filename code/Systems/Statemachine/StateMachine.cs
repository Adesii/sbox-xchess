namespace Chess.StateSystem;

public partial class StateMachine : Entity
{
	[Net]
	public IDictionary<string, BaseState> States { get; set; }

	public Chessboard Gamemode => Parent as Chessboard;

	[Net, Predicted]
	private BaseState _CurrentState { get; set; }

	public BaseState CurrentState
	{
		get
		{
			return _CurrentState;
		}
		protected set
		{
			if ( _CurrentState.IsValid() )
			{
				_CurrentState.OnExit();
			}
			_CurrentState = value;
			if ( _CurrentState.IsValid() )
			{
				_CurrentState.StateMachine = this;
				_CurrentState.Parent = this;
				_CurrentState.OnEnter();
			}
		}
	}

	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
		CurrentState?.Simulate( cl );
	}

	[Event.Tick.Server]
	public virtual void Tick()
	{
		CurrentState?.OnTick();
		CurrentState?.CheckSwitchState();
	}

	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );
	}

	public virtual void SetState<T>() where T : BaseState
	{
		SetState( typeof( T ).Name );
	}

	public virtual void SetState( string name )
	{
		if ( States.ContainsKey( name ) )
		{
			CurrentState = States[name];
		}
		else if ( Game.IsServer )
		{
			var state = TypeLibrary.Create<BaseState>( name );
			state.Parent = this;
			state.StateMachine = this;
			States.Add( name, state );

			CurrentState = state;
		}
	}

	public virtual void SetState( BaseState state )
	{
		if ( !state.IsValid() )
			return;

		state.StateMachine = this;
		state.Parent = this;
		States.TryAdd( CurrentState.GetType().Name, CurrentState );

		CurrentState = state;
	}

	public virtual void OnGamemodeStart() { }

	public virtual void OnGamemodeEnd() { }

	protected virtual void PreSpawnEntities<T>() where T : StateMachine
	{
		if ( Game.IsClient )
			return;

		var predictStates = TypeLibrary.GetTypes<PredictedBaseState<T>>();

		foreach ( var item in predictStates )
		{
			CacheState( item.Name );
		}
	}

	private void CacheState( string name )
	{
		if ( States.ContainsKey( name ) )
			return;

		var entity = TypeLibrary.Create<BaseState>( name );
		entity.Parent = this;
		entity.StateMachine = this;

		States.Add( name, entity );
	}
}
