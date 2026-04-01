using UnityEngine;
using System.Collections.Generic;

public class Rook : Piece
{
    public override List<Vector2Int> GetLegalMoves(BoardManager board, GameManager game)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1,0), new Vector2Int(-1,0),
            new Vector2Int(0,1), new Vector2Int(0,-1)
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int target = boardPos;
            while (true)
            {
                target += dir;
                if (target.x < 0 || target.x > 7 || target.y < 0 || target.y > 7)
                    break;

                if (board.IsTileEmpty(target))
                {
                    moves.Add(target);
                }
                else if (board.IsTileEnemy(target, color))
                {
                    moves.Add(target);
                    break;
                }
                else
                {
                    break;
                }
            }
        }

        return moves;
    }
}