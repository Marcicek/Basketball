using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMENU : MonoBehaviour
{
    public GameObject mapSelectionUI; // Reference to the Map Selection UI

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
        switch (mapIndex)
        {
            case 1:
                SceneManager.LoadScene("Map1"); // Replace "Map1" with your actual scene name
                break;
            case 2:
                SceneManager.LoadScene("Map2"); // Replace "Map2" with your actual scene name
                break;
            case 3:
                SceneManager.LoadScene("Map3"); // Replace "Map3" with your actual scene name
                break;
            default:
                Debug.LogError("Invalid map index selected!");
                break;
        }
    }

    public void QuitGame()
    {
        Debug.Log("QUIT");
        Application.Quit();
    }
}
