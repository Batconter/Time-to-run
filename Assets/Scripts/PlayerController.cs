using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public Transform cameraTransform;

    [Header("Interaction")]
    public float interactRange = 3f;
    public LayerMask interactableLayer = ~0;

    private CharacterController cc;
    private bool movementBlocked = false;
    private bool invisible = false;

    // ✅ ГРАВИТАЦИЯ
    private float verticalVelocity = 0f;
    private const float gravity = -20f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null) 
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (!movementBlocked)
            HandleMovement();

        if (!movementBlocked && Input.GetKeyDown(KeyCode.Space))
            TryInteractNearest();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // камеру выравниваем
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cameraTransform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 dir = (forward * v + right * h);
        if (dir.magnitude > 1f) dir.Normalize();

        Vector3 motion = dir * moveSpeed;

        // ✅ применяем гравитацию
        if (cc.isGrounded)
            verticalVelocity = -0.5f; // легкое прижатие к земле
        else
            verticalVelocity += gravity * Time.deltaTime;

        motion.y = verticalVelocity;

        cc.Move(motion * Time.deltaTime);
    }

    private void TryInteractNearest()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, interactRange, interactableLayer);

        InteractableObject nearest = null;
        float minDist = float.MaxValue;

        foreach (var c in cols)
        {
            if (c == null) continue;
            InteractableObject io = c.GetComponent<InteractableObject>();
            if (io == null) io = c.GetComponentInParent<InteractableObject>();
            if (io != null)
            {
                float d = Vector3.Distance(transform.position, io.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    nearest = io;
                }
            }
        }

        if (nearest != null)
            nearest.Interact(this);
    }

    public void SetMovementBlocked(bool blocked)
    {
        movementBlocked = blocked;
    }

    public void SetInvisible(bool state)
    {
        invisible = state;

        var meshRends = GetComponentsInChildren<MeshRenderer>(true);
        foreach (var r in meshRends)
            if (r != null) r.enabled = !state;

        var skinned = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (var r in skinned)
            if (r != null) r.enabled = !state;

        var sprites = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var s in sprites)
            if (s != null) s.enabled = !state;
    }

    public bool IsInvisible() => invisible;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
