namespace Chess;

public struct MoveInfo
{
	public Vector2Int From;
	public Vector2Int To;
	public bool IsEnemy;
	public bool IsAlly;

	public string EventToCall;

	public override string ToString()
	{
		return $"From: {From}, To: {To}, IsEnemy: {IsEnemy}, IsAlly: {IsAlly}, EventToCall: {EventToCall}";
	}
}

public struct MoveSearchRequest
{
	public Vector2Int From;
	public bool CheckNewKingPosition;
	public Vector2Int KingPosition;
	public bool OverrideFrom;
	public bool CheckCheck;
	public bool CheckProtection;

	public bool SimulateMove;
	public Vector2Int SimulatedPosition;

	public bool CheckForCheckOverlap;
	public Vector2Int CheckForCheckOverlapPosition;

	public bool CheckForMovedCheck;

	public ChessPiece IgnorePiece;
	public ChessPiece FakePiece;
	public Vector2Int FakePiecePosition;


	public override string ToString()
	{
		return $"From: {From}, CheckNewKingPosition: {CheckNewKingPosition}, KingPosition: {KingPosition}, OverrideFrom: {OverrideFrom}, CheckCheck: {CheckCheck}, CheckProtection: {CheckProtection}, SimulateMove: {SimulateMove}, SimulatedPosition: {SimulatedPosition}";
	}
}

