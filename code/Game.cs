﻿using Chess.Addons.Classic;
using Chess.Systems.Chess;

namespace Chess;


public partial class ChessGame : GameManager
{
	public static ChessGame Instance => (ChessGame)Current;
	public ChessGame()
	{
		if ( Game.IsServer )
		{
			_ = new Hud();
		}
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
		foreach ( var board in Chessboard.AllBoards )
		{
			board?.Simulate( cl );
		}
	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new Player();
		client.Pawn = pawn;
		pawn.Respawn();

		// Get all of the spawnpoints
		var spawnpoints = Entity.All.OfType<SpawnPoint>();

		// chose a random one
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		// if it exists, place the pawn there
		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 10.0f; // raise it up
			pawn.Transform = tx;
		}

		Chat.AddChatEntry( To.Everyone, client.Name, "joined the game", client.SteamId, true );
	}

	public override void ClientDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( client, reason );
		Chat.AddChatEntry( To.Everyone, client.Name, "left the game", client.SteamId, true );
	}

	public override void DoPlayerDevCam( IClient client )
	{
		if ( client.Pawn is not Player player )
			return;
		player.DebugFly = !player.DebugFly;
	}



	[ConCmd.Server]
	public static void StartGame()
	{
		if ( ConsoleSystem.Caller.GetBoard() is not Chessboard board )
			return;
		board.StateMachine.OnGamemodeStart();
	}
	[ConCmd.Server]
	public static void EndGame()
	{
		if ( ConsoleSystem.Caller.GetBoard() is not Chessboard board )
			return;
		board.StateMachine.OnGamemodeEnd();
	}

	[ConCmd.Server]
	public static void EndTurn()
	{
		if ( ConsoleSystem.Caller.GetBoard() is not Chessboard board )
			return;
		board.StateMachine.EndTurn();
	}
}
