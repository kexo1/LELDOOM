using UnityEngine;

public class RadioSound : MonoBehaviour
{
    [SerializeField] private AudioClip radioStatic;
    [SerializeField] private AudioClip magyar;

    void Start()
    {
        int randomNumber = Random.Range(1, 11);
        AudioSource radioAudioSource = GetComponent<AudioSource>();

        if (randomNumber == 1)
            radioAudioSource.clip = magyar;
        else
            radioAudioSource.clip = radioStatic;

        radioAudioSource.Play();
    }

}
