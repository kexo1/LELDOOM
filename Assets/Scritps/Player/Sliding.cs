using System;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private AttributesManager playerHealth;
    [SerializeField] private PlayerScript playerScript;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private int layerMask;
    
    [Header("Sliding")]
    public float slideSpeed;
    public float slideForce;
    public float slideTimer;
    [SerializeField] private bool autoDetectCeiling;
    [SerializeField] private float crouchSmooth;
    [SerializeField] private float maxSlideTime;
    [SerializeField] private float slideCooldown;

    [Header("Input")]
    [SerializeField] private KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    [Header("Slide Sounds")]
    [SerializeField] private float slideSoundVolume;
    [SerializeField] private AudioClip[] slideSounds;

    private Vector3 inputDirection;
    private float startHeight;
    private float crouchHeight;
    private float heightTarget;
    private float cameraHeightTarget;
    private bool autoCheckCeiling = true;
    private bool readyToSlide = true;
    
    private readonly int excludedLayerMask = ~(1 << 9 | 1 << 10);

    private void Start()
    {
        startHeight = capsuleCollider.height;
        crouchHeight = startHeight / 2;  
        heightTarget = startHeight;
        cameraHeightTarget = cameraHolder.localPosition.y;    
        slideSounds = Resources.LoadAll<AudioClip>("Audio/Other/Slide");

        int bulletLayer = LayerMask.NameToLayer("Bullet");
        layerMask = ~(1 << bulletLayer);
    }

    private void Update()
    {   
        MyInput();
        UpdateColliderHeight();
        
        // If player crashes into a wall, player been sliding too long
        if (pm.sliding && rigidBody.velocity.magnitude < 2f)
            StopSlide(); 
                       
    }

    private void UpdateColliderHeight()
    {
        if(pm.isGrounded)
        {
            capsuleCollider.height = Mathf.Lerp(capsuleCollider.height, heightTarget, Time.deltaTime * crouchSmooth);
            cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, new Vector3(0, cameraHeightTarget, 0), Time.deltaTime * crouchSmooth);

            if(!pm.sliding && autoDetectCeiling)
                AutoDetectCeiling();
            
            if (pm.underCeiling)
                UpdateCeilingHeight();
        }
    }

    private void MyInput()
    {   
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && !pm.sliding && rigidBody.velocity.magnitude > 10f && readyToSlide)
            StartSlide();

        if (Input.GetKeyUp(slideKey) && pm.sliding)
            StopSlide();
    }   

    private void FixedUpdate()
    {
        if (pm.sliding)
            SlidingMovement();
    }

    private void StartSlide()
    {
        pm.sliding = true;
        readyToSlide = false;
        heightTarget = crouchHeight;
        cameraHeightTarget = heightTarget * 0.3f;

        if(pm.isGrounded)
            audioManager.PlaySFX(audioManager.slideSound);

        rigidBody.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {   
        if(!pm.OnSlope() | rigidBody.velocity.y > -0.1f)
            slideTimer -= Time.deltaTime;
        else
            // If on slope, accelerate player going down
            rigidBody.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
    }

    // Garbage complicated unoptimized code (somehow works), do not touch!
    private void AutoDetectCeiling()
    {
        var castOrigin = transform.position; // + orientation.forward
        if(Physics.Raycast(castOrigin, Vector3.up, out RaycastHit hit, 1f, excludedLayerMask))
        {   
            if (!pm.underCeiling && CheckCeiling()) pm.underCeiling = true;
                    
            if (autoCheckCeiling && !pm.underCeiling)
            {
                autoCheckCeiling = false;
                heightTarget = crouchHeight;
                cameraHeightTarget = heightTarget * 0.3f;
            }
        }
        else if (!pm.underCeiling)
        {
            autoCheckCeiling = true;
            heightTarget = startHeight;
            cameraHeightTarget = heightTarget * 0.3f;
        }
    }

    private bool CheckCeiling()
    {   
        var castOrigin = transform.position;
        // This  SphereCast which has garbage documentation and no DrawSphere function, 
        // I still have no idea how it works, but hey, it somehow detects ceiling :)
        if (Physics.SphereCast(castOrigin, capsuleCollider.radius, Vector3.up, out RaycastHit hit, 1f))
            return true;
        return false;
    }

    private void UpdateCeilingHeight()
    {
        var castOrigin = transform.position;

        // Spehere cast for some reason is innacurate
        if (!Physics.SphereCast(castOrigin, capsuleCollider.radius, Vector3.up, out RaycastHit hit, 1f, excludedLayerMask) && !Physics.Raycast(castOrigin, Vector3.up, 1f, excludedLayerMask))
        {
            pm.underCeiling = false;
            ForceUpdateSpeed();

            heightTarget = startHeight;
            cameraHeightTarget = heightTarget * 0.3f;
 
        } else if (autoCheckCeiling) {

            pm.underCeiling = true;
            autoCheckCeiling = false;

            heightTarget = crouchHeight;
            cameraHeightTarget = heightTarget * 0.3f;
        }
    }

    private void ForceUpdateSpeed()
    {
        pm.desiredMoveSpeed = pm.walkSpeed;
        pm.moveSpeed = pm.walkSpeed;
    }

    private void ResetSlide()
    {   
        readyToSlide = true;
    }

    public void StopSlide()
    {   
        pm.sliding = false;
        Invoke(nameof(ResetSlide), slideCooldown);

        // When you are sliding till you stop, reset speed
        if(pm.moveSpeed < 12f)
            ForceUpdateSpeed();

        if (CheckCeiling())
        {
            UpdateCeilingHeight();
            return;
        }

        heightTarget = startHeight;
        cameraHeightTarget = heightTarget * 0.3f;
    }

    void OnCollisionEnter(Collision other)
    {
        if (pm.sliding && pm.moveSpeed > 12f && other.gameObject.name == "EnemyObject")
        {
            var atm = other.transform.GetComponent<AttributesManager>();
            atm.DealDamage(atm.gameObject, 50);
            audioManager.PlaySFX(audioManager.slideHit);

            playerHealth.AddHP(Math.Min(playerHealth.health + 10, playerScript.maxHealth));
        }
    }
}
