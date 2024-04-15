using UnityEngine;

public class ReloadSpeedInteract: MonoBehaviour, IInteractable
{
    private Weapon weapon;
    [SerializeField] private WeaponSwitching weaponSwitch;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GameObject abilities;
    [SerializeField] private GameObject weaponHolder;
    [SerializeField] private GameObject capybara;

    public string GetDescription() {
        return $"-{GetReloadValue()} seconds Reload time on {weapon.gameObject.name}. (Current: {weapon.reloadSpeed})";
    }

    public void Interact() {
        if (weapon.reloadSpeed - GetReloadValue() > GetMinReloadValue())
        {
            weapon.reloadSpeed = weapon.reloadSpeed - GetReloadValue();
            audioManager.PlaySFX(audioManager.nextWave);
            abilities.SetActive(false);
            capybara.SetActive(false);
        }
    }

    private void FixedUpdate() {
        weapon = weaponSwitch.heldWeapon;
    }

    float GetReloadValue()
    {
        if (weapon.gameObject.name == "Pump Action Shotgun")
            return 0.05f;
        else
            return 0.2f;
    }

    float GetMinReloadValue()
    {
        if (weapon.gameObject.name == "Pump Action Shotgun")
            return 0.1f;
        else
            return 0.3f;
    }
}
