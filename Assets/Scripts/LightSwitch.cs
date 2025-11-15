using UnityEngine;

public class LightSwitch : InteractableObject
{
    [Header("Lights")]
    public Light[] lightsToControl;

    [Header("State")]
    public bool isOn = true;

    [Header("Guard Settings")]
    public float callGuardRange = 15f; // радиус реакции охранников

    void Start()
    {
        RefreshLights();
    }

    // ✅ Игрок взаимодействует (через PlayerController)
    public override void Interact(PlayerController player)
    {
        if (!isOn) return;
        TurnOff();
    }

    // ✅ Выключение света
    public void TurnOff()
    {
        isOn = false;
        RefreshLights();

        GuardController[] guards = FindObjectsOfType<GuardController>();
        GuardController nearest = null;
        float bestDist = Mathf.Infinity;

        foreach (var g in guards)
        {
            if (g == null) continue;

            float dist = Vector3.Distance(g.transform.position, transform.position);
            if (dist < bestDist && dist <= callGuardRange)
            {
                bestDist = dist;
                nearest = g;
            }
        }

        if (nearest != null)
        {
            // offset — чтобы цель была на NavMesh, а не в стене
            Vector3 targetPos = GetNavMeshTargetPosition();
            nearest.GoToLightSwitch(targetPos, this);
        }
    }

    // ✅ Включение света охранником
    public void TurnOn()
    {
        isOn = true;
        RefreshLights();
    }

    // ✅ Сдвигаем цель на NavMesh
    public Vector3 GetNavMeshTargetPosition()
    {
        return transform.position + transform.forward * 0.5f;
    }

    private void RefreshLights()
    {
        if (lightsToControl == null) return;

        foreach (var l in lightsToControl)
            if (l != null)
                l.enabled = isOn;
    }
}
