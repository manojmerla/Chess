using UnityEngine;
using System.Collections.Generic;

public class Pawn : Piece
{
    public override List<Vector2Int> GetLegalMoves(BoardManager board, GameManager game)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        int direction = (color == PieceColor.White) ? 1 : -1;

        Vector2Int forward = boardPos + new Vector2Int(0, direction);
        if (forward.y >= 0 && forward.y < 8 && board.IsTileEmpty(forward))
        {
            moves.Add(forward);
        }

        int startRow = (color == PieceColor.White) ? 1 : 6;
        Vector2Int forward2 = boardPos + new Vector2Int(0, 2 * direction);
        if (boardPos.y == startRow && forward2.y >= 0 && forward2.y < 8)
        {
            if (board.IsTileEmpty(forward) && board.IsTileEmpty(forward2))
            {
                moves.Add(forward2);
            }
        }

        Vector2Int[] diagonals = { new Vector2Int(-1, direction), new Vector2Int(1, direction) };
        foreach (Vector2Int diag in diagonals)
        {
            Vector2Int target = boardPos + diag;
            if (target.x >= 0 && target.x < 8 && target.y >= 0 && target.y < 8)
            {
                if (!board.IsTileEmpty(target) && board.IsTileEnemy(target, color))
                {
                    moves.Add(target);
                }
            }
        }

        return moves;
    }
}