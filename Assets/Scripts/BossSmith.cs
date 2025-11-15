using UnityEngine;
using UnityEngine.AI;

public class BossSmith : MonoBehaviour
{
    [Header("Guard")]
    public Transform guardPoint;
    public float viewDistance = 10f;
    public float viewAngle = 90f;
    public float loseSightTime = 2f;
    public float chaseSpeed = 6f;
    public float patrolSpeed = 3f;
    public float catchDistance = 1.0f;

    private NavMeshAgent agent;
    private Transform player;
    private bool isChasing = false;
    private float lostSightTimer = 0f;

    private BossSwitch assignedSwitch = null;
    private bool goingToSwitch = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (guardPoint != null)
        {
            agent.speed = patrolSpeed;
            agent.SetDestination(guardPoint.position);
        }
    }

    void Update()
    {
        if (player == null) return;

        // If sees player -> immediate chase
        if (CanSeePlayer())
        {
            StartChase();
        }

        if (isChasing)
        {
            HandleChase();
            return;
        }

        if (goingToSwitch && assignedSwitch != null)
        {
            agent.SetDestination(assignedSwitch.transform.position);
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                assignedSwitch.TurnOnSwitch();
                assignedSwitch = null;
                goingToSwitch = false;
                agent.SetDestination(guardPoint.position);
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            agent.speed = patrolSpeed;
            if (guardPoint != null) agent.SetDestination(guardPoint.position);
        }
    }

    private bool CanSeePlayer()
    {
        var pc = player.GetComponent<PlayerController>();
        if (pc != null && pc.IsInvisible()) return false;

        Vector3 dir = player.position - transform.position;
        float dist = dir.magnitude;
        if (dist > viewDistance) return false;
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle * 0.5f) return false;

        if (Physics.Raycast(transform.position + Vector3.up * 0.6f, dir.normalized, out RaycastHit hit, viewDistance))
        {
            return hit.collider.CompareTag("Player");
        }
        return false;
    }

    private void StartChase()
    {
        isChasing = true;
        lostSightTimer = 0f;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    private void HandleChase()
    {
        var pc = player.GetComponent<PlayerController>();
        if (pc != null && pc.IsInvisible())
        {
            StopChaseAndReturn();
            return;
        }

        agent.SetDestination(player.position);

        if (Vector3.Distance(transform.position, player.position) <= catchDistance)
        {
            GameEvents.OnPlayerCaught?.Invoke();
            return;
        }

        if (!CanSeePlayer())
        {
            lostSightTimer += Time.deltaTime;
            if (lostSightTimer >= loseSightTime)
            {
                StopChaseAndReturn();
            }
        }
        else
        {
            lostSightTimer = 0f;
        }
    }

    private void StopChaseAndReturn()
    {
        isChasing = false;
        lostSightTimer = 0f;
        agent.speed = patrolSpeed;
        if (guardPoint != null) agent.SetDestination(guardPoint.position);
    }

    // Called by BossSwitch to assign this switch
    public void AssignSwitch(BossSwitch bs)
    {
        if (bs == null) return;
        assignedSwitch = bs;
        goingToSwitch = true;
    }
}
