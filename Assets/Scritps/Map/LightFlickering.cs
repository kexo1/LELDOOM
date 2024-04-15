using System.Collections;
using UnityEngine;

public class BrokenLight : MonoBehaviour
{
    [SerializeField] private float minDelay = 0.1f;
    [SerializeField] private float maxDelay = 0.5f; 

    private Light pointLight;

    void Start()
    {
        pointLight = GetComponentInChildren<Light>();
        StartCoroutine(Flicker());
    }

    IEnumerator Flicker()
    {
        while (true)
        {
            pointLight.enabled = !pointLight.enabled;
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        }
    }
}
