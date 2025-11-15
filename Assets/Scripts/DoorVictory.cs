using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;  // добавлено для IEnumerator

public class DoorVictory : MonoBehaviour
{
    [Header("UI победы")]
    public GameObject victoryPanel;

    [Header("Настройки перехода")]
    public float delayBeforeMenu = 2f;  // через сколько секунд перейти в меню

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        // Показываем панель победы
        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        // Блокируем движение игрока
        var pc = other.GetComponent<PlayerController>();
        if (pc != null) pc.SetMovementBlocked(true);

        // Фризим весь мир
        Time.timeScale = 0f;

        // Запускаем переход в меню с учётом реального времени
        StartCoroutine(GoToMenuDelayed());
    }

    private IEnumerator GoToMenuDelayed()
    {
        // Ждем реальное время, игнорируя Time.timeScale
        yield return new WaitForSecondsRealtime(delayBeforeMenu);

        // Перед загрузкой меню возобновляем время
        Time.timeScale = 1f;

        SceneManager.LoadScene("Menu");
    }
}
