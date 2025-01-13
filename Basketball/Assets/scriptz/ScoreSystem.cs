using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LootLocker.Requests;

public class ScoreSystem : MonoBehaviour
{
    public Lboard leaderboard;
    public TMP_Text scoreText;
    public Transform hoop; 
    public Transform character;
    public float distanceThreshold = 1;
    private int score = 0;
    private float lastScoreTime = 0;

    private void Start()
    {
        lastScoreTime = Time.time;
        StopRoutine();
    }

    public void scored()
    {
        if (Time.time - lastScoreTime < 0.5f) 
        {
            return;
        }

        float distance = Vector3.Distance(character.position, hoop.position); 

        if (distance > distanceThreshold) 
        {
            score = score + 3; 
        }
        else 
        {
            score = score + 2; 
        }

        scoreText.SetText(score.ToString());

        lastScoreTime = Time.time;
    }
   
    IEnumerator StopRoutine()
    {
        yield return leaderboard.SubmitScoreRoutine(score);
    }
}