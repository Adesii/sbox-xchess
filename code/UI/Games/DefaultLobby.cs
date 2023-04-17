using Sandbox;
using Sandbox.UI;

namespace Chess;
public partial class DefaultLobby : Panel
{
	public ChessboardSpot ChessboardSpot { get; set; }

	public DefaultLobby( ChessboardSpot spot )
	{
		ChessboardSpot = spot;
	}

	protected override int BuildHash()
	{
		return ChessboardSpot?.GetHashCode() + ChessboardSpot?.Players?.Count + base.BuildHash() ?? base.BuildHash();
	}

}
