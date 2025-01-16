using JetBrains.Annotations;
using System;
using System.Collections;
using UnityEngine;
using static BasketballController.BallControl;

public class BasketballController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Transform ball;
    [SerializeField] Transform posDribble;
    [SerializeField] Transform posOverHead;
    [SerializeField] Transform arms;
    [SerializeField] Transform hoop;
    [SerializeField] Leaderboard leaderboard;
    [SerializeField] Ball playerBall; // The ball controlled by the player
    
    Rigidbody playerBallRigidbody;
    Rigidbody ballRigidbody;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 10;
    [SerializeField] float sprintMultiplier = 2f; // Used to calculate sprint speed
    [SerializeField] float bounceForce = 500f;

    [Header("Adjustable Keys")]
    [SerializeField] KeyCode MoveUpKey = KeyCode.W;
    [SerializeField] KeyCode MoveDownKey = KeyCode.S;
    [SerializeField] KeyCode MoveLeftKey = KeyCode.A;
    [SerializeField] KeyCode MoveRightKey = KeyCode.D;
    [SerializeField] KeyCode ShootKey = KeyCode.V;           
    [SerializeField] KeyCode SprintKey = KeyCode.LeftShift;  
    [SerializeField] KeyCode BlockKey = KeyCode.T;         
    [SerializeField] KeyCode StealBallKey = KeyCode.M;           

    [Header("Blocking Mechanics")]
    [SerializeField] float blockJumpHeight = 2f;  // Height to jump when blocking
    [SerializeField] float blockDuration = 0.5f;
    [SerializeField] float distanceToStealBall = 2f;

    private float timeSinceLastShotInSeconds;
    private bool isBlocking = false;
    private bool isBallInHands = false;
    private bool isBallFlying = false;
    private bool shotTimerStarted = false;
    private bool isHoldingBallOverhead = false;

    private float timeHoldingShootKeyInSeconds = 0;

    private BasketballController opponent;

    private void Start()
    {
        ballRigidbody = ball.GetComponent<Rigidbody>();
        playerBallRigidbody = playerBall.GetComponent<Rigidbody>();

        // Automatically find and set the opponent
        BasketballController[] players = FindObjectsOfType<BasketballController>();
        foreach (var player in players)
        {
            if (player != this)
            {
                opponent = player; // Set the other player as the opponent
                break;
            }
        }
    }

    private void Update()
    {
        if (!isHoldingBallOverhead && !isBlocking)
            HandleMovement();

        if (Input.GetKeyDown(BlockKey) && !isBlocking)
            StartCoroutine(BlockAction());

        if (Input.GetKeyDown(StealBallKey))
            TryStealBall();

        HandleBallLogic();
        UpdatePlayerRoles();
    }

    private void UpdatePlayerRoles()
    {
        if (isBallInHands)
            opponent.SetAsOpponent();
        else
            opponent.ResetOpponent();
    }

    public void SetAsOpponent()
    {
        // Logic for when this player is the opponent
        Debug.Log($"{name} is now the opponent.");
    }

    public void ResetOpponent()
    {
        // Logic for resetting opponent state
        Debug.Log($"{name} is no longer the opponent.");
    }

    private void OnTriggerEnter(Collider collision)
    {
        // This needs to verify that no player is holding the ball
        if (collision.CompareTag("Ball") && !isBallInHands && !opponent.isBallInHands && !isBallFlying)
        {
            SetBallControl(GainBall);
        }
    }

    private void HandleMovement()
    {
        Vector3 direction = Vector3.zero;

        // This could instead be done with Input.GetAxis
        // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Input.GetAxis.html
        if (Input.GetKey(MoveUpKey)) 
            direction.z += 1;
        if (Input.GetKey(MoveDownKey)) 
            direction.z -= 1;
        if (Input.GetKey(MoveLeftKey)) 
            direction.x -= 1;
        if (Input.GetKey(MoveRightKey)) 
            direction.x += 1;

        if (direction != Vector3.zero)
        {
            bool isSprinting = Input.GetKey(SprintKey);
            float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

            transform.position += currentSpeed * Time.deltaTime * direction.normalized;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 10 * Time.deltaTime);
        }
    }

    // The large number of flags indicate this could be better written as a State Machine
    // State Machine Pattern: https://gameprogrammingpatterns.com/state.html
    private void HandleBallLogic()
    {
        // Lerp ball towards hoop
        if (isBallFlying && !isBlocking)
        {
            timeSinceLastShotInSeconds += Time.deltaTime;
            float duration = 0.35f;

            Vector3 current = posOverHead.position;
            Vector3 target = hoop.position;
            float time = timeSinceLastShotInSeconds / duration;

            Vector3 positionToHoop = Vector3.Lerp(current, target, time);
            Vector3 arc = 7 * Mathf.Sin(time * Mathf.PI) * Vector3.up;

            ball.position = positionToHoop + arc;

            if (time >= 1)
            {
                isBallFlying = false;
                ballRigidbody.isKinematic = false;
                ball.SetParent(null);
            }
            return;
        }

        if (!isBallInHands || isBlocking)
            return;

        playerBall.DisableTrail();

        // Ready shot
        if (Input.GetKey(ShootKey))
        {
            isHoldingBallOverhead = true;
            ball.position = posOverHead.position;
            arms.localEulerAngles = Vector3.right * 150;
            transform.LookAt(new Vector3(hoop.position.x, transform.position.y, hoop.position.z));

            if (!shotTimerStarted)
            {
                timeHoldingShootKeyInSeconds = 0;
                shotTimerStarted = true; // This value is never set to false
            }

            if (timeHoldingShootKeyInSeconds >= 1.5f)
                ShootBall();

            timeHoldingShootKeyInSeconds += Time.deltaTime;
        }
        else if (Input.GetKeyUp(ShootKey))
            ShootBall();
        // Dribble
        else
        {
            isHoldingBallOverhead = false;
            ball.position = posDribble.position + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * 5));
            arms.localEulerAngles = Vector3.zero;
        }
    }

    void ShootBall()
    {
        isHoldingBallOverhead = false;
        isBallInHands = false;
        isBallFlying = true;
        timeSinceLastShotInSeconds = 0;
        arms.localEulerAngles = Vector3.zero;
        playerBall.ActiveTrail();
    }

    private IEnumerator BlockAction()
    {
        isBlocking = true;
        var groundPosition = transform.position;
        var jumpPosition = groundPosition + Vector3.up * blockJumpHeight;

        // Lerp player to air, then back to ground
        // This could also just be done by applying upwards force, no?
        for (int i = 0; i < 2; i++)
        {
            float timeSinceLerpStarted = 0;
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = i == 0 ? jumpPosition : groundPosition;

            while (timeSinceLerpStarted < blockDuration)
            {
                timeSinceLerpStarted += Time.deltaTime;
                transform.position = Vector3.Lerp(startPosition, targetPosition, timeSinceLerpStarted / blockDuration);
                yield return null;
            }
        }
        isBlocking = false;
    }

    private void TryStealBall()
    {
        float distanceToOpponent = Vector3.Distance(transform.position, opponent.transform.position);

        if (!opponent.isBallInHands)
            return;
        if (distanceToOpponent < distanceToStealBall)
            return;

        // This code is never ran, likely due to the above conditionals
        Debug.Log("The ball was stolen!");

        opponent.SetBallControl(LoseBall);
        SetBallControl(GainBall);
    }

    public enum BallControl { GainBall, LoseBall }
    public void SetBallControl(BallControl ballControl)
    {
        bool hasControl = ballControl == GainBall;
        Debug.Log($"{gameObject.name} {(hasControl ? "gained" : "lost")} control of the ball!");

        Transform ballParent = hasControl ? transform : null;

        isBallInHands = hasControl;
        playerBall.IsOnGround = !hasControl;
        playerBall.transform.SetParent(ballParent);
        playerBallRigidbody.isKinematic = hasControl;
        
        UpdatePlayerRoles();
    }
}