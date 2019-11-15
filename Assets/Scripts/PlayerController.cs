using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RigidbodyMovement2D))]
public class PlayerController : MonoBehaviour {

    //master movement class
    RigidbodyMovement2D Movement;

    //variables for getting input and controlling speed
    [HideInInspector]
    public Vector2 moveInput;
    [HideInInspector]
    public Vector2 pushInput; //Input specifically for when you possess a push core
    public float moveSpeed = 6f;
    public float dashSpeed = 10f;
    public float accelerationTimeAirborne;
    private float velocityXSmoothing;
    public float moveAfterLaunchTime;
    private float moveAfterLaunchTimer;
    private int dashDir; //1 is right, 2 is diag down right, and so on clockwise (0 is null basically)
    private int lastDir;

    //variables for variable jump height
    public float maxJumpVelocity;
    public float minJumpVelocity;
    public int jumpCount;
    public int curJumpCount;

    //Boolean to decide if you can possess or not
    [HideInInspector]
    public bool canPossess = true; 

    //placeholder name for the variable, stands for an object that you're going to possess
    public Rigidbody2D coreRB;
 
    //the player's rigidbody
    private Rigidbody2D rb;

    //for turning the player around
    private bool facingRight = true;
    //so that we can use the run start up frames
    private bool wasJustIdle;

    //Everything for being grounded
    [HideInInspector]
    public bool isGrounded;
    public float checkRadius;
    public LayerMask whatIsGround;
    public Transform groundCheck;

    public float possessionTimerOriginal;
    private float possessionTimer;

    //dashing and possessing booleans for deciding if you can enter an object or not
    [HideInInspector]
    public bool entering; 
    [HideInInspector]
    public bool possessing; 
    [HideInInspector]
    public bool isCancelledPressed;

    [HideInInspector]
    public BoxCollider2D boxCollider;

    private Vector3 playerScale;

    GameObject nonCollideCore;
    BoostPaintingController BoostPaintingController;
    SpriteRenderer spriteRenderer;

    private bool isBoostPainting;
    private bool canEnter;

    public enum PlayerStates
    {
        Idle = 0,
        Moving = 1,
        JumpingUp = 2,
        Falling = 3,
        Entering = 4,
        PossessingCollide = 5,
        PossessingNonCollide = 6

    }

