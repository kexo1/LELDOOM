using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{   
    [Header("References")]
    [SerializeField] private Settings settings;

    [Header("TextMesh")]
    public TMP_Text _musicText; 
    public TMP_Text _sfxText; 
    public TMP_Text _fpsText;
    public TMP_Text _fovText;
    public TMP_Text _sensText;
    public TMP_Text _waveText;
    
    // So some variables which store integer require to
    // be saved usng prefs.save, but for whatever reason float doesn't need to be saved???

    private void Awake() 
    {
        Time.timeScale = 1;
        if (PlayerPrefs.HasKey("fullScreenValue"))
        {
            if (PlayerPrefs.GetInt("fullScreenValue") == 1)
                Screen.fullScreen = true;
            else
                Screen.fullScreen = false;
            settings.fullScreenToggle.isOn = Screen.fullScreen;
        }
        else
        {
            Screen.fullScreen = true;
            PlayerPrefs.SetInt("fullScreenValue", 1);
            PlayerPrefs.Save();
        }
    }

    private void Start()
    {   

        if (PlayerPrefs.HasKey("musicVolume"))
        {
            settings.musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
            settings.SetMusicVolume();
        }
        else
            settings.SetMusicVolume();


        if (PlayerPrefs.HasKey("sfxVolume"))
        {
            settings.sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");
            settings.SetSFXVolume();
        }
        else
            settings.SetSFXVolume();
            

        if (PlayerPrefs.HasKey("fpsValue"))
        {
            settings.fpsSlider.value = PlayerPrefs.GetInt("fpsValue");
            settings.SetFpsValue();
        }
        else
            settings.SetFpsValue();


        if (PlayerPrefs.HasKey("fovValue"))
        {
            settings.fovSlider.value = PlayerPrefs.GetInt("fovValue");
            settings.SetFovValue();
        }
        else
            settings.SetFovValue();


        if (PlayerPrefs.HasKey("sensValue"))
        {
            settings.sensSlider.value = PlayerPrefs.GetFloat("sensValue");
            settings.SetSensitivityValue();
        }
        else
            settings.SetSensitivityValue();


        if (PlayerPrefs.HasKey("vsyncValue"))
        {
            if (PlayerPrefs.GetInt("vsyncValue") == 1)
            {
                QualitySettings.vSyncCount = 1;
                settings.fpsSlider.interactable = false;
            }
            else
            {
                QualitySettings.vSyncCount = 0;
                settings.fpsSlider.interactable = true;
            }
            settings.vSyncToggle.isOn = !settings.fpsSlider.interactable;
        }
        else
            PlayerPrefs.SetInt("vsyncValue", 0);
            PlayerPrefs.Save();

        if (settings.postProccesing)
        {
            if (PlayerPrefs.HasKey("postProccesing"))
            {
                if (PlayerPrefs.GetInt("postProccesing") == 1)
                    settings.postProccesing.SetActive(true);
                else
                    settings.postProccesing.SetActive(false);

                settings.postProcessToggle.isOn = settings.postProccesing.activeSelf;

            }
            else
                PlayerPrefs.SetInt("postProccesing", 1);
                PlayerPrefs.Save();
        }
        
            
        if (settings.playerSpeed)
        {
            if (PlayerPrefs.HasKey("playerSpeed"))
            {
                if (PlayerPrefs.GetInt("playerSpeed") == 1)
                {
                    settings.playerSpeed.SetActive(true);
                }
                else
                {
                    settings.playerSpeed.SetActive(false);
                }
            }
            else
                PlayerPrefs.SetInt("playerSpeed", 0);
                PlayerPrefs.Save();
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadSceneAsync("Arena");
    }

    public void QuitGame()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }
}