using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public WeaponManager weaponManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GameObject parent;

    private void OnTriggerEnter(Collider other)
    {   
        if (other.CompareTag("Player"))
        {   
            weaponManager.heldWeapon.allAmmo += weaponManager.heldWeapon.magSize;
            weaponManager.heldWeapon._ammoText.text = weaponManager.heldWeapon.ammo + " / " + weaponManager.heldWeapon.allAmmo;
            
            audioManager.PlaySFX(audioManager.ammoPickupSound);
            Destroy(parent);
        }
    }
}
