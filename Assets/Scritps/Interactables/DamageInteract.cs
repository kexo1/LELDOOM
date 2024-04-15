using UnityEngine;

public class DamageInteract: MonoBehaviour, IInteractable
{
    private Weapon weapon;
    [SerializeField] private WeaponSwitching weaponSwitch;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GameObject abilities;
    [SerializeField] private GameObject weaponHolder;
    [SerializeField] private GameObject capybara;

    public string GetDescription() {
        return $"Add {GetValue()} damage to {weapon.gameObject.name}. (Current: {weapon.damage})";
    }

    public void Interact() {
        weapon.damage = weapon.damage + GetValue();
        audioManager.PlaySFX(audioManager.nextWave);
        abilities.SetActive(false);
        capybara.SetActive(false);
    }

    private void FixedUpdate() {
        weapon = weaponSwitch.heldWeapon;
    }

    int GetValue()
    {
        if (weapon.gameObject.name == "PumpActionShotgun")
            return 5;
        else
            return 3;
    }
}
