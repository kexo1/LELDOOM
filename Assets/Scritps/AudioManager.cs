using UnityEngine;

public class AudioManager : MonoBehaviour
{   
    [Header("References")]
    public AudioSource musicScore;
    public AudioSource SFXSource;

    [Header("Music")]
    public AudioClip arenaSoundtrack;
    public AudioClip youSurvived;
    public AudioClip youDied;
    public AudioClip startSoundtrack;

    [Header("Player")]
    public AudioClip[] slideSound;
    public AudioClip[] landSound;
    public AudioClip[] ammoPickupSound;
    public AudioClip healthPickupSound;
    public AudioClip jumpPadSound;
    public AudioClip enemyHit;
    public AudioClip playerHit;
    public AudioClip slideHit;
    public AudioClip nextWave;
    public AudioClip secretFound;
    
    private void Start()
    {
        musicScore.clip = startSoundtrack;
        musicScore.Play();
    }

    public void StopMusic()
    {
        musicScore.Stop();
    }

    public void PlayMusic(AudioClip clip)
    {
        musicScore.clip = clip;
        musicScore.Play();
    }

    public void PlayMusicOnce(AudioClip clip)
    {
        musicScore.PlayOneShot(clip);
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    public void PlaySFX(params AudioClip[] clips)
    {
        SFXSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }
}
