using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public Camera mainCam;
    public float interactionDistance = 2f;

    public GameObject interactionUI;
    public GameObject informableUI;
    [SerializeField] private GameObject pauseMenu;
    public TMP_Text interactionText;
    public TMP_Text informableText;
    private bool hitInteractable;
    private bool hitInformable;
    
    private void Update()
    {
        InteractionRay();
    }

    void InteractionRay()
    {
        // If not paused
        if (!pauseMenu.activeSelf)
        {
            hitInteractable = false;
            hitInformable = false;

            Ray ray = mainCam.ViewportPointToRay(Vector3.one / 2f);

            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance)) {
                
                if (hit.collider.TryGetComponent<IInteractable>(out var interactable)) {
                    hitInteractable = true;
                    interactionText.text = interactable.GetDescription();

                    if (Input.GetKeyDown(KeyCode.E)) {
                        interactable.Interact();
                    }
                }

                if (hit.collider.TryGetComponent<IInformable>(out var informable)) {
                    hitInformable = true;
                    informableText.text = informable.GetDescription();
                }
            }
            
            informableUI.SetActive(hitInformable);
            interactionUI.SetActive(hitInteractable);

        } else {
            
            informableUI.SetActive(false);
            interactionUI.SetActive(false);
        }
        
    }
}