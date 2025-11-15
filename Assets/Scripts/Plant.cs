using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Plant : MonoBehaviour
{
    // Тег игрока (по умолчанию "Player")
    public string playerTag = "Player";

    void Reset()
    {
        // делаем триггер по умолчанию (удобно при добавлении компонента)
        var c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // пытаемся получить PlayerController у объекта игрока (в родителях/детях)
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) pc = other.GetComponentInChildren<PlayerController>();

        if (pc != null)
        {
            pc.SetInvisible(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) pc = other.GetComponentInChildren<PlayerController>();

        if (pc != null)
        {
            pc.SetInvisible(false);
        }
    }
}
