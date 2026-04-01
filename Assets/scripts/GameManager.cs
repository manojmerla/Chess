using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; 
    public PieceColor currentTurn = PieceColor.White;
    public King whiteKing, blackKing;
    private BoardManager board;
    private bool gameOver = false;
    
    [SerializeField] private string gameOverSceneName = "GameOverScene";
    public TMP_Text turnText;
    
    // King materials - use the default materials from your king pieces
    [Header("King Materials")]
    public Material whiteKingNormalMaterial;
    public Material blackKingNormalMaterial;
    public Material kingCheckMaterial; // Red material for check
    
    // Sound variables
    [Header("Sound Effects")]
    public AudioClip moveSound;
    public AudioClip captureSound;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Input.multiTouchEnabled = true;
        board = FindObjectOfType<BoardManager>();
        
        // AudioSource setup
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        FindKings();
        UpdateTurnText();
        ResetKingMaterials(); // Start with normal materials
    }

    void FindKings()
    {
        whiteKing = null;
        blackKing = null;
        
        Piece[] allPieces = FindObjectsOfType<Piece>(true);
        foreach (Piece piece in allPieces)
        {
            if (piece is King king)
            {
                if (king.color == PieceColor.White) 
                    whiteKing = king;
                else 
                    blackKing = king;
            }
        }
    }

    void UpdateTurnText()
    {
        if (turnText != null)
        {
            turnText.text = currentTurn == PieceColor.White ? "WHITE'S TURN" : "BLACK'S TURN";
            turnText.color = currentTurn == PieceColor.White ? Color.white : Color.black;
        }
    }

    // Update king materials based on check status
    void UpdateKingMaterials()
    {
        // White king - red only if in check, otherwise white
        if (whiteKing != null)
        {
            Renderer whiteRenderer = whiteKing.GetComponent<Renderer>();
            if (whiteRenderer != null)
            {
                whiteRenderer.material = IsKingInCheck(whiteKing) ? kingCheckMaterial : whiteKingNormalMaterial;
            }
        }
        
        // Black king - red only if in check, otherwise black
        if (blackKing != null)
        {
            Renderer blackRenderer = blackKing.GetComponent<Renderer>();
            if (blackRenderer != null)
            {
                blackRenderer.material = IsKingInCheck(blackKing) ? kingCheckMaterial : blackKingNormalMaterial;
            }
        }
    }

    // Reset both kings to their normal colors
    void ResetKingMaterials()
    {
        // White king gets white material
        if (whiteKing != null && whiteKingNormalMaterial != null)
        {
            Renderer whiteRenderer = whiteKing.GetComponent<Renderer>();
            if (whiteRenderer != null)
            {
                whiteRenderer.material = whiteKingNormalMaterial;
            }
        }
        
        // Black king gets black material
        if (blackKing != null && blackKingNormalMaterial != null)
        {
            Renderer blackRenderer = blackKing.GetComponent<Renderer>();
            if (blackRenderer != null)
            {
                blackRenderer.material = blackKingNormalMaterial;
            }
        }
    }

    public void PlayMoveSound(bool isCapture = false)
    {
        if (audioSource != null)
        {
            AudioClip clip = isCapture ? captureSound : moveSound;
            if (clip != null) audioSource.PlayOneShot(clip);
        }
    }

    public void EndTurn()
    {
        if (gameOver) return;

        CheckForCapturedKings();
        if (gameOver) return;

        currentTurn = currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
        UpdateTurnText();
        CheckForCheck();
        UpdateKingMaterials(); // Update king colors after turn change
    }

    public void CheckForCapturedKings()
    {
        if (whiteKing == null || blackKing == null) FindKings();

        if (whiteKing == null || !whiteKing.gameObject.activeInHierarchy)
        {
            GameOver(PieceColor.Black);
            return;
        }

        if (blackKing == null || !blackKing.gameObject.activeInHierarchy)
        {
            GameOver(PieceColor.White);
            return;
        }
    }

    public bool IsPlayersTurn(PieceColor pieceColor)
    {
        return pieceColor == currentTurn;
    }

    public void CheckForCheck()
    {
        if (gameOver) return;
        
        King currentKing = currentTurn == PieceColor.White ? whiteKing : blackKing;
        
        if (currentKing == null || !currentKing.gameObject.activeInHierarchy)
        {
            CheckForCapturedKings();
            return;
        }
        
        if (IsKingInCheck(currentKing))
        {
            Debug.Log("⚡ CHECK! " + currentTurn + " King is in danger!");
            
            if (IsKingInCheckmate(currentKing))
            {
                Debug.Log("💀 CHECKMATE! " + currentTurn + " loses the game!");
                GameOver(currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White);
            }
        }
    }

    public bool IsKingInCheck(King king)
    {
        if (king == null || !king.gameObject.activeInHierarchy) return true;

        Piece[] allPieces = FindObjectsOfType<Piece>(true);
        foreach (Piece piece in allPieces)
        {
            if (piece.color != king.color && piece.gameObject.activeInHierarchy)
            {
                List<Vector2Int> moves = piece.GetLegalMoves(board, this);
                if (moves.Contains(king.boardPos))
                    return true;
            }
        }
        return false;
    }

    public bool IsKingInCheckmate(King king)
    {
        if (king == null || !king.gameObject.activeInHierarchy) return true;
        if (!IsKingInCheck(king)) return false;

        // Check if king can move to safety
        List<Vector2Int> kingMoves = king.GetLegalMoves(board, this);
        foreach (Vector2Int move in kingMoves)
        {
            if (!WouldMoveCauseCheck(king, move))
                return false;
        }

        // Check if check can be blocked or attacker captured
        Piece attacker = FindAttackingPiece(king);
        if (attacker != null)
        {
            if (CanPieceBeCaptured(attacker, king.color)) return false;
            if (CanAttackBeBlocked(attacker, king)) return false;
        }

        return true;
    }

    private Piece FindAttackingPiece(King king)
    {
        Piece[] allPieces = FindObjectsOfType<Piece>(true);
        foreach (Piece piece in allPieces)
        {
            if (piece.color != king.color && piece.gameObject.activeInHierarchy)
            {
                List<Vector2Int> moves = piece.GetLegalMoves(board, this);
                if (moves.Contains(king.boardPos))
                    return piece;
            }
        }
        return null;
    }

    private bool CanPieceBeCaptured(Piece targetPiece, PieceColor defenderColor)
    {
        Piece[] allPieces = FindObjectsOfType<Piece>(true);
        foreach (Piece piece in allPieces)
        {
            if (piece.color == defenderColor && piece.gameObject.activeInHierarchy)
            {
                List<Vector2Int> moves = piece.GetLegalMoves(board, this);
                if (moves.Contains(targetPiece.boardPos))
                    return true;
            }
        }
        return false;
    }

    private bool CanAttackBeBlocked(Piece attacker, King king)
    {
        if (attacker is Knight) return false;

        List<Vector2Int> squaresBetween = GetSquaresBetween(attacker.boardPos, king.boardPos);
        
        Piece[] allPieces = FindObjectsOfType<Piece>(true);
        foreach (Piece piece in allPieces)
        {
            if (piece.color == king.color && piece.gameObject.activeInHierarchy)
            {
                List<Vector2Int> moves = piece.GetLegalMoves(board, this);
                foreach (Vector2Int move in moves)
                {
                    if (squaresBetween.Contains(move))
                        return true;
                }
            }
        }
        return false;
    }

    private List<Vector2Int> GetSquaresBetween(Vector2Int from, Vector2Int to)
    {
        List<Vector2Int> squares = new List<Vector2Int>();
        Vector2Int direction = new Vector2Int(
            Mathf.Clamp(to.x - from.x, -1, 1),
            Mathf.Clamp(to.y - from.y, -1, 1)
        );

        Vector2Int current = from + direction;
        while (current != to)
        {
            squares.Add(current);
            current += direction;
        }
        return squares;
    }

    private bool WouldMoveCauseCheck(Piece piece, Vector2Int targetPos)
    {
        Tile targetTile = board.tiles[targetPos.x, targetPos.y].GetComponent<Tile>();
        Piece originalTargetPiece = targetTile.currentPiece;
        Vector2Int originalPos = piece.boardPos;

        piece.boardPos = targetPos;
        targetTile.currentPiece = piece;
        if (originalTargetPiece != null) originalTargetPiece.gameObject.SetActive(false);

        bool inCheck = IsKingInCheck(piece.color == PieceColor.White ? whiteKing : blackKing);

        piece.boardPos = originalPos;
        targetTile.currentPiece = originalTargetPiece;
        if (originalTargetPiece != null) originalTargetPiece.gameObject.SetActive(true);

        return inCheck;
    }

    public void GameOver(PieceColor winner)
    {
        if (gameOver) return;
        gameOver = true;
        
        string winnerName = winner.ToString();
        PlayerPrefs.SetString("Winner", winnerName);
        PlayerPrefs.Save();

        ResetKingMaterials(); // Reset to normal colors before game over

        if (!string.IsNullOrEmpty(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }

        // Disable all pieces
        Piece[] allPieces = FindObjectsOfType<Piece>(true);
        foreach (Piece piece in allPieces) piece.enabled = false;
    }

    public bool IsGameOver()
    {
        return gameOver;
    }
}