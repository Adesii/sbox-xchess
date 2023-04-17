namespace Chess;

public static class Debug
{
	[ConVar.Replicated( "chess_debug_level" )]
	public static int Level { get; set; } = 10;

	// @TODO: revert when ConVar.Replicated is fixed
	public static bool Enabled => true;
}

public static class LoggerExtension
{
	public static void Debug( this Logger log, object obj )
	{
		if ( !Chess.Debug.Enabled )
			return;

		log.Info( $"[{(Game.IsClient ? "CL" : "SV")}] {obj}" );
	}

	public static void Debug( this Logger log, object obj, int level )
	{
		if ( Chess.Debug.Level < level )
			return;

		log.Info( $"[{(Game.IsClient ? "CL" : "SV")}] {obj}" );
	}
}