    public PlayerStates currentState;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }
    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody2D>();
        Movement = GetComponent<RigidbodyMovement2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerScale = transform.localScale;
        possessionTimer = possessionTimerOriginal;
        moveAfterLaunchTimer = moveAfterLaunchTime;
    }


    private void FixedUpdate()
    {

            if (currentState == PlayerStates.Idle || currentState == PlayerStates.Moving || currentState == PlayerStates.JumpingUp || currentState == PlayerStates.Falling)
            {

                if (isGrounded)
                {
                    //this line literally moves the character by changing its velocity directly
                    rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
                    jumpCount = curJumpCount;
                }
                else
                {
                    float targetVelocityX = moveInput.x * moveSpeed;
                    rb.velocity = new Vector2(Mathf.SmoothDamp(rb.velocity.x, targetVelocityX, ref velocityXSmoothing, accelerationTimeAirborne), rb.velocity.y);
                }

        }
    }

    // Update is called once per frame
    void Update()
    {
        if(moveInput.x != 0)
        {
            lastDir = (int)Mathf.Sign(moveInput.x);
        }

        //checks if you're grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);

        //canPossess acts as a dash charge, once you land it refills.
        if (isGrounded)
        {
            canPossess = true;
        }

        //gets horizontal and vertical input so you can move and dash in all 8 directions
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        switch (currentState)
        {
            case PlayerStates.Idle:
                wasJustIdle = true;
                if (moveInput.x > 0 || moveInput.x < 0)
                {
                    currentState = PlayerStates.Moving;
                }
                if (Input.GetButton("Jump")) 
                {
                    currentState = PlayerStates.JumpingUp;
                }
                if (Input.GetButtonDown("Enter") && canPossess)
                {
                    entering = true;
                    canPossess = false;
                }
                if (!isGrounded)
                {
                    currentState = PlayerStates.Falling;
                }
                break;
            case PlayerStates.Moving:
                if (moveInput.x < 0.01f && moveInput.x > -0.01f) {
                    currentState = PlayerStates.Idle;
                }
                if (Input.GetButtonDown("Jump"))
                {
                    wasJustIdle = false;
                    currentState = PlayerStates.JumpingUp;
                }

                if (Input.GetButtonDown("Enter") && canPossess)
                {
                    entering = true;
                    canPossess = false;
                    wasJustIdle = false;
                }
                wasJustIdle = false;
                break;
            case PlayerStates.JumpingUp:
                //if you jump it changes your y velocity to the maxJumpVelocity
                Movement.JumpPlayer(ref rb, isGrounded, maxJumpVelocity);
                jumpCount--;
                //if you release jump while your y velocity is above your minJumpVelocity, your velocity gets set to your min jump velocity (variable jump height)
                if (Input.GetButtonUp("Jump"))
                {
                    Movement.JumpPlayerRelease(ref rb, minJumpVelocity);
                    currentState = PlayerStates.Falling;
                }

                if (Input.GetButtonDown("Enter") && canPossess)
                {
                    entering = true;
                    canPossess = false;
                }

                if (rb.velocity.y <= 0) {
                    currentState = PlayerStates.Falling;
                }

                break;
            case PlayerStates.Falling:
                isCancelledPressed = false; 
                if (isGrounded == true) {
                    if (Mathf.Abs(moveInput.x) > 0) {
                        currentState = PlayerStates.Moving;
                    }
                    currentState = PlayerStates.Idle;
                }
                if (Input.GetButtonDown("Jump") && jumpCount > 0)
                {
                    wasJustIdle = false;
                    currentState = PlayerStates.JumpingUp;
                }
                if (Input.GetButtonDown("Enter") && canPossess)
                {
                    entering = true;
                    canPossess = false;
                }

                break;

            case PlayerStates.PossessingCollide:
                spriteRenderer.color = Color.clear;
                if (possessing && Input.GetButtonDown("Jump") || possessionTimer <= 0)
                {
                    jumpCount = curJumpCount;
                    possessionTimer = possessionTimerOriginal;
                    RevertParent();
                    if(coreRB.velocity == new Vector2(0, 0)) 
                    {
                        canPossess = true;
                        isGrounded = true;
                        Movement.JumpPlayer(ref rb, isGrounded, maxJumpVelocity);
                        spriteRenderer.color = Color.white;
                        currentState = PlayerStates.JumpingUp;
                    }
                }

                possessionTimer -= Time.deltaTime;
                break;
            case PlayerStates.PossessingNonCollide:
                spriteRenderer.color = Color.clear;
                    if (possessing && Input.GetButtonDown("Jump") || possessionTimer <= 0)
                    {
                    pushInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                    isCancelledPressed = Input.GetButtonDown("Jump");
                        if (BoostPaintingController.pushTimer <= 0 || isCancelledPressed)
                        {
                            possessionTimer = possessionTimerOriginal;
                            RevertParent();
                            canPossess = true; 
                            spriteRenderer.color = Color.white;
                            currentState = PlayerStates.Falling;
                        }
                    }
                

                possessionTimer -= Time.deltaTime;
                break;
        }

	}


    //Changes the player's parent to whatever it's trying to possess
    void ChangeParent(Rigidbody2D core)
    {
        boxCollider.enabled = false;
        rb.isKinematic = true;
        transform.parent = core.transform;
        transform.position = core.transform.position;
    }

    void NonCollideChangeParent(GameObject core)
    {
        boxCollider.enabled = false;
        rb.isKinematic = true;
        transform.parent = core.transform;
        transform.position = core.transform.position;
    }

    //Revert the parent of object 2.
    void RevertParent()
    {
        possessing = false;
        transform.parent = null;
        rb.isKinematic = false;
        boxCollider.enabled = true;
        transform.localScale = playerScale;
        transform.rotation = Quaternion.Euler(0,0,0);

    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        transform.parent = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (entering)
        {
            Debug.Log("Input Registered.");

            if (collision.gameObject.layer == 10) 
            {
                if(collision.gameObject.tag == "BoostPainting") {
                    //stopping the player so that they don't start possessing with an initial velocity
                    rb.velocity = new Vector2(0, 0);
                    isBoostPainting = true;
                    possessing = true;
                    entering = false;
                    canEnter = true;
                    coreRB = collision.gameObject.GetComponent<Rigidbody2D>();
                    BoostPaintingController = collision.gameObject.GetComponent<BoostPaintingController>();
                    nonCollideCore = collision.gameObject;
                    NonCollideChangeParent(nonCollideCore);
                    currentState = PlayerStates.PossessingNonCollide;
                }

            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (entering)
        {
            Debug.Log("Input Registered.");
            if (collision.gameObject.layer == 10) 
            {
                if(collision.gameObject.tag == "BoostPainting") {
                    //stopping the player so that they don't start possessing with an initial velocity
                    rb.velocity = new Vector2(0, 0);
                    isBoostPainting = true;
                    possessing = true;
                    entering = false;
                    coreRB = collision.gameObject.GetComponent<Rigidbody2D>();
                    BoostPaintingController = collision.gameObject.GetComponent<BoostPaintingController>();
                    nonCollideCore = collision.gameObject;
                    NonCollideChangeParent(nonCollideCore);
                    currentState = PlayerStates.PossessingNonCollide;
                }
            }
        }
    }




    //flips the player around so we don't have to make more animations
    void Flip()
    {
        facingRight = !facingRight;
        Vector3 Scaler = transform.localScale;
        Scaler.x *= -1;
        transform.localScale = Scaler;
        playerScale.x = -playerScale.x;

    }
}
