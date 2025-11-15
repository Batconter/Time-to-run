using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GameObject))]
public class DefeatHandler : MonoBehaviour
{
    [Header("UI")]
    public GameObject defeatPanel;     // назначь свою панель поражения в инспекторе

    [Header("Переход")]
    public float delayBeforeMenu = 2f; // сколько секунд показываем панель перед переходом

    private void OnEnable()
    {
        GameEvents.OnPlayerCaught += OnDefeat;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerCaught -= OnDefeat;
    }

    private void Start()
    {
        if (defeatPanel != null)
            defeatPanel.SetActive(false);
    }

    private void OnDefeat()
    {
        // Включаем панель
        if (defeatPanel != null)
            defeatPanel.SetActive(true);

        // Попробуем заблокировать игрока (если объект игрока есть и у него PlayerController)
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.SetMovementBlocked(true);
        }

        // Через delay — в меню
        Invoke(nameof(LoadMenu), delayBeforeMenu);
    }

    private void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
