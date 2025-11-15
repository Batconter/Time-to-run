using UnityEngine;
using System.Text;
using UnityEngine.UI;

// Временный диагностический скрипт. ПРИМЕЧАНИЕ: удалите после отладки.
public class InteractionDebugger : MonoBehaviour
{
    public float testRange = 3f;
    public LayerMask layerMask = ~0;
    public Text debugUIText; // опционально: перетащи UI Text на Canvas чтобы видеть прямо на экране

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== Interaction Debug ===");
            sb.AppendLine($"Player pos: {transform.position}");
            sb.AppendLine($"interactRange: {testRange}");
            // 1) OverlapSphere collider check
            Collider[] cols = Physics.OverlapSphere(transform.position, testRange, layerMask);
            sb.AppendLine($"OverlapSphere found {cols.Length} colliders:");
            for (int i = 0; i < cols.Length; i++)
            {
                var c = cols[i];
                sb.AppendLine($"  {i}: {c.name} (layer {LayerMask.LayerToName(c.gameObject.layer)}) dist={(c.transform.position - transform.position).magnitude:F2}");
            }

            // 2) FindObjectsOfType InteractableObject check
            var ios = FindObjectsOfType<InteractableObject>();
            sb.AppendLine($"FindObjectsOfType<InteractableObject>() found {ios.Length}:");
            for (int i = 0; i < ios.Length; i++)
            {
                var io = ios[i];
                float d = Vector3.Distance(transform.position, io.transform.position);
                sb.AppendLine($"  {i}: {io.gameObject.name} dist={d:F2} activeSelf={io.gameObject.activeSelf} scriptEnabled={(io.enabled)}");
            }

            // 3) nearest candidate by distance
            InteractableObject nearest = null;
            float best = float.MaxValue;
            foreach (var io in ios)
            {
                if (!io.gameObject.activeInHierarchy) continue;
                float d = Vector3.Distance(transform.position, io.transform.position);
                if (d <= testRange && d < best)
                {
                    best = d; nearest = io;
                }
            }
            sb.AppendLine(nearest != null ? $"Nearest within range: {nearest.gameObject.name} dist={best:F2}" : "Nearest within range: NONE");

            Debug.Log(sb.ToString());

            if (debugUIText != null)
                debugUIText.text = sb.ToString();
        }
    }
}
