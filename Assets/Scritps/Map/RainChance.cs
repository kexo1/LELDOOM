using UnityEngine;

public class RainCHance : MonoBehaviour
{
    [SerializeField] private GameObject rain;

    void Start()
    {
        int randomNumber = Random.Range(1, 4);
        if (randomNumber < 3)
            rain.SetActive(false);
    }
}
