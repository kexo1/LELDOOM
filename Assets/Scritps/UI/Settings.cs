using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private FpsLock fpsLock;
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private AttributesManager attributesManager;
    [SerializeField] private WaveSpawner waveSpawner;
    public GameObject playerSpeed;
    public GameObject postProccesing;

    [Header("Sliders/Buttons")]
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider fpsSlider;
    public Slider fovSlider;
    public Slider sensSlider;
    public Slider waveSlider;
    public Toggle fullScreenToggle;
    public Toggle vSyncToggle;
    public Toggle postProcessToggle;
    public Toggle playerSpeedToggle;
    public Toggle invincibilityToggle;

    public void SetVsync()
    {
        if (vSyncToggle.isOn is false)
        {
            QualitySettings.vSyncCount = 0;
            PlayerPrefs.SetInt("vsyncValue", 0);
            fpsSlider.interactable = true;
            PlayerPrefs.Save();
        }
        else
        {
            QualitySettings.vSyncCount = 1;
            PlayerPrefs.SetInt("vsyncValue", 1);
            fpsSlider.interactable = false;
            PlayerPrefs.Save();
        }
    }

    public void SetFullscreen()
    {
        if (fullScreenToggle.isOn is false)
        {
            Screen.fullScreen = false;
            PlayerPrefs.SetInt("fullScreenValue", 0);
            PlayerPrefs.Save();
        }
        else
        {
            Screen.fullScreen = true;
            PlayerPrefs.SetInt("fullScreenValue", 1);
            PlayerPrefs.Save();
        }
    }

    public void SetPostProcessing()
    {
        if (postProcessToggle.isOn is false)
        {
            postProccesing.SetActive(false);
            PlayerPrefs.SetInt("postProccesing", 0);
            PlayerPrefs.Save();
        }
        else
        {
            postProccesing.SetActive(true);
            PlayerPrefs.SetInt("postProccesing", 1);
            PlayerPrefs.Save();
        }
    }

    public void SetPlayerSpeed()
    {
        if (playerSpeedToggle.isOn is false)
        {
            playerSpeed.SetActive(false);
            PlayerPrefs.SetInt("playerSpeed", 0);
            PlayerPrefs.Save();
        }
        else
        {
            playerSpeed.SetActive(true);
            PlayerPrefs.SetInt("playerSpeed", 1);
            PlayerPrefs.Save();
        }
    }

    public void SetInvinciblity()
    {
        if (invincibilityToggle.isOn is false)
        {
            attributesManager.invincibility = false;
            PlayerPrefs.SetInt("invincibility", 0);
            PlayerPrefs.Save();
        }
        else
        {
            attributesManager.invincibility = true;
            PlayerPrefs.SetInt("invincibility", 1);
            PlayerPrefs.Save();
        }
    }

    public void SetFovValue()
    {
        int fovValue = (int)fovSlider.value;
        mainMenu._fovText.text = fovValue.ToString();
        PlayerPrefs.SetInt("fovValue", fovValue);
        PlayerPrefs.Save();
    }

    public void SetSensitivityValue()
    {
        float sensValue = sensSlider.value;
        mainMenu._sensText.text = Mathf.RoundToInt(sensValue).ToString();
        PlayerPrefs.SetFloat("sensValue", sensValue);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume()
    {
        float volume = sfxSlider.value;
        myMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        mainMenu._sfxText.text = Mathf.RoundToInt(volume * 100).ToString();
        PlayerPrefs.SetFloat("sfxVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        myMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        mainMenu._musicText.text = Mathf.RoundToInt(volume * 100).ToString();
        PlayerPrefs.SetFloat("musicVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetFpsValue()
    {
        float fpsValue = fpsSlider.value;
        fpsLock.frameRate = (int)fpsValue;
        mainMenu._fpsText.text = fpsLock.frameRate.ToString();
        PlayerPrefs.SetInt("fpsValue", fpsLock.frameRate);
        PlayerPrefs.Save();
    }

    public void SetWave(bool updateTextOnly)
    {   
        if (!updateTextOnly)
        {
            float waveValue = waveSlider.value;
            waveSpawner.nextWave = (int)waveValue;
        }
        
        mainMenu._waveText.text = (waveSpawner.nextWave + 1).ToString();
    }
}
