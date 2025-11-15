using UnityEngine;
using System.Collections;

public class Phone : InteractableObject
{
    [Header("Phone Settings")]
    public float callRadius = 15f;          // радиус, в котором охранники услышат звонок
    public float phoneStopDuration = 2f;    // сколько охранник стоит у телефона

    [Header("Audio")]
    public AudioSource ringSource;          // 🔊 источник звука (звонок)

    public override void Interact(PlayerController player)
    {
        // 🔊 запускаем звук звонящего телефона
        if (ringSource != null)
            ringSource.Play();

        StartCoroutine(CallNearestPhoneForEachGuard());
    }

    private IEnumerator CallNearestPhoneForEachGuard()
    {
        // Получаем список всех телефонов на сцене
        Phone[] allPhones = FindObjectsOfType<Phone>();

        // Получаем список всех охранников
        GuardController[] guards = FindObjectsOfType<GuardController>();

        foreach (var guard in guards)
        {
            if (guard == null) continue;

            // Проверяем, в зоне ли он слышимости
            if (Vector3.Distance(guard.transform.position, transform.position) > callRadius)
                continue;

            // Ищем ближайший телефон к ЭТОМУ конкретному охраннику
            Phone nearest = null;
            float bestDist = Mathf.Infinity;

            foreach (var ph in allPhones)
            {
                if (ph == null) continue;
                float d = Vector3.Distance(guard.transform.position, ph.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    nearest = ph;
                }
            }

            if (nearest != null)
            {
                // Охранник идёт к своему ближайшему телефону
                guard.RespondToPhone(nearest.transform.position, phoneStopDuration);
            }
        }

        yield return null;
    }
}
