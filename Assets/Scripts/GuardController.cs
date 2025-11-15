using UnityEngine;
using UnityEngine.AI;

public class GuardController : MonoBehaviour
{
    [Header("Vision")]
    public float viewDistance = 10f;
    public float viewAngle = 90f;
    public float loseSightTime = 2f;

    [Header("Movement")]
    public float patrolSpeed = 3f;
    public float chaseSpeed = 6f;

    [Header("Distraction")]
    public float ignoreDistractionRange = 3f;
    public float distractionInvestigateTime = 2f;

    [Header("Light Reaction")]
    public float lightCheckRange = 8f;  // зона, в которой охранник реагирует на выключенный свет

    [Header("Patrol")]
    public Transform[] patrolPoints;

    [Header("Catch Player")]
    public float catchDistance = 1.5f; // новая дистанция для поражения

    private NavMeshAgent agent;
    [SerializeField] private Transform player;

    private int patrolIndex = 0;

    // states
    private bool isChasing = false;
    private float lostSightTimer = 0f;

    private bool respondingToPhone = false;
    private Vector3 phonePosition;
    private float phoneTimer = 0f;

    private bool investigating = false;
    private Vector3 distractionPos;
    private float distractionTimer = 0f;

    // going to switch
    private bool goingToSwitch = false;
    private LightSwitch targetSwitch = null;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else Debug.LogWarning("[GuardController] Player not found by tag 'Player'");
        }

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            agent.speed = patrolSpeed;
            agent.SetDestination(patrolPoints[0].position);
        }
    }

    void Update()
    {
        if (player == null) return;

        bool seesPlayer = CanSeePlayer();

        // highest priority: going to switch
        if (goingToSwitch)
        {
            HandleLightSwitch();
            CheckCatchPlayer(); // добавлено
            return;
        }

        if (respondingToPhone)
        {
            HandlePhoneResponse(seesPlayer);
            CheckCatchPlayer(); // добавлено
            return;
        }

        if (investigating)
        {
            HandleInvestigation(seesPlayer);
            CheckCatchPlayer(); // добавлено
            return;
        }

        if (isChasing)
        {
            HandleChase(seesPlayer);
            CheckCatchPlayer(); // добавлено
            return;
        }

        // scan for nearby dark switches (optional passive check)
        ScanForDarkAreasPassive();

        Patrol();

        if (seesPlayer) StartChase();

        // проверка "поймал ли игрока" даже при патруле
        CheckCatchPlayer();
    }

    // -------------------
    // Public API methods
    // -------------------

    public void GoToLightSwitch(Vector3 pos, LightSwitch sw)
    {
        goingToSwitch = true;
        investigating = false;
        respondingToPhone = false;
        isChasing = false;

        targetSwitch = sw;
        distractionPos = pos;

        agent.speed = patrolSpeed;
        agent.SetDestination(distractionPos);
    }

    public void RespondToPhone(Vector3 phonePos, float stopDuration)
    {
        if (isChasing && Vector3.Distance(transform.position, player.position) < ignoreDistractionRange) return;

        goingToSwitch = false;
        investigating = false;

        respondingToPhone = true;
        phonePosition = phonePos;
        phoneTimer = 0f;
        agent.speed = patrolSpeed;
        agent.SetDestination(phonePosition);

        distractionInvestigateTime = stopDuration;
    }

    public void InvestigateDistraction(Vector3 pos, float effectiveRange)
    {
        if (isChasing && Vector3.Distance(transform.position, player.position) < ignoreDistractionRange) return;
        if (Vector3.Distance(transform.position, pos) > effectiveRange) return;

        goingToSwitch = false;
        respondingToPhone = false;

        investigating = true;
        distractionPos = pos;
        distractionTimer = 0f;
        agent.speed = patrolSpeed;
        agent.SetDestination(distractionPos);
    }

    // -------------------
    // Private behavior
    // -------------------

    private void HandleLightSwitch()
    {
        if (!agent.pathPending)
            agent.SetDestination(distractionPos);

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (targetSwitch != null)
            {
                targetSwitch.TurnOn();
                targetSwitch = null;
            }

            goingToSwitch = false;
            agent.speed = patrolSpeed;
            agent.SetDestination(GetReturnPosition());
        }
    }

    private void HandlePhoneResponse(bool seesPlayer)
    {
        var pc = player.GetComponent<PlayerController>();
        if (pc != null && pc.IsInvisible())
        {
            respondingToPhone = false;
            agent.SetDestination(GetReturnPosition());
            return;
        }

        if (seesPlayer)
        {
            respondingToPhone = false;
            StartChase();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            phoneTimer += Time.deltaTime;
            if (phoneTimer >= distractionInvestigateTime)
            {
                respondingToPhone = false;
                phoneTimer = 0f;
                agent.SetDestination(GetReturnPosition());
            }
        }
    }

    private void HandleInvestigation(bool seesPlayer)
    {
        var pc = player.GetComponent<PlayerController>();
        if (pc != null && pc.IsInvisible())
        {
            investigating = false;
            agent.SetDestination(GetReturnPosition());
            return;
        }

        if (seesPlayer)
        {
            investigating = false;
            StartChase();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            distractionTimer += Time.deltaTime;
            if (distractionTimer >= distractionInvestigateTime)
            {
                investigating = false;
                agent.SetDestination(GetReturnPosition());
            }
        }
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            agent.speed = patrolSpeed;
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        var pc = player.GetComponent<PlayerController>();
        if (pc != null && pc.IsInvisible()) return false;

        Vector3 dir = player.position - transform.position;
        float dist = dir.magnitude;
        if (dist > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle * 0.5f) return false;

        Vector3 origin = transform.position + Vector3.up * 0.6f;
        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, viewDistance))
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

    private void HandleChase(bool seesPlayer)
    {
        var pc = player.GetComponent<PlayerController>();
        if (pc != null && pc.IsInvisible())
        {
            StopChaseAndReturn();
            return;
        }

        agent.SetDestination(player.position);

        if (seesPlayer)
        {
            lostSightTimer = 0f;
            return;
        }

        lostSightTimer += Time.deltaTime;
        if (lostSightTimer >= loseSightTime)
        {
            StopChaseAndReturn();
        }
    }

    private void StopChaseAndReturn()
    {
        isChasing = false;
        lostSightTimer = 0f;
        agent.speed = patrolSpeed;
        agent.SetDestination(GetReturnPosition());
    }

    private Vector3 GetReturnPosition()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
            return patrolPoints[patrolIndex].position;
        return transform.position;
    }

    private float passiveScanTimer = 0f;
    private float passiveScanInterval = 1f;
    private void ScanForDarkAreasPassive()
    {
        passiveScanTimer += Time.deltaTime;
        if (passiveScanTimer < passiveScanInterval) return;
        passiveScanTimer = 0f;

        if (isChasing || respondingToPhone || investigating || goingToSwitch) return;

        LightSwitch[] switches = FindObjectsOfType<LightSwitch>();
        LightSwitch nearest = null;
        float best = Mathf.Infinity;

        foreach (var sw in switches)
        {
            if (sw == null) continue;
            if (sw.isOn) continue;
            float d = Vector3.Distance(transform.position, sw.transform.position);
            if (d < best && d <= lightCheckRange)
            {
                best = d;
                nearest = sw;
            }
        }

        if (nearest != null)
        {
            GoToLightSwitch(nearest.GetNavMeshTargetPosition(), nearest);
        }
    }

    // -------------------
    // Новый метод: проверка дистанции до игрока
    // -------------------
    private void CheckCatchPlayer()
    {
        if (Vector3.Distance(transform.position, player.position) <= catchDistance)
        {
            GameEvents.OnPlayerCaught?.Invoke();
        }
    }
}
