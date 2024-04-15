using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    
    float xRotation;
    float yRotation;
    
    private void Start() 
    {   
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {   
        float sensitivity = PlayerPrefs.GetFloat("sensValue");
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensitivity;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
