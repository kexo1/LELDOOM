using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public TMP_Text _healthText;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject backround;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private GameObject youDiedText;
    [SerializeField] private GameObject respawnButton;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject weaponPopUp;
    [SerializeField] private GameObject youSurvived;
    [SerializeField] private GameObject timePlayed;
    [SerializeField] private GameObject uiElements;
    [SerializeField] private Volume postProccesing;
    [SerializeField] private TMP_Text _timePlayedText;
    [SerializeField] private AttributesManager attributesManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private Button optionsButton;
    
    private int healthValue;
    private bool playerDied = false;
    private bool playerSurvived = false;

    void Start()
    {
        Time.timeScale = 1;
        healthValue = playerObject.GetComponent<AttributesManager>().health;
        _healthText.text = attributesManager.health.ToString();
    }

    public void Health_Update(int health)
    {
        _healthText.text = health.ToString();
        healthValue = health;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !playerDied && !playerSurvived)
        {
            if (backround.activeInHierarchy)
                Resume();
            else
                Pause();
        }

        if (healthValue <= 30)
        {
            if (postProccesing.profile.TryGet(out Vignette vignette))
            {
                vignette.color.value = Color.red;
                vignette.intensity.value = Mathf.Lerp(0.6f, 0.1f, Mathf.InverseLerp(0, 30, healthValue));
            }
        } else {
            if (postProccesing.profile.TryGet(out Vignette vignette))
            {
                vignette.color.value = Color.black;
                vignette.intensity.value = 0.3f;
            }
        }

        if (healthValue <= 0 && !playerDied)
        {
            playerDied = true;
            _healthText.text = "0";
            DeathMenu();
        }
    }

    private void DeathMenu()
    {
        Time.timeScale = 0;

        uiElements.SetActive(false);
        optionsMenu.SetActive(false);
        backround.SetActive(true);
        pauseMenu.SetActive(true);
        youDiedText.SetActive(true);
        respawnButton.SetActive(true);

        audioManager.StopMusic();
        audioManager.PlayMusicOnce(audioManager.youDied);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Pause()
    {
        Time.timeScale = 0;
        uiElements.SetActive(false);
        optionsMenu.SetActive(false);
        backround.SetActive(true);
        pauseMenu.SetActive(true);

        // This button forgets it's state so after you close options menu, 
        // and repopen menu, the button has colour like it's pressed. I have no clue
        // why it is the only button which does that.

        if (EventSystem.current.currentSelectedGameObject == optionsButton.gameObject)
            EventSystem.current.SetSelectedGameObject(null);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void MainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Main Menu");
    }

    public void Respawn()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
        SceneManager.LoadScene("Arena");
    }

    public void WinScreen(string time)
    {
        Time.timeScale = 0;

        uiElements.SetActive(false);
        optionsMenu.SetActive(false);
        backround.SetActive(true);
        pauseMenu.SetActive(true);
        youSurvived.SetActive(true);
        timePlayed.SetActive(true);

        playerSurvived = true;
        _timePlayedText.text = time;
        audioManager.StopMusic();
        audioManager.PlayMusicOnce(audioManager.youSurvived);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Resume()
    {
        Time.timeScale = 1;
        uiElements.SetActive(true);
        backround.SetActive(false);
        pauseMenu.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void QuitGame()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }
}
