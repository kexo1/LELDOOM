using UnityEngine;

public class BunnyHopInteract: MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GameObject abilities;
    [SerializeField] private GameObject capybara;

    public string GetDescription() {
        return $"Add 1 to max BunnyHop speed. (Current: {playerMovement.maxJumpStrikes})";
    }

    public void Interact() {
        playerMovement.maxJumpStrikes += 1;
        audioManager.PlaySFX(audioManager.nextWave);
        abilities.SetActive(false);
        capybara.SetActive(false);
    }
}
