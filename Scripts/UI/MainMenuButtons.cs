using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{
    public void StartGame()
    {
        // Загружаем сцену с индексом 1 (первая игровая сцена)
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            // Если игра запущена в редакторе Unity
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Если игра запущена как собранное приложение
            Application.Quit();
        #endif
    }
}
