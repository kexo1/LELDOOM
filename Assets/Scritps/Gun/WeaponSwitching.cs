using TMPro;
using UnityEngine;


public class WeaponSwitching : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCameraPos;
    public TMP_Text ammoText;
    public Weapon heldWeapon;

    [Header("Keys")]
    [SerializeField] private KeyCode[] keys;

    [Header("Settings")]
    [SerializeField] private float switchTime;

    private int selectedWeapon = 0;
    private Transform[] weapons;

    private void Start() 
    {
        SetWeapons();
        Select(selectedWeapon);
    }

    private void Update() 
    {
        int previousSelectedWeapon = selectedWeapon;
        for (int i = 0; i < weapons.Length; i++)
        {
            if (Input.GetKeyDown(keys[i]))
                selectedWeapon = i;
        }

        // Mouse scroll input for weapon switching
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            int weaponDelta = (int)Mathf.Sign(scroll);
            int newWeaponIndex = selectedWeapon + weaponDelta;

            newWeaponIndex = Mathf.Clamp(newWeaponIndex, 0, weapons.Length - 1);

            if (newWeaponIndex != selectedWeapon)
                selectedWeapon = newWeaponIndex;
        }

        if (previousSelectedWeapon != selectedWeapon) Select(selectedWeapon);
    }

    public void SetWeapons()
    {
        weapons = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
            weapons[i] = transform.GetChild(i);

        keys ??= new KeyCode[weapons.Length];
    }

    public void Select(int weaponIndex)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (i == weaponIndex)
            {
                weapons[i].gameObject.SetActive(true);
                heldWeapon = weapons[i].GetComponent<Weapon>();
                heldWeapon.Switch(transform, playerCameraPos, ammoText);
            }
            else
                weapons[i].gameObject.SetActive(false);
        }
    }
}
