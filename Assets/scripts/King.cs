using UnityEngine;
using System.Collections.Generic;

public class King : Piece
{
    public override List<Vector2Int> GetLegalMoves(BoardManager board, GameManager game)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1,0), new Vector2Int(-1,0),
            new Vector2Int(0,1), new Vector2Int(0,-1),
            new Vector2Int(1,1), new Vector2Int(-1,1),
            new Vector2Int(1,-1), new Vector2Int(-1,-1)
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int target = boardPos + dir;
            if (target.x >= 0 && target.x <= 7 && target.y >= 0 && target.y <= 7)
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

        Debug.Log(color + " King legal moves: " + moves.Count);
        return moves;
    }

    public override void MoveTo(Vector2Int newPos)
    {
        base.MoveTo(newPos);
        Debug.Log(color + " King moved to: " + newPos);
    }
}