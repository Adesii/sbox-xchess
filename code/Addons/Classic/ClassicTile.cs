namespace Chess.Addons.Classic;

public class ClassicTile : ChessTile<ClassicBoard>
{
	public override void DebugDraw()
	{
		var Letter = (char)('A' + MapPosition.x);
		var Number = MapPosition.y + 1;
		//DebugOverlay.Text( $"{Letter}{Number}", Position, Color );
		float boxoffsetsize = 3f;
		DebugOverlay.Box( Position - ((Vector3)CurrentBoard.TileSize / 2 - (Vector3)boxoffsetsize).WithZ( 0 ), Position + ((Vector3)CurrentBoard.TileSize / 2 - boxoffsetsize).WithZ( 2 ), Color );
	}
}
