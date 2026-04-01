using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ChessAI : MonoBehaviour
{
    public GameManager gameManager;
    public BoardManager board;
    public float moveDelay = 1.0f;
    
    // Piece values for smarter decision making
    private Dictionary<System.Type, int> pieceValues = new Dictionary<System.Type, int>()
    {
        { typeof(Pawn), 10 },
        { typeof(Knight), 30 },
        { typeof(Bishop), 30 },
        { typeof(Rook), 50 },
        { typeof(Queen), 90 },
        { typeof(King), 900 }
    };
    
    void Start()
    {
        gameManager = GameManager.Instance;
        board = FindObjectOfType<BoardManager>();
    }
    
    void Update()
    {
        if (gameManager.currentTurn == PieceColor.Black && !gameManager.IsGameOver() && !IsInvoking("MakeAIMove"))
        {
            Invoke("MakeAIMove", moveDelay);
        }
    }
    
    void MakeAIMove()
    {
        if (gameManager.IsGameOver())
        {
            Debug.Log("Game is already over, AI won't move");
            return;
        }
        
        Debug.Log("AI Thinking...");
        
        List<AIMove> allMoves = GetAllPossibleMoves();
        
        if (allMoves.Count == 0)
        {
            Debug.Log("AI has no valid moves! This means checkmate - White wins!");
            gameManager.GameOver(PieceColor.White);
            return;
        }

        List<AIMove> safeMoves = allMoves.Where(move => !WouldMoveCauseCheck(move)).ToList();
        
        if (safeMoves.Count == 0)
        {
            Debug.Log("No safe moves available! This means checkmate - White wins!");
            gameManager.GameOver(PieceColor.White);
            return;
        }

        // Evaluate all safe moves and choose the best one
        AIMove bestMove = EvaluateBestMove(safeMoves);
        ExecuteMove(bestMove);
    }

    List<AIMove> GetAllPossibleMoves()
    {
        List<AIMove> moves = new List<AIMove>();
        
        foreach (Piece piece in FindObjectsOfType<Piece>())
        {
            if (piece.color == PieceColor.Black && piece.gameObject.activeInHierarchy)
            {
                Vector2Int currentPos = GetPiecePosition(piece);
                List<Vector2Int> legalMoves = piece.GetLegalMoves(board, gameManager);
                
                foreach (Vector2Int move in legalMoves)
                {
                    moves.Add(new AIMove(piece, move, currentPos));
                }
            }
        }
        
        return moves;
    }

    AIMove EvaluateBestMove(List<AIMove> safeMoves)
    {
        // Prioritize moves that give check
        List<AIMove> checkMoves = safeMoves.Where(move => WouldMoveCheckOpponent(move)).ToList();
        if (checkMoves.Count > 0)
        {
            Debug.Log("AI found check move!");
            return GetHighestValueMove(checkMoves);
        }

        // Prioritize captures, especially high-value pieces
        List<AIMove> captureMoves = safeMoves.Where(move => 
            board.tiles[move.move.x, move.move.y].GetComponent<Tile>().currentPiece != null).ToList();
        
        if (captureMoves.Count > 0)
        {
            // Get the highest value capture
            AIMove bestCapture = GetHighestValueCapture(captureMoves);
            int captureValue = GetPieceValue(board.tiles[bestCapture.move.x, bestCapture.move.y].GetComponent<Tile>().currentPiece);
            int movingPieceValue = GetPieceValue(bestCapture.piece);
            
            // Only capture if it's profitable or equal exchange
            if (captureValue >= movingPieceValue || captureValue >= 30) // Don't sacrifice pieces for low-value captures
            {
                Debug.Log("AI making profitable capture!");
                return bestCapture;
            }
        }

        // If no good captures, develop pieces and control center
        return GetBestPositionalMove(safeMoves);
    }

    AIMove GetHighestValueMove(List<AIMove> moves)
    {
        AIMove bestMove = moves[0];
        int bestScore = -10000;

        foreach (AIMove move in moves)
        {
            int score = EvaluateMoveScore(move);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }
        
        return bestMove;
    }

    AIMove GetHighestValueCapture(List<AIMove> captureMoves)
    {
        AIMove bestCapture = captureMoves[0];
        int bestValue = -10000;

        foreach (AIMove move in captureMoves)
        {
            Piece targetPiece = board.tiles[move.move.x, move.move.y].GetComponent<Tile>().currentPiece;
            int captureValue = GetPieceValue(targetPiece);
            
            if (captureValue > bestValue)
            {
                bestValue = captureValue;
                bestCapture = move;
            }
        }
        
        return bestCapture;
    }

    AIMove GetBestPositionalMove(List<AIMove> safeMoves)
    {
        AIMove bestMove = safeMoves[0];
        int bestScore = -10000;

        foreach (AIMove move in safeMoves)
        {
            int score = EvaluatePositionalScore(move);
            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }
        
        return bestMove;
    }

    int EvaluateMoveScore(AIMove move)
    {
        int score = 0;
        
        // Check bonus
        if (WouldMoveCheckOpponent(move))
            score += 50;
        
        // Capture value
        Piece targetPiece = board.tiles[move.move.x, move.move.y].GetComponent<Tile>().currentPiece;
        if (targetPiece != null)
        {
            int captureValue = GetPieceValue(targetPiece);
            score += captureValue;
        }
        
        // Positional score
        score += EvaluatePositionalScore(move);
        
        // Prefer developing pieces from the back rank
        if (move.originalPosition.y == 0 && move.move.y > 0)
            score += 10;
            
        // Avoid moving the same piece repeatedly without reason
        if (move.piece.hasMoved)
            score -= 5;
        
        return score;
    }

    int EvaluatePositionalScore(AIMove move)
    {
        int score = 0;
        
        // Control center squares (prefer moves to center)
        Vector2Int targetPos = move.move;
        int centerDistance = Mathf.Abs(targetPos.x - 3) + Mathf.Abs(targetPos.y - 3);
        score += (6 - centerDistance) * 2; // Closer to center is better
        
        // Prefer moves that don't put pieces in danger
        if (!IsSquareSafeForPiece(targetPos, move.piece))
            score -= GetPieceValue(move.piece) / 2;
        
        // Bonus for castling moves (if implemented)
        if (move.piece is King && Mathf.Abs(move.originalPosition.x - targetPos.x) == 2)
            score += 40;
        
        return score;
    }

    bool IsSquareSafeForPiece(Vector2Int square, Piece piece)
    {
        // Check if any white piece can capture this square
        foreach (Piece whitePiece in FindObjectsOfType<Piece>())
        {
            if (whitePiece.color == PieceColor.White && whitePiece.gameObject.activeInHierarchy)
            {
                if (whitePiece.GetLegalMoves(board, gameManager).Contains(square))
                {
                    // If the attacking piece is less valuable, it might be okay
                    if (GetPieceValue(whitePiece) < GetPieceValue(piece))
                        continue;
                    return false;
                }
            }
        }
        return true;
    }

    int GetPieceValue(Piece piece)
    {
        if (piece == null) return 0;
        
        System.Type pieceType = piece.GetType();
        if (pieceValues.ContainsKey(pieceType))
            return pieceValues[pieceType];
        
        return 0;
    }

    Vector2Int GetPiecePosition(Piece piece)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (board.tiles[x, y].GetComponent<Tile>().currentPiece == piece)
                    return new Vector2Int(x, y);
            }
        }
        return new Vector2Int(-1, -1);
    }

    bool WouldMoveCauseCheck(AIMove move)
    {
        Tile targetTile = board.tiles[move.move.x, move.move.y].GetComponent<Tile>();
        Piece capturedPiece = targetTile.currentPiece;
        Vector2Int originalPos = move.piece.boardPos;
        
        // Apply move temporarily
        board.tiles[move.originalPosition.x, move.originalPosition.y].GetComponent<Tile>().currentPiece = null;
        targetTile.currentPiece = move.piece;
        move.piece.boardPos = move.move;
        
        bool causesCheck = IsKingInCheck(PieceColor.Black);
        
        // Undo move
        board.tiles[move.originalPosition.x, move.originalPosition.y].GetComponent<Tile>().currentPiece = move.piece;
        targetTile.currentPiece = capturedPiece;
        move.piece.boardPos = originalPos;
        
        return causesCheck;
    }

    bool WouldMoveCheckOpponent(AIMove move)
    {
        Tile targetTile = board.tiles[move.move.x, move.move.y].GetComponent<Tile>();
        Piece capturedPiece = targetTile.currentPiece;
        Vector2Int originalPos = move.piece.boardPos;
        
        // Apply move temporarily
        board.tiles[move.originalPosition.x, move.originalPosition.y].GetComponent<Tile>().currentPiece = null;
        targetTile.currentPiece = move.piece;
        move.piece.boardPos = move.move;
        
        bool checksOpponent = IsKingInCheck(PieceColor.White);
        
        // Undo move
        board.tiles[move.originalPosition.x, move.originalPosition.y].GetComponent<Tile>().currentPiece = move.piece;
        targetTile.currentPiece = capturedPiece;
        move.piece.boardPos = originalPos;
        
        return checksOpponent;
    }

    bool IsKingInCheck(PieceColor color)
    {
        Vector2Int kingPos = FindKingPosition(color);
        if (kingPos.x == -1) return false;

        foreach (Piece piece in FindObjectsOfType<Piece>())
        {
            if (piece.color != color && piece.gameObject.activeInHierarchy)
            {
                if (piece.GetLegalMoves(board, gameManager).Contains(kingPos))
                    return true;
            }
        }
        return false;
    }

    Vector2Int FindKingPosition(PieceColor color)
    {
        foreach (Piece piece in FindObjectsOfType<Piece>())
        {
            if (piece is King && piece.color == color && piece.gameObject.activeInHierarchy)
            {
                return GetPiecePosition(piece);
            }
        }
        return new Vector2Int(-1, -1);
    }

    void ExecuteMove(AIMove aiMove)
    {
        Tile targetTile = board.tiles[aiMove.move.x, aiMove.move.y].GetComponent<Tile>();
        
        bool isCapture = false;
        
        if (targetTile.currentPiece != null && targetTile.currentPiece.color == PieceColor.White)
        {
            Debug.Log("AI Capturing: " + targetTile.currentPiece.name);
            isCapture = true;
            targetTile.currentPiece.gameObject.SetActive(false);
        }
        
        board.tiles[aiMove.originalPosition.x, aiMove.originalPosition.y].GetComponent<Tile>().currentPiece = null;
        targetTile.currentPiece = aiMove.piece;
        
        aiMove.piece.MoveTo(aiMove.move);
        aiMove.piece.hasMoved = true;
        
        // Play sound for AI move
        GameManager.Instance.PlayMoveSound(isCapture);
        
        gameManager.EndTurn();
    }

    public class AIMove
    {
        public Piece piece;
        public Vector2Int move;
        public Vector2Int originalPosition;
        
        public AIMove(Piece piece, Vector2Int move, Vector2Int originalPosition)
        {
            this.piece = piece;
            this.move = move;
            this.originalPosition = originalPosition;
        }
    }
}