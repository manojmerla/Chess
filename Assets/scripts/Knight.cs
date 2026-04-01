using UnityEngine;
using System.Collections.Generic;

public class Knight : Piece
{
    public override List<Vector2Int> GetLegalMoves(BoardManager board, GameManager game)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        Vector2Int[] deltas = new Vector2Int[]
        {
            new Vector2Int(2, 1), new Vector2Int(2, -1),
            new Vector2Int(-2, 1), new Vector2Int(-2, -1),
            new Vector2Int(1, 2), new Vector2Int(1, -2),
            new Vector2Int(-1, 2), new Vector2Int(-1, -2)
        };

        foreach (Vector2Int delta in deltas)
        {
            Vector2Int target = boardPos + delta;
            if (target.x >= 0 && target.x < 8 && target.y >= 0 && target.y < 8)
            {
                if (board.IsTileEmpty(target))
                {
                    moves.Add(target);
                }
                else if (board.IsTileEnemy(target, color))
                {
                    moves.Add(target);
                }
            }
        }

        return moves;
    }
}