using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;

[RequireComponent(typeof(Collider))]
public class ComputerTerminalFull : InteractableObject
{
    // --- Глобальный прогресс ---
    private static int currentStage = 0;      // текущая стадия всех компьютеров
    private static bool isBusyGlobal = false;
    private static bool firstComputerInteracted = false;

    // --- Вопросы и ответы ---
    private static readonly string[] questions = new string[]
    {
        "Дата рождения вашего менеджера:",
        "Пароль от почты:",
        "Номер банковской карты:"
    };

    private static readonly string[] answers = new string[]
    {
        "ANY_DIGITS",          // любое число для первого вопроса
        "332378",
        "313943216512"
    };

    [Header("UI (назначить через инспектор)")]
    public GameObject uiPanel;
    public TMP_Text questionText;
    public TMP_Text underlineText;
    public TMP_InputField inputField;
    public TMP_Text hintText;
    public Button submitButton;

    [Header("Extra guards (настройка через инспектор)")]
    public GameObject[] extraGuards;
    public GameObject[] guardToRemovePerStage;

    [Header("Настройки")]
    public float inactivityTimeout = 5f; // таймер бездействия

    private float inactivityTimer = 0f;
    private PlayerController cachedPlayer;
    private int currentStageLocal;
    private bool miniGameActive = false;

    void Start()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
        if (submitButton != null)
        {
            submitButton.onClick.RemoveListener(OnSubmitPressed);
            submitButton.onClick.AddListener(OnSubmitPressed);
        }

        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    public override void Interact(PlayerController player)
    {
        if (isBusyGlobal) return;
        if (currentStage >= questions.Length) return;

        cachedPlayer = player;

        // Определяем стадию для этого компьютера
        if (!firstComputerInteracted)
        {
            currentStageLocal = 0;
            firstComputerInteracted = true;
        }
        else
        {
            currentStageLocal = currentStage;
        }

        OpenMiniGame();
    }

    private void OpenMiniGame()
    {
        isBusyGlobal = true;
        miniGameActive = true;
        inactivityTimer = 0f;

        if (uiPanel != null) uiPanel.SetActive(true);
        if (inputField != null)
        {
            inputField.text = "";
            inputField.ActivateInputField();
        }

        if (questionText != null) questionText.text = questions[currentStageLocal];

        // Подчёркивания
        int len = answers[currentStageLocal] == "ANY_DIGITS" ? 8 : answers[currentStageLocal].Length;
        if (underlineText != null) underlineText.text = new string('_', Mathf.Max(1, len));

        if (hintText != null)
            hintText.text = "Введите ответ и нажмите ОК. ESC или пустой ввод = отмена.";

        if (cachedPlayer != null) cachedPlayer.SetMovementBlocked(true);
        SetGuardsFrozen(true);
    }

    private void CloseMiniGame()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
        if (cachedPlayer != null) cachedPlayer.SetMovementBlocked(false);
        SetGuardsFrozen(false);
        miniGameActive = false;
        isBusyGlobal = false;
    }

    void Update()
    {
        if (!miniGameActive) return;

        inactivityTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMiniGame();
            return;
        }

        // Таймер бездействия
        if (inactivityTimer >= inactivityTimeout)
        {
            CloseMiniGame();
            return;
        }

        // Если игрок нажал Enter
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ProcessAnswer(inputField != null ? inputField.text.Trim() : "");
        }

        // Обновление подчёркиваний при вводе
        if (inputField != null && underlineText != null)
        {
            string input = inputField.text;
            int len = answers[currentStageLocal] == "ANY_DIGITS" ? 8 : answers[currentStageLocal].Length;
            char[] display = new string('_', len).ToCharArray();
            for (int i = 0; i < input.Length && i < display.Length; i++)
                display[i] = input[i];
            underlineText.text = new string(display);
        }

        // Сброс таймера при вводе
        if (Input.anyKeyDown)
        {
            inactivityTimer = 0f;
        }
    }

    private void OnSubmitPressed()
    {
        ProcessAnswer(inputField != null ? inputField.text.Trim() : "");
    }

    private void ProcessAnswer(string given)
    {
        if (string.IsNullOrEmpty(given))
        {
            CloseMiniGame();
            return;
        }

        string correct = answers[currentStageLocal];
        bool ok = false;

        if (correct == "ANY_DIGITS")
            ok = Regex.IsMatch(given, @"^[\d\.\-]+$"); // <-- цифры, точки и дефисы
        else
            ok = given == correct;

        if (ok) HandleCorrect();
        else HandleIncorrect();
    }

    private void HandleCorrect()
    {
        if (guardToRemovePerStage != null && currentStageLocal < guardToRemovePerStage.Length)
        {
            GameObject guardObj = guardToRemovePerStage[currentStageLocal];
            if (guardObj != null) guardObj.SetActive(false);
        }

        gameObject.SetActive(false);
        currentStage = Mathf.Min(currentStage + 1, questions.Length);

        CloseMiniGame();
    }

    private void HandleIncorrect()
    {
        if (extraGuards != null)
        {
            foreach (GameObject g in extraGuards)
            {
                if (g == null) continue;
                if (!g.activeSelf)
                {
                    g.SetActive(true);
                    break;
                }
            }
        }

        CloseMiniGame();
    }

    private void SetGuardsFrozen(bool freeze)
    {
        var guards = FindObjectsOfType<GuardController>();
        foreach (var g in guards)
        {
            if (g == null) continue;
            var agent = g.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (freeze)
            {
                if (agent != null) agent.isStopped = true;
                g.enabled = false;
            }
            else
            {
                g.enabled = true;
                if (agent != null) agent.isStopped = false;
            }
        }
    }

    void OnDestroy()
    {
        if (submitButton != null) submitButton.onClick.RemoveListener(OnSubmitPressed);
    }
}
