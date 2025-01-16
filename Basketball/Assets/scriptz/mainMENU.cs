using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mapSelectionUI;

    // Called when the "Play" button is clicked
    public void PlayGame()
    {
        // Show the map selection UI instead of directly loading a scene
        mapSelectionUI.SetActive(true);
    }

    // Called when a specific map is selected
    public void SelectMap(int mapIndex)
    {
        // Hide the map selection UI
        mapSelectionUI.SetActive(false);

        // Load the selected map scene
        if (SceneManager.GetSceneByName($"Map{mapIndex}") == null)
        {
            Debug.LogError("Invalid map index selected!");
            return;
        }

        SceneManager.LoadScene($"Map{mapIndex}");
    }

    public void QuitGame()
    {
        Debug.Log("QUIT");
        Application.Quit();
    }
}
