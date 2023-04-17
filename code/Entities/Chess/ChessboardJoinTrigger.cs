namespace Chess;
[HammerEntity]
public class ChessboardJoinTrigger : BaseTrigger
{
	[Property]
	public EntityTarget Chessboard { get; set; }

	public override void OnTouchStart( Entity toucher )
	{
		base.OnTouchStart( toucher );
		Log.Info( $"Player {toucher}" );
		if ( toucher is Player player )
		{
			var board = Chessboard.GetTarget<ChessboardSpot>();
			Log.Info( $"Board {board}" );
			if ( board != null )
			{
				board.AddPlayer( player.Client );
				//Disable();
			}
		}
	}
}
