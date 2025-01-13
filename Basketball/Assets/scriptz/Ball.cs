using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Ball : MonoBehaviour
{
    public GameObject trailObject;
   
    public UnityEvent scoredEvent;

    public BasketballController lastHolder; // Referencia na posledného hráèa, ktorý mal loptu
    public bool IsBallOnGround = false;
    public bool IsOnGround { get; set; } = false;

    void Start()
    {
        trailObject.SetActive(false);
       
    }

    public void ActiveTrail()
    {
        trailObject.SetActive(true);
    }

    public void DisableTrail()
    {
        trailObject.SetActive(false);
    }

    void OnTriggerEnter(Collider otherCollider)
    {
        if (otherCollider.GetComponent<ScoreArea>() != null)
        {
            Debug.Log("Score");
            scoredEvent.Invoke();
            DisableTrail();
        }
    }
  
}