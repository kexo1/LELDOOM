using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RagePickUp : MonoBehaviour
{
    [SerializeField] private int weaponDamageMultiplier;
    [SerializeField] private float effectTime;
    [SerializeField] Sprite piwoImage;
    [SerializeField] private GameObject piwoIndicator;
    private WeaponManager weaponManager;
    private AudioManager audioManager;
    private Image piwoIndicatorImage;
    private Renderer objectRenderer;
    private int defaultDamage;
    private GameObject parent;
    private bool isPickedUp = false;

    private void Start()
    {
        weaponManager = GameObject.Find("WeaponManager").GetComponent<WeaponManager>();
        audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
        piwoIndicatorImage = piwoIndicator.GetComponent<Image>();
        objectRenderer = GetComponent<Renderer>();
        parent = transform.parent.gameObject;
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
        
        defaultDamage = weaponManager.heldWeapon.damage;
        weaponManager.heldWeapon.damage *= weaponDamageMultiplier;

        audioManager.PlaySFX(audioManager.healthPickupSound);
        StartCoroutine(ResetDamageAndDestroy());
    }

    private IEnumerator ResetDamageAndDestroy()
    {
        yield return new WaitForSeconds(effectTime);

        // If player picked up another beer
        if (piwoImage.name == piwoIndicatorImage.sprite.name)
            piwoIndicator.SetActive(false);

        weaponManager.heldWeapon.damage = defaultDamage;

        Destroy(parent);
    }
}

