// InteractableObject.cs
using UnityEngine;

/// <summary>
/// Базовый класс для всех интерактивных объектов в проекте.
/// Сделан простым, безопасным и совместимым: метод Interact объявлен virtual.
/// Если в проекте уже есть свой InteractableObject — замени его на этот (или приведи метод Interact к virtual).
/// </summary>
public abstract class InteractableObject : MonoBehaviour
{
    /// <summary>
    /// Взаимодействие с игроком. Производные классы переопределяют этот метод через override.
    /// </summary>
    /// <param name="player">ссылка на PlayerController</param>
    public virtual void Interact(PlayerController player)
    {
        Debug.Log($"Interact called on {gameObject.name} (base InteractableObject).");
    }
}
