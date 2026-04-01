using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{
    [SerializeField] private string sceneName; // set in Inspector

    // This will be called only when Button is pressed
    public void SwitchScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
