namespace Chess.Systems.Chess;

public enum ChessPieceType
{
	Pawn,
	Rook,
	Knight,
	Bishop,
	Queen,
	King
}
public static class ChessPieceLibraryHelper
{
	public static ChessPiece GetPiece( ChessPieceType type, PlayerTeam color )
	{
		var piecePrefab = PrefabLibrary.Spawn<ChessPiece>( $"/prefabs/{type.ToString().ToLower()}.prefab" );
		piecePrefab.Team = color;
		return piecePrefab;
	}

	public static ChessPiece GetPiece( char c )
	{
		var type = ChessPieceType.Pawn;
		var color = PlayerTeam.White;
		switch ( c )
		{
			case 'P':
				type = ChessPieceType.Pawn;
				color = PlayerTeam.White;
				break;
			case 'R':
				type = ChessPieceType.Rook;
				color = PlayerTeam.White;
				break;
			case 'N':
				type = ChessPieceType.Knight;
				color = PlayerTeam.White;
				break;
			case 'B':
				type = ChessPieceType.Bishop;
				color = PlayerTeam.White;
				break;
			case 'Q':
				type = ChessPieceType.Queen;
				color = PlayerTeam.White;
				break;
			case 'K':
				type = ChessPieceType.King;
				color = PlayerTeam.White;
				break;
			case 'p':
				type = ChessPieceType.Pawn;
				color = PlayerTeam.Black;
				break;
			case 'r':
				type = ChessPieceType.Rook;
				color = PlayerTeam.Black;
				break;
			case 'n':
				type = ChessPieceType.Knight;
				color = PlayerTeam.Black;
				break;
			case 'b':
				type = ChessPieceType.Bishop;
				color = PlayerTeam.Black;
				break;
			case 'q':
				type = ChessPieceType.Queen;
				color = PlayerTeam.Black;
				break;
			case 'k':
				type = ChessPieceType.King;
				color = PlayerTeam.Black;
				break;
		}
		return GetPiece( type, color );

	}
}
