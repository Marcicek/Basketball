using System.Collections;
using UnityEngine;
// using LootLocker.Requests;

public class Leaderboard : MonoBehaviour
{
    int leaderboardID = 25326; // Leaderboard ID zostáva int

    public IEnumerator SubmitScoreRoutine(int scoreToUpload)
    {
        // Skontroluj, èi máme platné Player ID
        string playerID = PlayerPrefs.GetString("PlayerID");
        if (string.IsNullOrEmpty(playerID))
        {
            Debug.LogError("Player ID is not set. Please set a valid Player ID.");
            yield break; // Ukonèi korutinu, ak je Player ID neplatné
        }

        bool done = false;

        // Konverzia leaderboardID na string pri volaní metódy
        /*LootLockerSDKManager.SubmitScore(playerID, scoreToUpload, leaderboardID.ToString(), (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully uploaded score: " + scoreToUpload);
            }
            else
            {
                Debug.LogError("Failed to upload score.");
            }
            done = true;
        });*/

        // Poèkaj, kým sa proces odovzdania dokonèí
        yield return new WaitWhile(() => !done);
    }
}
