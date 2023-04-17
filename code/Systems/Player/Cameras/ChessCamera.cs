namespace Chess;

public class ChessCamera : PlayerCamera
{
	public override void Update( Player player )
	{
		if ( player.Client.GetBoard() is Chessboard board )
		{
			Transform boardTransform = board.GetTransformForTeam( player.Team );
			Camera.Position = boardTransform.Position;
			Camera.Rotation = boardTransform.Rotation;
			Camera.FieldOfView = 90;

		}
		else
		{
			base.Update( player );
		}
	}
}
