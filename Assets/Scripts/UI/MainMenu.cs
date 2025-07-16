using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Reference to the AudioSource component
    public AudioSource mainMenuAudio;

    private void Start()
    {
        // Ensure that the audio starts playing when this scene starts
        if (SceneManager.GetActiveScene().buildIndex == 0) // Assuming the main menu is scene 0
        {
            mainMenuAudio.Play();
        }
    }

    private void OnEnable()
    {
        // Subscribe to the scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Unsubscribe from the scene loaded event to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // This method is called whenever a new scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Stop the audio when leaving the main menu (scene index 0)
        if (scene.buildIndex != 0)
        {
            mainMenuAudio.Stop();
        }
    }

    public void PlayGame()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadSceneAsync(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
