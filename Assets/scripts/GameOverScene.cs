using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverScene : MonoBehaviour
{
    public TMP_Text winnerText;
    public Button playAgainButton;
    public Button mainMenuButton;

    void Start()
    {
        // Display the winner from PlayerPrefs
        string winner = PlayerPrefs.GetString("Winner", "None");
        
        if (winner == "White")
        {
            winnerText.text = "WHITE WINS!";
        }
        else if (winner == "Black")
        {
            winnerText.text = "BLACK WINS!";
        }
        else
        {
            winnerText.text = "DRAW!";
        }
        
        // 🔥 DIRECT BUTTON LISTENER SETUP
        playAgainButton.onClick.AddListener(PlayAgain);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    void PlayAgain()
    {
        SceneManager.LoadScene("gamescene"); // Mee game scene name
    }

    void ReturnToMainMenu()
    {
        SceneManager.LoadScene("mainmenu"); // Mee main menu scene name
    }
}