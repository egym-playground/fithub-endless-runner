using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{

    public GameObject model;
    public float runSpeed = 10f;
    public float minSpeed = 10f;
    public float maxSpeed = 30f;
    public float laneChangeSpeed = 10f;
    public float jumpSpeed = 5f;
    public float jumpLength = 7.5f;
    public float slideLength = 10f;
    public float jumpHeight = 1f;
    public float invisibleTime = 5f;
    public bool isGameOver = false;

    private Animator animator;
    private Rigidbody rb;
    private UIManager uiManager;
    private GameOverMenu gameOverMenu;
    private GameApiManager gameApiManager;
    private BoxCollider boxCollider;
    private Vector3 targetPosition;
    private Vector3 boxColliderSize;
    private Vector2 startingTouch;
    private bool isSwiping = false;
    private bool isJumping = false;
    private bool isSliding = false;
    private bool isInvisible = false;
    private float jumpStart;
    private float slideStart;
    private int currentLives = 3;
    private int coins;
    private float score;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        uiManager = FindObjectOfType<UIManager>();
        gameOverMenu = FindObjectOfType<GameOverMenu>();
        SetupGameApiManager();
        boxCollider = GetComponent<BoxCollider>();
        boxColliderSize = boxCollider.size;
        animator.Play("runStart");
    }
    
    void SetupGameApiManager()
    {
        // Find existing GameApiManager or create one
        gameApiManager = FindObjectOfType<GameApiManager>();
        if (gameApiManager == null)
        {
            Debug.Log("GameApiManager not found, creating one for score submission...");
            GameObject apiObject = new GameObject("GameApiManager");
            gameApiManager = apiObject.AddComponent<GameApiManager>();
            Debug.Log("GameApiManager created successfully by Player script");
        }
        else
        {
            Debug.Log("GameApiManager found by Player script");
        }
    }

    void Update()
    {
        if (!isGameOver)
        {
            HandleScore();
            MoveCharacter();
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = Vector3.forward * runSpeed;
    }

    void HandleScore()
    {
        score += Time.deltaTime * runSpeed;
        uiManager.UpdateScore((int)score);
    }

    void MoveCharacter()
    {
        HandleKeyboard();
        HandleTouch();
        HandleJump();
        HandleSlide();

        Vector3 newPosition = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
        transform.localPosition = Vector3.MoveTowards(transform.position, newPosition, laneChangeSpeed * Time.deltaTime);
    }

    void HandleKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) ChangeLane(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) ChangeLane(1);
        else if (Input.GetKeyDown(KeyCode.UpArrow)) Jump();
        else if (Input.GetKeyDown(KeyCode.DownArrow)) Slide();
    }

    void HandleTouch()
    {
        if (Input.touchCount == 1)
        {
            if (isSwiping)
            {
                Vector2 diff = Input.GetTouch(0).position - startingTouch;
                diff = new Vector2(diff.x / Screen.width, diff.y / Screen.width);

                if (diff.magnitude > 0.01f)
                {
                    if (Mathf.Abs(diff.y) > Mathf.Abs(diff.x))
                    {
                        HandleVerticalSwiping(diff.y);
                    }
                    else
                    {
                        HandleHorizontalSwipping(diff.x);
                    }

                    isSwiping = false;
                }
            }

            validateSwiping();
        }
    }

    void HandleVerticalSwiping(float diffY)
    {
        if (diffY < 0) Slide();
        else Jump();
    }

    void HandleHorizontalSwipping(float diffX)
    {
        if (diffX < 0) ChangeLane(-1);
        else ChangeLane(1);
    }

    void validateSwiping()
    {
        if (Input.GetTouch(0).phase == TouchPhase.Began)
        {
            startingTouch = Input.GetTouch(0).position;
            isSwiping = true;
        }

        if (Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            isSwiping = false;
        }
    }

    public void ChangeLane(int direction)
    {
        float targetLane = targetPosition.x + direction;

        if (targetLane >= -1f && targetLane <= 1f)
        {
            targetPosition.x = targetLane;
        }
    }

    public void Jump()
    {
        if (!isJumping)
        {
            isJumping = true;
            jumpStart = transform.position.z;
            animator.SetBool("Jumping", true);
            animator.SetFloat("JumpSpeed", runSpeed / jumpLength);
        }
    }

    void HandleJump()
    {
        if (isJumping && !isSliding)
        {
            float ratio = (transform.position.z - jumpStart) / jumpLength;

            if (ratio >= 1)
            {
                isJumping = false;
                animator.SetBool("Jumping", false);
            }
            else
            {
                targetPosition.y = Mathf.Sin(ratio * Mathf.PI) * jumpHeight;
            }
        }
        else
        {
            targetPosition.y = Mathf.MoveTowards(targetPosition.y, 0, jumpSpeed * Time.deltaTime);
        }
    }

    public void Slide()
    {
        if (!isJumping && !isSliding)
        {
            isSliding = true;
            slideStart = transform.position.z;
            boxCollider.size /= 2;
            animator.SetBool("Sliding", true);
            animator.SetFloat("JumpSpeed", runSpeed / jumpLength);
        }
    }

    void HandleSlide()
    {
        if (isSliding)
        {
            float ratio = (transform.position.z - slideStart) / slideLength;

            if (ratio >= 1)
            {
                isSliding = false;
                boxCollider.size = boxColliderSize;
                animator.SetBool("Sliding", false);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin")) GetCoins(other);
        if (!isInvisible && other.CompareTag("Obstacle")) HitObstacles();
    }

    void GetCoins(Collider other)
    {
        coins++;
        uiManager.UpdateCoins(coins);
        other.gameObject.SetActive(false);
    }

    void HitObstacles()
    {
        if (isGameOver) return;

        currentLives--;
        Debug.Log($"Player hit obstacle! Lives remaining: {currentLives}");
        uiManager.UpdateLives(currentLives);
        animator.SetTrigger("Hit");

        if (currentLives <= 0)
        {
            Debug.Log("GAME OVER! Player has no lives left!");
            isGameOver = true;
            runSpeed = 0;
            animator.SetBool("Dead", true);
            StartCoroutine(GameOver());
        }
        else
        {
            StartCoroutine(Blinking());
        }
    }

    IEnumerator Blinking()
    {
        float timer = 0;
        float currentBlink = 1f;
        float lastBlink = 0;
        float blinkPeriod = 0.1f;
        bool enabled = false;
        isInvisible = true;
        yield return new WaitForSeconds(0.5f);
        runSpeed = minSpeed;

        while (timer < invisibleTime && isInvisible)
        {
            model.SetActive(enabled);
            yield return null;
            timer += Time.deltaTime;
            lastBlink += Time.deltaTime;

            if (blinkPeriod < lastBlink)
            {
                lastBlink = 0;
                currentBlink = 1f - currentBlink;
                enabled = !enabled;
            }
        }

        model.SetActive(true);
        isInvisible = false;
    }

    public void IncreaseSpeed()
    {
        runSpeed *= 1.15f;
        runSpeed = (runSpeed >= maxSpeed) ? maxSpeed : runSpeed;
    }

    IEnumerator GameOver()
    {
        Debug.Log("GameOver coroutine started! Waiting 2 seconds for death animation...");

        // Wait for death animation to play
        yield return new WaitForSeconds(2f);

        Debug.Log("Death animation finished. Submitting score to API...");

        // Ensure GameApiManager is available before submitting score
        if (gameApiManager == null)
        {
            Debug.Log("GameApiManager was null, trying to find or create it now...");
            SetupGameApiManager();
        }

        // Submit score to API before showing UI
        if (gameApiManager != null)
        {
            SubmitScoreToApi((int)score, coins);
        }
        else
        {
            Debug.LogError("CRITICAL: Still cannot find or create GameApiManager! Score will not be submitted to API.");
        }

        Debug.Log("Attempting to show Game Over UI...");

        // Show game over UI - try both UIManager and GameOverMenu
        if (gameOverMenu != null)
        {
            Debug.Log("Using GameOverMenu to show game over");
            gameOverMenu.ShowGameOver((int)score, coins);
        }
        else if (uiManager != null)
        {
            Debug.Log("Using UIManager to show game over");
            uiManager.ShowGameOverMenu();
        }
        else
        {
            Debug.LogError("CRITICAL: No Game Over UI found! Add UIManager or GameOverMenu to the scene.");
        }
    }

    /// <summary>
    /// Submit the player's final score and coins to the API
    /// </summary>
    private void SubmitScoreToApi(int finalScore, int finalCoins)
    {
        Debug.Log("=== SCORE SUBMISSION STARTED ===");
        Debug.Log($"Player final stats - Score: {finalScore}, Coins: {finalCoins}");
        
        // Validate GameApiManager
        if (gameApiManager == null)
        {
            Debug.LogError("GameApiManager is null! Cannot submit score to API.");
            return;
        }
        
        Debug.Log("GameApiManager found, creating score data object...");
        
        // Create score data object
        var scoreData = new GameOverScoreData
        {
            score = finalScore,
            coins = finalCoins
        };

        Debug.Log($"Score data created: {JsonUtility.ToJson(scoreData)}");
        Debug.Log("Calling GameApiManager.SubmitGameOverScore...");

        // Submit to API - you can fill in the endpoint URL
        gameApiManager.SubmitGameOverScore(scoreData, 
            (success) => {
                Debug.Log("=== SCORE SUBMISSION CALLBACK ===");
                if (success)
                {
                    Debug.Log("✅ Score submitted successfully to API!");
                    Debug.Log($"Final submitted data - Score: {finalScore}, Coins: {finalCoins}");
                }
                else
                {
                    Debug.LogWarning("❌ Failed to submit score to API!");
                    Debug.LogWarning($"Failed data - Score: {finalScore}, Coins: {finalCoins}");
                }
                Debug.Log("=== SCORE SUBMISSION COMPLETE ===");
            });
    }
}
