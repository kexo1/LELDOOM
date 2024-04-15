using UnityEngine;

public class JumpScript : MonoBehaviour
{
    public float jumpForce = 10.0f;
    public float randomRotationRange = 30.0f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 20.0f))
            {
                if (hit.collider.gameObject.layer == 8)
                {
                    Rigidbody otherRb = hit.collider.gameObject.GetComponentInParent<Rigidbody>();
                    if (otherRb != null)
                    {
                        otherRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                        otherRb.AddTorque(Random.insideUnitSphere * randomRotationRange);
                    }
                }
            }
        }
    }
}