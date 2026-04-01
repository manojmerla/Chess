using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public GameObject squarePrefab;
    public Transform boardParent;
    public Transform piecesParent;
    public float tileSize = 1f;

    public GameObject[,] tiles = new GameObject[8, 8];

    [Header("White Pieces")]
    public GameObject whitePawnPrefab, whiteRookPrefab, whiteKnightPrefab,
                      whiteBishopPrefab, whiteQueenPrefab, whiteKingPrefab;

    [Header("Black Pieces")]
    public GameObject blackPawnPrefab, blackRookPrefab, blackKnightPrefab,
                      blackBishopPrefab, blackQueenPrefab, blackKingPrefab;

    void Start()
    {
        GenerateBoard();
        PlaceInitialPieces();
    }

    void GenerateBoard()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0, y * tileSize);
                GameObject t = Instantiate(squarePrefab, pos, Quaternion.identity, boardParent);
                t.name = $"Tile_{x}_{y}";
                tiles[x, y] = t;

                if (t.GetComponent<Tile>() == null)
                    t.AddComponent<Tile>();
                
                Tile tile = t.GetComponent<Tile>();
                tile.boardPos = new Vector2Int(x, y);

                var rend = t.GetComponent<Renderer>();
                if (rend != null)
                    rend.material.color = ((x + y) % 2 == 0) ? Color.white : Color.gray;
            }
        }
    }

    void PlaceInitialPieces()
    {
        for (int x = 0; x < 8; x++)
        {
            PlacePiece(whitePawnPrefab, x, 1);
            PlacePiece(blackPawnPrefab, x, 6);
        }

        PlacePiece(whiteRookPrefab, 0, 0);
        PlacePiece(whiteKnightPrefab, 1, 0);
        PlacePiece(whiteBishopPrefab, 2, 0);
        PlacePiece(whiteQueenPrefab, 3, 0);
        PlacePiece(whiteKingPrefab, 4, 0);
        PlacePiece(whiteBishopPrefab, 5, 0);
        PlacePiece(whiteKnightPrefab, 6, 0);
        PlacePiece(whiteRookPrefab, 7, 0);

        PlacePiece(blackRookPrefab, 0, 7);
        PlacePiece(blackKnightPrefab, 1, 7);
        PlacePiece(blackBishopPrefab, 2, 7);
        PlacePiece(blackQueenPrefab, 3, 7);
        PlacePiece(blackKingPrefab, 4, 7);
        PlacePiece(blackBishopPrefab, 5, 7);
        PlacePiece(blackKnightPrefab, 6, 7);
        PlacePiece(blackRookPrefab, 7, 7);
    }

    void PlacePiece(GameObject prefab, int x, int y)
    {
        Vector3 worldPos = new Vector3(x * tileSize, 0.5f, y * tileSize);
        GameObject piece = Instantiate(prefab, worldPos, Quaternion.identity, piecesParent);
        
        Piece p = piece.GetComponent<Piece>();
        if (p != null)
        {
            p.board = this;
            p.boardPos = new Vector2Int(x, y);
            
            Tile tile = tiles[x, y].GetComponent<Tile>();
            tile.currentPiece = p;
        }
    }

    public bool IsTileEmpty(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= 8 || pos.y < 0 || pos.y >= 8) return false;
        Tile tile = tiles[pos.x, pos.y].GetComponent<Tile>();
        return tile.currentPiece == null;
    }

    public bool IsTileEnemy(Vector2Int pos, PieceColor myColor)
    {
        if (pos.x < 0 || pos.x >= 8 || pos.y < 0 || pos.y >= 8) return false;
        Tile tile = tiles[pos.x, pos.y].GetComponent<Tile>();
        return tile.currentPiece != null && tile.currentPiece.color != myColor;
    }
}