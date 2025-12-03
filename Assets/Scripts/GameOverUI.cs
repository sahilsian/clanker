using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Debug.Log("Game Restarted!");
        SceneManager.LoadScene("level1");
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit!");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}

