using System.Collections;
using UnityEngine;

public class BasketballController : MonoBehaviour
{
    public Lboard leaderboard;
    public float MoveSpeed = 10;
    public float SprintMultiplier = 2f; // Speed multiplier while sprinting
    public Transform Ball;
    public Transform PosDribble;
    public Transform PosOverHead;
    private bool IsBallOnGround = false; // Indication if the ball is on the ground
    public Transform Arms;
    public Transform Target;
    public float BounceForce = 500f;
    public Ball ball; // The ball controlled by the player

    private bool IsBallInHands = false;
    private bool IsBallFlying = false;
    private float T = 0;
    private float shotTimer = 0;
    private bool shotTimerStarted = false;
    private bool isHoldingBallOverhead = false;

    // Adjustable keys
    public KeyCode MoveUpKey = KeyCode.W;
    public KeyCode MoveDownKey = KeyCode.S;
    public KeyCode MoveLeftKey = KeyCode.A;
    public KeyCode MoveRightKey = KeyCode.D;
    public KeyCode ShootKey = KeyCode.V; // Shooting
    public KeyCode SprintKey = KeyCode.LeftShift; // Sprinting
    public KeyCode BlockKey = KeyCode.T; // Blocking
    public KeyCode StealKey = KeyCode.M; // Stealing the ball

    // Blocking mechanics
    public float BlockJumpHeight = 2f; // Height to jump when blocking
    public float BlockDuration = 0.5f; // Duration of the block
    private bool isBlocking = false; // Indicates if the player is blocking

    private BasketballController opponent; // Reference to the other player

    private void Start()
    {
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
        {
            HandleMovement();
        }

        if (Input.GetKeyDown(BlockKey) && !isBlocking)
        {
            StartCoroutine(BlockAction());
        }

        if (Input.GetKeyDown(StealKey))
        {
            TryStealBall();
        }

        HandleBallLogic();
        UpdatePlayerRoles();
    }

    private void UpdatePlayerRoles()
    {
        if (IsBallInHands)
        {
            opponent.SetAsOpponent();
        }
        else
        {
            opponent.ResetOpponent();
        }
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") && !IsBallInHands && !IsBallFlying)
        {
            GainBall();
        }
    }

    private void HandleMovement()
    {
        Vector3 direction = Vector3.zero;

        if (Input.GetKey(MoveUpKey)) direction.z += 1;
        if (Input.GetKey(MoveDownKey)) direction.z -= 1;
        if (Input.GetKey(MoveLeftKey)) direction.x -= 1;
        if (Input.GetKey(MoveRightKey)) direction.x += 1;

        if (direction != Vector3.zero)
        {
            bool isSprinting = Input.GetKey(SprintKey);
            float currentSpeed = isSprinting ? MoveSpeed * SprintMultiplier : MoveSpeed;

            transform.position += direction.normalized * currentSpeed * Time.deltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 10 * Time.deltaTime);
        }
    }

    private void HandleBallLogic()
    {
        if (IsBallInHands && !isBlocking)
        {
            ball.DisableTrail();

            if (Input.GetKey(ShootKey))
            {
                isHoldingBallOverhead = true;
                Ball.position = PosOverHead.position;
                Arms.localEulerAngles = Vector3.right * 150;
                transform.LookAt(new Vector3(Target.position.x, transform.position.y, Target.position.z));

                if (!shotTimerStarted)
                {
                    shotTimer = 0;
                    shotTimerStarted = true;
                }

                if (shotTimer >= 1.5f)
                {
                    ShootBallAutomatically();
                }

                shotTimer += Time.deltaTime;
            }
            else
            {
                isHoldingBallOverhead = false;
                Ball.position = PosDribble.position + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * 5));
                Arms.localEulerAngles = Vector3.zero;
            }

            if (Input.GetKeyUp(ShootKey))
            {
                isHoldingBallOverhead = false;
                IsBallInHands = false;
                IsBallFlying = true;
                ball.ActiveTrail();
                T = 0;
                Arms.localEulerAngles = Vector3.zero;
            }
        }
        else if (IsBallFlying && !isBlocking)
        {
            T += Time.deltaTime;
            float duration = 0.35f;
            float t01 = T / duration;

            Vector3 A = PosOverHead.position;
            Vector3 B = Target.position;
            Vector3 pos = Vector3.Lerp(A, B, t01);
            Vector3 arc = Vector3.up * 7 * Mathf.Sin(t01 * Mathf.PI);

            Ball.position = pos + arc;

            if (t01 >= 1)
            {
                IsBallFlying = false;
                Ball.GetComponent<Rigidbody>().isKinematic = false;
                Ball.SetParent(null);
            }
        }
    }

    private IEnumerator BlockAction()
    {
        isBlocking = true;
        Vector3 originalPosition = transform.position;
        Vector3 jumpPosition = originalPosition + Vector3.up * BlockJumpHeight;

        float timer = 0;
        while (timer < BlockDuration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(originalPosition, jumpPosition, timer / BlockDuration);
            yield return null;
        }

        timer = 0;
        while (timer < BlockDuration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(jumpPosition, originalPosition, timer / BlockDuration);
            yield return null;
        }

        isBlocking = false;
    }

    private void TryStealBall()
    {
        // Check if the opponent has the ball and if the player is close enough
        if (opponent != null && opponent.IsBallInHands)
        {
            float distanceToOpponent = Vector3.Distance(transform.position, opponent.transform.position);

            if (distanceToOpponent <= 2f) // Adjust this distance as needed
            {
                StealBall();
            }
        }
    }

    public void LoseBall()
    {
        IsBallInHands = false;
        ball.IsOnGround = true;
        ball.transform.SetParent(null);
        ball.GetComponent<Rigidbody>().isKinematic = false;
        UpdatePlayerRoles();
        Debug.Log($"{name} lost the ball.");
    }

    public void GainBall()
    {
        IsBallInHands = true;
        ball.IsOnGround = false;
        ball.transform.SetParent(transform);
        ball.GetComponent<Rigidbody>().isKinematic = true;
        UpdatePlayerRoles();
        Debug.Log($"{name} gained the ball.");
    }

    private void ShootBallAutomatically()
    {
        isHoldingBallOverhead = false;
        IsBallInHands = false;
        IsBallFlying = true;
        T = 0;
        Arms.localEulerAngles = Vector3.zero;
        ball.ActiveTrail();
    }

    private void StealBall()
    {
        if (opponent != null && opponent.IsBallInHands)
        {
            // Transfer the ball from the opponent to this player
            opponent.LoseBall();
            GainBall();

            Debug.Log("The ball was stolen!");
        }
    }
}