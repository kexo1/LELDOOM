using UnityEngine;

public class CameraEffects : MonoBehaviour
{   
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private ParticleSystem speedEffect;

    [Header("Fov Settings")]
    [SerializeField] private float fovMultiplier;

    [Header("Camera Settings")]
    [SerializeField] private float cameraRotationSize;
    [SerializeField] private float cameraHeightTarget;

    [Header("Speed Effect")]
    [SerializeField] private float EmissionMultiplier;
    [SerializeField] private float EmissionBase;
    private float horizontalInput;

    [System.Obsolete]
    private void Update()
    {   
        horizontalInput = Input.GetAxisRaw("Horizontal");

        SpeedEffect();
        CameraRotation();
        FixCameraClipping();
    }

    [System.Obsolete]
    private void SpeedEffect()
    {
        float fovBase = PlayerPrefs.GetInt("fovValue");
        float rbSpeed = rigidBody.linearVelocity.magnitude;
        speedEffect.transform.SetPositionAndRotation(playerCamera.transform.position + playerCamera.transform.forward * 1.3f, playerCamera.transform.rotation);
        
        if (rbSpeed > 16f)
        {
            speedEffect.emissionRate = Mathf.Lerp(speedEffect.emission.rateOverTime.constant, EmissionBase + Mathf.Sqrt(rbSpeed - 16) * EmissionMultiplier, 10f * Time.deltaTime);
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fovBase + Mathf.Sqrt(rbSpeed - 16) * fovMultiplier, 10f * Time.deltaTime);
        }
        else
        {
            speedEffect.emissionRate = 0f;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fovBase, 10f * Time.deltaTime);
        }
    }   

    private void CameraRotation()
    {
        playerCamera.transform.localRotation = Quaternion.Slerp(playerCamera.transform.localRotation, Quaternion.identity, 5f * Time.deltaTime);
        playerCamera.transform.localRotation *= Quaternion.Euler(new Vector3(0 , 0, -1 * horizontalInput * cameraRotationSize));
    }

    private void FixCameraClipping()
    {
        cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, new Vector3(0, cameraHeightTarget, 0), Time.deltaTime * 4);

        if (rigidBody.linearVelocity.y < -0.1f)
            cameraHeightTarget = 0.6f;
        else
            cameraHeightTarget = 0.3f;

    }
}
