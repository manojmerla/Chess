using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public Camera cam;
    private Piece selectedPiece;
    public LayerMask pieceMask;
    public LayerMask tileMask;

    private RaycastHit hit;

    void Start()
    {
        Input.multiTouchEnabled = true;
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
                Debug.LogError("No camera assigned to InputManager and no Camera.main found!");
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver()) return;

        // Mobile touch input
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                    HandleClick(Input.GetTouch(i).position);
            }
        }
        // Mouse input for editor
        else if (Input.GetMouseButtonDown(0))
        {
            HandleClick(Input.mousePosition);
        }
    }

    void HandleClick(Vector2 screenPos)
    {
        if (GameManager.Instance.IsGameOver()) return;

        Ray ray = cam.ScreenPointToRay(screenPos);

        // Piece click
        if (Physics.Raycast(ray, out hit, 100f, pieceMask))
        {
            Piece clickedPiece = hit.collider.GetComponent<Piece>();
            if (clickedPiece == null) return;

            if (selectedPiece == null)
            {
                if (GameManager.Instance.IsPlayersTurn(clickedPiece.color))
                {
                    selectedPiece = clickedPiece;
                    Debug.Log("SELECTED: " + selectedPiece.name + " at " + selectedPiece.boardPos);
                }
            }
            else
            {
                TryCapture(clickedPiece);
            }
        }
        // Tile click
        else if (Physics.Raycast(ray, out hit, 100f, tileMask) && selectedPiece != null)
        {
            string[] parts = hit.collider.gameObject.name.Split('_');
            int x = int.Parse(parts[1]);
            int y = int.Parse(parts[2]);
            Vector2Int newPos = new Vector2Int(x, y);

            TryMoveToTile(newPos);
        }
    }

    private void TryCapture(Piece targetPiece)
    {
        if (targetPiece.color != selectedPiece.color)
        {
            List<Vector2Int> legalMoves = selectedPiece.GetLegalMoves(selectedPiece.board, GameManager.Instance);

            if (legalMoves.Contains(targetPiece.boardPos))
            {
                Debug.Log("CAPTURING: " + targetPiece.name);

                if (targetPiece is King)
                {
                    targetPiece.gameObject.SetActive(false);
                    GameManager.Instance.GameOver(selectedPiece.color);
                    selectedPiece = null;
                    return;
                }

                targetPiece.gameObject.SetActive(false);
                selectedPiece.MoveTo(targetPiece.boardPos);

                GameManager.Instance.PlayMoveSound(true);
                GameManager.Instance.EndTurn();
            }
        }
        selectedPiece = null;
    }

    private void TryMoveToTile(Vector2Int newPos)
    {
        List<Vector2Int> legalMoves = selectedPiece.GetLegalMoves(selectedPiece.board, GameManager.Instance);

        if (!legalMoves.Contains(newPos))
        {
            selectedPiece = null;
            return;
        }

        Tile targetTile = selectedPiece.board.tiles[newPos.x, newPos.y].GetComponent<Tile>();

        // Capture if enemy piece exists
        if (targetTile.currentPiece != null && targetTile.currentPiece.color != selectedPiece.color)
        {
            Debug.Log("CAPTURING: " + targetTile.currentPiece.name);

            if (targetTile.currentPiece is King)
            {
                targetTile.currentPiece.gameObject.SetActive(false);
                GameManager.Instance.GameOver(selectedPiece.color);
                selectedPiece = null;
                return;
            }

            targetTile.currentPiece.gameObject.SetActive(false);
            selectedPiece.MoveTo(newPos);

            GameManager.Instance.PlayMoveSound(true);
        }
        else
        {
            selectedPiece.MoveTo(newPos);
            GameManager.Instance.PlayMoveSound(false);
        }

        GameManager.Instance.EndTurn();
        selectedPiece = null;
    }
}
