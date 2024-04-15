using UnityEngine;

public class Secret : MonoBehaviour
{
    [SerializeField] AudioManager audioManager;
    [SerializeField] AudioClip sfx;
    private bool played = false;

    public void RunSecret()
    {
        if (!played)
        {
            played = true;
            audioManager.PlaySFX(sfx);
        }  
    }
}
