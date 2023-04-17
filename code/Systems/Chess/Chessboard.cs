using System.Collections.Generic;
using Chess.StateSystem;
using Chess.Systems.Chess;
using Editor;

namespace Chess;

public partial class Chessboard : Entity, IStateMachine<TurnStateMachine>
{

	public static List<Chessboard> AllBoards = new();

	[Net]
	public TurnStateMachine StateMachine { get; set; }

	[Net]
	public IList<IClient> Players { get; set; }


	public override void Spawn()
	{
		base.Spawn();
		Transmit = TransmitType.Always;
		StateMachine = new()
		{
			Parent = this
		};

		AllBoards.Add( this );
	}

	[ConCmd.Server]
	public static void ClearBoard()
	{
		if ( ConsoleSystem.Caller.Components.Get<ChessboardAssignmentComponent>() is not ChessboardAssignmentComponent assignment )
			return;

		assignment.Chessboard.Clear();
	}

	protected virtual void Clear()
	{
		var alltiles = Entity.All.OfType<ChessTile>();
		foreach ( var tile in alltiles )
		{
			tile.Delete();
		}
		var allpieces = Entity.All.OfType<ChessPiece>();
		foreach ( var piece in allpieces )
		{
			piece.Delete();
		}
	}


	[Event.Tick.Server]
	public static void Draw()
	{
		foreach ( var board in AllBoards )
		{
			board.DebugDraw();
		}
	}
	public virtual void DebugDraw()
	{
		Log.Info( "Drawing board" );
	}

	public virtual Transform GetTransformForTeam( PlayerTeam team ) { return new(); }

	public override void Simulate( IClient cl )
	{
		StateMachine.Simulate( cl );
	}



	[ConCmd.Server( "Move" )]
	public static void MakeMove( string from, string to )
	{
		if ( ConsoleSystem.Caller.Components.Get<ChessboardAssignmentComponent>() is not ChessboardAssignmentComponent assignment )
			return;
		var board = assignment.Chessboard;
		var fromPos = DecodeMove( from.ToUpper() );
		var toPos = DecodeMove( to.ToUpper() );
		board.Move( fromPos, toPos );
	}

	public static Vector2Int DecodeMove( string letternumber )
	{
		//decode A1 to 0,0
		//decode H8 to 7,7
		Vector2Int result = new Vector2Int();
		result.x = letternumber[0] - 'A';
		result.y = letternumber[1] - '1';
		return result;
	}

	public virtual void Move( Vector2Int from, Vector2Int to ) { }

	public virtual void Move( ChessPiece entity, Vector2Int goal ) { }

	public virtual void CreateBoard() { }

	public virtual void SetPiece( Vector2Int position, ChessPiece piece ) { }

	public virtual ChessTile GetChessTile( Vector3 value ) { return null; }

	public virtual Vector3 ToWorld( Vector2Int goal )
	{
		var tile = GetTile( goal );
		if ( tile == null )
		{
			return Vector3.Zero;
		}
		return tile.Position;
	}

	public virtual ChessTile GetTile( Vector2Int goal ) { return null; }

	public virtual void AddPlayer( IClient client )
	{
		if ( Players.Contains( client ) )
			return;
		if ( client.GetAssignment() is not ChessboardAssignmentComponent assignment )
			return;
		Players.Add( client );
		assignment.Chessboard = this;
		assignment.Team = Players.Count % 2 == 0 ? PlayerTeam.White : PlayerTeam.Black;

		assignment.Ready = true;

	}

	public virtual void PlacePiecesFromAN( string fen )
	{
	}
}
