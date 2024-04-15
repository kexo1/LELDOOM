using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject objectToInstantiate;
    [SerializeField] private float timeBetweenChecks = 60f;
    private float nextCheckTime;

    private void Start()
    {
        nextCheckTime = Time.time + timeBetweenChecks;
    }

    private void Update()
    {
        
        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + timeBetweenChecks;

            int randomNumber = Random.Range(1, 3);
            if (randomNumber == 1)
            {
                SpawnObject();
            }
        }
    }

    private void SpawnObject()
    {
        // Check if the object has already been spawned
        if (transform.childCount == 0)
        {
            // Spawn the object as a child of the spawner
            Instantiate(objectToInstantiate, transform.position, Quaternion.identity, transform);
        }
    }
}
