using UnityEngine;

public class MagSizeInteract: MonoBehaviour, IInteractable
{
    private Weapon weapon;
    [SerializeField] private WeaponSwitching weaponSwitch;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GameObject abilities;
    [SerializeField] private GameObject weaponHolder;
    [SerializeField] private GameObject capybara;

    public string GetDescription() {
        return $"Add {GetValue()} max Magazine size to {weapon.gameObject.name}. (Current: {weapon.magSize})";
    }

    public void Interact() 
    {
        weapon.magSize = weapon.magSize + GetValue();
        weapon.ammo = weapon.magSize;
        weapon._ammoText.text = $"{weapon.ammo} / {weapon.allAmmo}";
        audioManager.PlaySFX(audioManager.nextWave);
        abilities.SetActive(false);
        capybara.SetActive(false);
    }

    private void FixedUpdate() {
        weapon = weaponSwitch.heldWeapon;
    }

    int GetValue()
    {
        if (weapon.gameObject.name == "AK-47" || weapon.gameObject.name == "Tommy Gun" || weapon.gameObject.name == "Glock")
            return 5;
        else if (weapon.gameObject.name == "Pump Action Shotgun" || weapon.gameObject.name == "Malorian")
            return 1;
        else
            return 10;
    }
}
