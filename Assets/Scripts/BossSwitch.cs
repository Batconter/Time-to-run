using UnityEngine;

public class BossSwitch : InteractableObject
{
    [Header("Lights")]
    public Light[] lightsToControl;

    [Header("State")]
    public bool switchIsOn = true;

    [Header("Assigned Guard")]
    public BossSmith assignedGuard;

    private void Start()
    {
        RefreshLights();
    }

    // ✅ Этот выключатель используется через Interact(), как всё остальное
    public override void Interact(PlayerController player)
    {
        if (!switchIsOn) return;
        TurnOffSwitch();
    }

    public void TurnOffSwitch()
    {
        switchIsOn = false;
        RefreshLights();

        if (assignedGuard != null)
            assignedGuard.AssignSwitch(this);
    }

    public void TurnOnSwitch()
    {
        switchIsOn = true;
        RefreshLights();
    }

    private void RefreshLights()
    {
        if (lightsToControl == null) return;
        foreach (var l in lightsToControl)
            if (l != null)
                l.enabled = switchIsOn;
    }

    // ✅ Очень важно: точка на полу, чтобы босс шёл по NavMesh
    public Vector3 GetNavMeshPoint()
    {
        return new Vector3(transform.position.x, 0.05f, transform.position.z);
    }
}
