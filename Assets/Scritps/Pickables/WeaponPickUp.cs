using UnityEngine;

public class WeaponPickUp : MonoBehaviour
{
    [SerializeField] private WeaponSwitching weaponSwitching;
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GameObject malorian;
    [SerializeField] private GameObject malorianAnimator;
    [SerializeField] private GameObject weaponHolder;
    [SerializeField] private GameObject door;
    [SerializeField] private GameObject player;

    private void OnTriggerEnter(Collider other)
    {   
        if (other.CompareTag("Player"))
        {   
            malorian.transform.parent = weaponHolder.transform;
            weaponSwitching.SetWeapons();
            weaponSwitching.Select(1);
            // Show player he got new weapon
            StartCoroutine(waveSpawner.WeaponPopUp());

            door.SetActive(true);
            player.transform.position = new Vector3(0, 1.05f, 0);
            audioManager.PlaySFX(audioManager.secretFound);

            //Destroy(malorianAnimator.GetComponent<Animator>());
            Destroy(gameObject, 6);
        }
    }
}
