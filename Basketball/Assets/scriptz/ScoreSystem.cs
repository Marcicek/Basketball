using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
// using LootLocker.Requests;

public class ScoreSystem : MonoBehaviour
{
    public Leaderboard leaderboard;
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

    public void Scored()
    {
        if (Time.time - lastScoreTime < 0.5f) 
            return;

        float playerDistanceFromHoop = Vector3.Distance(character.position, hoop.position); 

        if (playerDistanceFromHoop > distanceThreshold) 
            score += 3; 
        else 
            score += 2; 

        scoreText.SetText(score.ToString());
        lastScoreTime = Time.time;
    }
   
    IEnumerator StopRoutine()
        => leaderboard.SubmitScoreRoutine(score);
}