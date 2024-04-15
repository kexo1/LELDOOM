using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{   
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private CapsuleCollider capsuleCollider;
    public Rigidbody rigidBody;
    [SerializeField] private TMP_Text playerSpeed;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private Sliding slideScript;

    [Header("Movement")]
    public float walkSpeed;
    [SerializeField] private float maxSlideSlopeSpeed;
    [SerializeField] private float slideMultiplier;
    [SerializeField] float crouchSpeed;
    [SerializeField] float playerPredictionTime;
    public float desiredMoveSpeed;
    
    private float lastDesiredMoveSpeed;
    public float speedChangeMultiplier = 1.5f;
    public float slopeIncreaseMultiplier = 2.5f;

    [Header("Jumping")]
    public float jumpForce;
    [SerializeField] private float jumpCooldown ;
    [SerializeField] private float airMultiplier;
    [SerializeField] private float groundTimer;
    [SerializeField] private float jumpBufferTime;
    [SerializeField] private int jumpStrike;
    public int maxJumpStrikes;

    private float timeOnGround;
    // Defines time between jump and ground hit
    private float jumpBufferCounter;
    // Defines time before bunny-hop jumps reset
    private float noJumpTime;
    private bool readyToJump = true;

    [Header("Ground Check")]
    [SerializeField] private float groundDrag = 8;
    [SerializeField] private float playerHeight = 2;
    [SerializeField] private LayerMask whatIsGround;
    public bool isGrounded;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle = 60f;

    [Header("Debug")]
    public bool sliding;
    public bool underCeiling;
    public bool isFalling = false;
    public MovementState state;
    public float moveSpeed = 12f;
    public Vector3 moveDirection;

    [Header("Enemy Data")]
    public bool playerPrediction = false;
    private float lastDirection = 0f;
    private float lastDirectonTimer;
    public Vector3 groundPosition;
    
    private RaycastHit slopeHit;
    private Vector3 currentCenter;
    private bool exitingSlope;
    private float horizontalInput;
    private float verticalInput;
    
    public enum MovementState
    {
        walking,
        air,
        underCeiling,
        sliding
    }

    private void Update()
    {   
        GroundCheck();
        MyInput();
        SpeedControl();
        StateHandler();
    }

    private void FixedUpdate() 
    {
        MovePlayer();
    }

    private void MyInput()
    {   
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        JumpBuffer();
        JumpMechanics();
        PlayerPreditcion();
    }

    private void GroundCheck()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.height * 0.5f + 0.4f, whatIsGround);
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 500, whatIsGround);
        groundPosition = hit.point;

        if(isGrounded && readyToJump)
        {
            if (isFalling && !audioManager.SFXSource.isPlaying && !sliding)
            {   
                audioManager.PlaySFX(audioManager.landSound);
                isFalling = false;
            }
            
            rigidBody.drag = groundDrag;
            timeOnGround = groundTimer;
        } 
        else
        {
            isFalling = true;
            timeOnGround -= Time.deltaTime;
            rigidBody.drag = 0;        
        }
    }

    private void PlayerPreditcion()
    {
        lastDirectonTimer += Time.deltaTime;
        
        if (horizontalInput != lastDirection || Input.GetKeyDown(KeyCode.Space) || (slideScript.slideTimer > 0f && sliding))
        {
            lastDirection = horizontalInput;
            lastDirectonTimer = 0f;
        }

        if (lastDirectonTimer <= playerPredictionTime)
            playerPrediction = false;
        else 
            playerPrediction = true;
    }

    private void JumpMechanics()
    {

        if(jumpBufferCounter > 0f && timeOnGround > 0f)
        {   
            // If jumped during slide
            if (sliding) slideScript.StopSlide();

            if (jumpBufferCounter < 0.2f && !OnSlope() && rigidBody.velocity.magnitude > 11f)
            {
                if (jumpStrike < maxJumpStrikes) 
                    jumpStrike += 1;
            }
            else
            {
                jumpStrike = 0;   
            }
        
            Jump();

            jumpBufferCounter = 0f;
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void JumpBuffer()
    {
        if(Input.GetKeyDown(KeyCode.Space) && readyToJump && !underCeiling)
        {
            jumpBufferCounter = jumpBufferTime;
            noJumpTime = 0.8f;
        } 
        else {
            
            jumpBufferCounter -= Time.deltaTime;
            noJumpTime -= Time.deltaTime;

            if (noJumpTime < 0)
                jumpStrike = 0;
        }
    }

    private void StateHandler()
    {   
        if (sliding)
        {
            state = MovementState.sliding;
            if(OnSlope() && rigidBody.velocity.y < 0.1f)
                desiredMoveSpeed = maxSlideSlopeSpeed;
            // If timer above slide time, then start slowing down
            else if (slideScript.slideTimer > 0f)
                desiredMoveSpeed = slideScript.slideSpeed;
            else
                desiredMoveSpeed = 0f;
        }

        else if (underCeiling)
        {   
            // If umder something, slow down player
            state = MovementState.underCeiling;
            desiredMoveSpeed = crouchSpeed;
            moveSpeed = crouchSpeed;
            jumpStrike = 0;
        }

        else if (isGrounded)
        {   
            // Always add BunnyHop speed to player
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed + jumpStrike;
        }
        else 
        {
            state = MovementState.air;

            if (rigidBody.velocity.y < -0.1f)
                capsuleCollider.height = 2f;
            else
                capsuleCollider.height = 1;
        }

        MoveSpeedChangeCheck();
    }

    private void MoveSpeedChangeCheck()
    {
        if(Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
            moveSpeed = desiredMoveSpeed;

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedChangeMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
            {   
                // Needs to be multiplied because player would slide unrelistically for longer time
                if (sliding)
                    time += Time.deltaTime * speedChangeMultiplier * 4;
                else
                    time += Time.deltaTime * speedChangeMultiplier;
            }
                

            yield return null;
        }
        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(OnSlope() && !exitingSlope) 
        {
            rigidBody.AddForce(10 * moveSpeed * GetSlopeMoveDirection(moveDirection), ForceMode.Force);
            if (rigidBody.velocity.y > 0)
                rigidBody.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if(isGrounded && sliding && 0f < slideScript.slideTimer)
            rigidBody.AddForce(moveDirection.normalized * slideScript.slideForce, ForceMode.Force);
            
        else if(isGrounded)
            rigidBody.AddForce(10f * moveSpeed * moveDirection.normalized, ForceMode.Force);

        else if(!isGrounded)
            rigidBody.AddForce(10f * airMultiplier * moveSpeed * moveDirection.normalized, ForceMode.Force);

        rigidBody.useGravity = !OnSlope();
        playerSpeed.text = $"{Math.Round(rigidBody.velocity.magnitude, 1)}";
    }

    private void SpeedControl()
    {   
        if (OnSlope() && !exitingSlope)
        {
            if(rigidBody.velocity.magnitude > moveSpeed)
                rigidBody.velocity = rigidBody.velocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new(rigidBody.velocity.x, 0f, rigidBody.velocity.z);

            if(flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rigidBody.velocity = new Vector3(limitedVel.x, rigidBody.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {   
        readyToJump = false;
        exitingSlope = true;
        rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);
        rigidBody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {   
        readyToJump = true;
        exitingSlope = false;
    }

    public bool OnSlope()
    {   
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {   
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {   
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private void OnCollisionEnter(Collision hit) 
    {
        switch(hit.gameObject.tag)
        {    
            case "JumpPad":
                exitingSlope = true;
                rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);
                
                // Get the normal of the collision surface
                Vector3 collisionNormal = hit.contacts[0].normal;

                Vector3 launchDirection = collisionNormal;

                rigidBody.velocity = Vector3.zero; 
                rigidBody.AddForce(5 * jumpForce * launchDirection, ForceMode.Impulse);

                audioManager.PlaySFX(audioManager.jumpPadSound);
                break;
        }   
    }
}
