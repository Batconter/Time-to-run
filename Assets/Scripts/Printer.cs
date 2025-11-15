using UnityEngine;
using TMPro;
using System.Text;

public class Printer : InteractableObject
{
    [Header("UI")]
    public GameObject uiPanel;      
    public TMP_Text instructionText;
    public TMP_Text targetText;    
    public TMP_Text typedText;      

    [Header("Settings")]
    public float inactivityTimeout = 3f;
    public string idleColorHex = "#FFF493";
    public string correctColorHex = "#00FF00";
    public string errorColorHex = "#FF0000";

    [TextArea(2, 6)]
    public string[] phrases =
    {
        "Бжж...я принтер",
        "Бип-пуп-пип. Печать",
        "Пи-пи-пи - смените картридж",
        "Ошибка 405918472",
        "Тетрагидропиранилциклопентилтетрагидропиридопиридиновые свечи"
    };

    // runtime
    private PlayerController player;
    private bool active = false;
    private string currentPhrase;
    private string typed = "";
    private float idleTimer = 0f;

    // важно — чтобы не ловить пробел, вызвавший Interact()
    private bool justOpened = false;

    public override void Interact(PlayerController player)
    {
        if (active) return;
        this.player = player;
        OpenMiniGame();
    }

    private void OpenMiniGame()
    {
        active = true;
        typed = "";
        idleTimer = 0f;

        currentPhrase = phrases[Random.Range(0, phrases.Length)];

        if (uiPanel != null) uiPanel.SetActive(true);

        if (instructionText != null)
            instructionText.text = "Если буду повторять звуки принтера — останусь невидимым";

        if (targetText != null)
            targetText.text = $"<color={idleColorHex}>{currentPhrase}</color>";

        if (typedText != null)
            typedText.text = "";

        if (player != null)
        {
            player.SetMovementBlocked(true);
            player.SetInvisible(true);
        }

        // пропускаем ввод в кадре открытия
        justOpened = true;
    }

    void Update()
    {
        if (!active) return;

        // панель должна всегда быть активна
        if (uiPanel != null && !uiPanel.activeInHierarchy)
        {
            uiPanel.SetActive(true);
        }

        // первый кадр — пропускаем input
        if (justOpened)
        {
            justOpened = false;
            return;
        }

        idleTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Escape) || idleTimer >= inactivityTimeout)
        {
            CloseMiniGame();
            return;
        }

        string s = Input.inputString;
        if (!string.IsNullOrEmpty(s))
        {
            idleTimer = 0f;

            foreach (char c in s)
            {
                if (c == '\b')
                {
                    if (typed.Length > 0)
                        typed = typed.Substring(0, typed.Length - 1);
                }
                else
                {
                    typed += c;
                }
            }

            CheckTyped();
        }
    }

    private void CheckTyped()
    {
        if (typed.Length > currentPhrase.Length)
        {
            ShowErrorAndClose();
            return;
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < currentPhrase.Length; i++)
        {
            if (i < typed.Length)
            {
                if (typed[i] == currentPhrase[i])
                {
                    sb.Append($"<color={correctColorHex}>{typed[i]}</color>");
                }
                else
                {
                    sb.Append($"<color={errorColorHex}>{typed[i]}</color>");
                    ShowErrorAndClose();
                    return;
                }
            }
            else
            {
                sb.Append($"<color={idleColorHex}>{currentPhrase[i]}</color>");
            }
        }

        if (typedText != null)
            typedText.text = sb.ToString();

        if (typed == currentPhrase)
        {
            CloseMiniGame();
        }
    }

    private void ShowErrorAndClose()
    {
        if (typedText != null)
            typedText.text = $"<color={errorColorHex}>{currentPhrase}</color>";

        CloseMiniGame();
    }

    private void CloseMiniGame()
    {
        active = false;
        typed = "";
        idleTimer = 0f;

        if (uiPanel != null)
            uiPanel.SetActive(false);

        if (instructionText != null) instructionText.text = "";
        if (targetText != null) targetText.text = "";
        if (typedText != null) typedText.text = "";

        if (player != null)
        {
            player.SetMovementBlocked(false);
            player.SetInvisible(false);
        }
    }
}
