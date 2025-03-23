using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject controlsPanel;
    
    private void Start()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
            
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Time.timeScale = 1f;
    }
    
    public void StartGame()
    {
        SceneManager.LoadScene("Map");
        Debug.Log("StartGame button clicked!");

    }
    
    public void ToggleControlsMenu()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(!controlsPanel.activeSelf);
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}