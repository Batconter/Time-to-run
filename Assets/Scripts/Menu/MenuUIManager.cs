using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // для работы с Image у кнопки

public class MenuUIManager : MonoBehaviour
{
    [Header("Main Buttons")]
    public GameObject playButton;
    public GameObject rulesButton;
    public GameObject exitButton;
    public GameObject soundButton; // должна иметь компонент Image для изменения цвета

    [Header("Stickers")]
    public GameObject yellowSticker;
    public GameObject pinkSticker;

    [Header("Panels")]
    public GameObject mailPanel;
    public GameObject exitPanel;

    [Header("Sound Settings")]
    public Color soundOnColor = Color.white;
    public Color soundOffColor = Color.gray;

    private bool soundEnabled = true; // звук включён по умолчанию
    private Image soundButtonImage;

    void Awake()
    {
        if (soundButton != null)
        {
            soundButtonImage = soundButton.GetComponent<Image>();
            if (soundButtonImage != null)
                soundButtonImage.color = soundOnColor; // начальный цвет
        }
    }

    // ─────────────────────────────────────────────
    // КНОПКИ ГЛАВНОГО МЕНЮ
    // ─────────────────────────────────────────────

    public void OnPlayClicked()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnRulesClicked()
    {
        playButton.SetActive(false);
        rulesButton.SetActive(false);
        exitButton.SetActive(false);
        soundButton.SetActive(false);

        yellowSticker.SetActive(false);
        pinkSticker.SetActive(false);

        mailPanel.SetActive(true);
    }

    public void OnExitClicked()
    {
        playButton.SetActive(false);
        rulesButton.SetActive(false);
        exitButton.SetActive(false);
        soundButton.SetActive(false);

        yellowSticker.SetActive(false);
        pinkSticker.SetActive(false);

        exitPanel.SetActive(true);
    }

    // ─────────────────────────────────────────────
    // КНОПКИ ПАНЕЛИ ПРАВИЛ
    // ─────────────────────────────────────────────

    public void OnCloseMail()
    {
        mailPanel.SetActive(false);

        playButton.SetActive(true);
        rulesButton.SetActive(true);
        exitButton.SetActive(true);
        soundButton.SetActive(true);

        yellowSticker.SetActive(true);
        pinkSticker.SetActive(true);
    }

    // ─────────────────────────────────────────────
    // КНОПКИ ПАНЕЛИ “ВЫЙТИ?”
    // ─────────────────────────────────────────────

    public void OnExitYes()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnExitNo()
    {
        exitPanel.SetActive(false);

        playButton.SetActive(true);
        rulesButton.SetActive(true);
        exitButton.SetActive(true);
        soundButton.SetActive(true);

        yellowSticker.SetActive(true);
        pinkSticker.SetActive(true);
    }

    // ─────────────────────────────────────────────
    // КНОПКА ЗВУКА
    // ─────────────────────────────────────────────

    public void OnSoundClicked()
    {
        soundEnabled = !soundEnabled;

        // включаем/выключаем все аудио в сцене
        AudioListener.volume = soundEnabled ? 1f : 0f;

        // меняем цвет кнопки для визуальной индикации
        if (soundButtonImage != null)
            soundButtonImage.color = soundEnabled ? soundOnColor : soundOffColor;
    }
}
