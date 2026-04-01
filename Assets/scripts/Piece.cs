using UnityEngine;
using System.Collections.Generic;

public enum PieceColor { White, Black }

public class Piece : MonoBehaviour
{
    public PieceColor color;
    public Vector2Int boardPos;
    public BoardManager board;

    // Track if the piece has moved (useful for AI and castling)
    public bool hasMoved = false;

    public virtual List<Vector2Int> GetLegalMoves(BoardManager board, GameManager game)
    {
        return new List<Vector2Int>();
    }

    public virtual void MoveTo(Vector2Int newPos)
    {
        // Remove piece from old tile
        board.tiles[boardPos.x, boardPos.y].GetComponent<Tile>().currentPiece = null;

        // Update board position
        boardPos = newPos;

        // Move piece visually
        transform.position = board.tiles[newPos.x, newPos.y].transform.position + Vector3.up * 0.5f;

        // Place piece on new tile
        board.tiles[newPos.x, newPos.y].GetComponent<Tile>().currentPiece = this;

        // Mark as moved
        hasMoved = true;
    }
}
