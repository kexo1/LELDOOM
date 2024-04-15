using UnityEngine;

public class HealthInteract: MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerScript playerScript;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AttributesManager player;
    [SerializeField] private GameObject abilities;
    [SerializeField] private GameObject capybara;

    public string GetDescription() {
        return $"Add 10 to max Health. Current: ({playerScript.maxHealth})";
    }

    public void Interact() {
        playerScript.maxHealth = playerScript.maxHealth + 10;
        player.AddHP(playerScript.maxHealth);
        audioManager.PlaySFX(audioManager.nextWave);
        abilities.SetActive(false);
        capybara.SetActive(false);
    }
}
