using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpeedPickUp : MonoBehaviour
{
    [SerializeField] private float effectTime;
    [SerializeField] private float speedEffectSpeed;
    [SerializeField] private float speedEffectJumpForce;
    [SerializeField] Sprite piwoImage;
    [SerializeField] private GameObject piwoIndicator;
    private PlayerMovement playerMovement;
    private AudioManager audioManager;
    private Image piwoIndicatorImage;
    private Renderer objectRenderer;
    private float defaultSpeed;
    private float defaultJumpForce;
    private GameObject parent;
    private bool isPickedUp = false;

    private void Start()
    {
        playerMovement = GameObject.Find("Player").GetComponent<PlayerMovement>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        piwoIndicatorImage = piwoIndicator.GetComponent<Image>();
        objectRenderer = GetComponent<Renderer>();
        parent = transform.parent.gameObject;

        defaultSpeed = playerMovement.walkSpeed;
        defaultJumpForce = playerMovement.jumpForce;
    }

    private void OnTriggerEnter(Collider other)
    {   
        if (other.CompareTag("Player") && !isPickedUp)
        {   
            isPickedUp = true;
            PickUp();
        }
    }

    private void PickUp()
    {
        // Hide object and make it inactive
        objectRenderer.enabled = false;
        piwoIndicator.SetActive(true);
        piwoIndicatorImage.sprite = piwoImage;

        playerMovement.walkSpeed = speedEffectSpeed;
        playerMovement.desiredMoveSpeed = speedEffectSpeed;
        playerMovement.jumpForce = speedEffectJumpForce;

        audioManager.PlaySFX(audioManager.healthPickupSound);

        StartCoroutine(ResetSpeedAndDestroy());
    }

    private IEnumerator ResetSpeedAndDestroy()
    {
        yield return new WaitForSeconds(effectTime);
        
        // If player picked up another beer
        if (piwoImage.name == piwoIndicatorImage.sprite.name)
            piwoIndicator.SetActive(false);

        playerMovement.walkSpeed = defaultSpeed;
        playerMovement.jumpForce = defaultJumpForce;

        Destroy(parent);
    }
}

