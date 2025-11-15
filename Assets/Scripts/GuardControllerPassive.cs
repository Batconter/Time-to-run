using UnityEngine;
using UnityEngine.AI;

public class GuardControllerPassive : MonoBehaviour
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
    public float lightCheckRange = 8f;

    [Header("Return Point")]
    public Transform guardPoint; // точка, куда возвращается караульный

    private NavMeshAgent agent;
    private Transform player;

    private bool isChasing = false;
    private float lostSightTimer = 0f;

    private bool respondingToPhone = false;
    private Vector3 phonePosition;
    private float phoneTimer = 0f;

    private bool investigating = false;
    private Vector3 distractionPos;
    private float distractionTimer = 0f;

    private bool goingToSwitch = false;
    private LightSwitch targetSwitch = null;

    private float passiveScanTimer = 0f;
    private float passiveScanInterval = 1f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (guardPoint == null)
            guardPoint = this.transform; // если не задана, возврат в стартовую позицию
    }

    void Update()
    {
        if (player == null) return;

        bool seesPlayer = CanSeePlayer();

        // проверка на выключенный свет (периодически)
        ScanForDarkAreasPassive();

        if (goingToSwitch)
        {
            HandleLightSwitch();
            return;
        }

        if (respondingToPhone)
        {
            HandlePhoneResponse(seesPlayer);
            return;
        }

        if (investigating)
        {
            HandleInvestigation(seesPlayer);
            return;
        }

        if (isChasing)
        {
            HandleChase(seesPlayer);
            return;
        }

        // если видит игрока — начать погоню
        if (seesPlayer)
        {
            StartChase();
            return;
        }

        // иначе — стоять или возвращаться на пост
        if (!agent.pathPending && agent.remainingDistance > 0.5f)
            agent.SetDestination(GetReturnPosition());
    }

    // === Реакции ===

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

    // === Обработчики ===

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

    private void HandleChase(bool seesPlayer)
    {
        agent.SetDestination(player.position);

        if (seesPlayer)
        {
            lostSightTimer = 0f;
            return;
        }

        lostSightTimer += Time.deltaTime;
        if (lostSightTimer >= loseSightTime)
            StopChaseAndReturn();
    }

    // === Служебные методы ===

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dir = player.position - transform.position;
        float dist = dir.magnitude;
        if (dist > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > viewAngle / 2f) return false;

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

    private void StopChaseAndReturn()
    {
        isChasing = false;
        lostSightTimer = 0f;
        agent.speed = patrolSpeed;
        agent.SetDestination(GetReturnPosition());
    }

    private Vector3 GetReturnPosition()
    {
        if (guardPoint != null) return guardPoint.position;
        return transform.position;
    }

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
            if (sw == null || sw.isOn) continue;
            float d = Vector3.Distance(transform.position, sw.transform.position);
            if (d < best && d <= lightCheckRange)
            {
                best = d;
                nearest = sw;
            }
        }

        if (nearest != null)
            GoToLightSwitch(nearest.GetNavMeshTargetPosition(), nearest);
    }
}
