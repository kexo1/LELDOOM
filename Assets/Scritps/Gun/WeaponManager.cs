using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class WeaponManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform playerCameraPos;
    [SerializeField] private Transform swayHolder;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera overlayCamera;
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Recoil recoilScript;
    [SerializeField] private PlayerMovement pm;
    [SerializeField] private GameObject backround;
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private GameObject informableUI;
    public Weapon heldWeapon;

    [Header("Sway")]
    [SerializeField] private float swaySize;
    [SerializeField] private float swayRotationSize;
    [SerializeField] private float swayMovementRotationSize;
    [SerializeField] private float swaySmooth;
    
    [Header("ViewBobbing")]
    [SerializeField] private float bobEffectIntensity;
    private float sinTime = 0f;
    private float sinAmountY;

    [Header("Fov")]
    public float scopedFov;
    public float fovSmooth;

    public TMP_Text ammoText;
    private bool justStartedMoving = false;
    private float horizontalInput;
    private float verticalInput;
    
    private void Update()
    {     
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (!backround.activeInHierarchy) 
        {
            ViewBobbing();
            SwayHolderUpdate();
            ApplyAllTranforms();
            WeaponScoping(); 
        } 
    }
    
    private void WeaponScoping()
    {
        if (!heldWeapon.scoping && !interactionUI.activeSelf && !informableUI.activeSelf)
            crosshairImage.gameObject.SetActive(true);
        else
            crosshairImage.gameObject.SetActive(false);

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, heldWeapon.scoping ? scopedFov : playerCamera.fieldOfView, fovSmooth * Time.deltaTime);
        overlayCamera.fieldOfView = Mathf.Lerp(overlayCamera.fieldOfView, heldWeapon.scoping ? scopedFov : playerCamera.fieldOfView, fovSmooth * Time.deltaTime);
    }

    private void ViewBobbing()
    {
        Vector3 inputVector = new(verticalInput, 0f, horizontalInput);
        float weaponLessHeight = 0.003f;
        float speed = inputVector.magnitude;

        if (speed > 0)
        {
            if (!justStartedMoving)
            {
                sinTime = 0f;
                justStartedMoving = true;
            }
            sinAmountY = -Mathf.Abs(bobEffectIntensity * Mathf.Sin(sinTime)) + weaponLessHeight;
            sinTime += Time.deltaTime * pm.moveSpeed / 2;
        }
        else
        {
            justStartedMoving = false;
            sinAmountY = Mathf.Lerp(sinAmountY, 0, Time.deltaTime * 10);
        }
    }

    private void SwayHolderUpdate() 
    {
        var mouseDelta = -new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        if (!heldWeapon.scoping)
            heldWeapon.transform.localRotation *= Quaternion.Euler(new Vector3(horizontalInput * swayMovementRotationSize, 0, 0));
    
        swayHolder.localPosition += (Vector3)mouseDelta * swaySize;
        swayHolder.localRotation *= Quaternion.Euler(swayRotationSize * new Vector3(0, 0, mouseDelta.x));
    }

    private void ApplyAllTranforms()
    {
        // 0.016 * 4 = 0.062
        swayHolder.SetLocalPositionAndRotation(Vector3.Lerp(swayHolder.localPosition, Vector3.zero, swaySmooth * Time.deltaTime), Quaternion.Slerp(swayHolder.localRotation, Quaternion.identity, swaySmooth * Time.deltaTime));
        heldWeapon.transform.localRotation = Quaternion.Lerp(heldWeapon.transform.localRotation, Quaternion.identity, 4 * Time.deltaTime);

        // View Bobbing
        if (!heldWeapon.scoping && pm.isGrounded && !pm.sliding)
            heldWeapon.transform.localPosition += Vector3.Lerp(new Vector3(0, sinAmountY, 0), Vector3.zero, 10 * Time.deltaTime);
    }

}
