using System.Collections;
using UnityEngine;

public class EnemyZombieController : MonoBehaviour {
    [Header("HP")]
    [SerializeField] private float m_maximumHp;
    [SerializeField] private float m_currentHp;

    [Header("Sound Effects")]
    [SerializeField] private SoundEffectHandler m_headshotSoundFX;
    [SerializeField] private SoundEffectHandler m_moanSoundsFX;
    [SerializeField] private float m_moanInterval = 2f;
    [SerializeField] private float m_moanChance = 0.5f;

    [Header("Movement")]
    [SerializeField] private float m_moveSpeed = 1.5f;
    [SerializeField] private float m_wanderRadius = 5f;
    [SerializeField] private float m_waypointTolerance = 0.3f;
    [SerializeField] private float m_rotationSpeed = 3f;
    [SerializeField] private Vector2 m_pauseTimeRange = new Vector2(1f, 3f);

    [Header("Detection")]
    [SerializeField] private float m_detectionRange = 8f;
    [SerializeField] private float m_loseRange = 12f;
    [SerializeField] private float m_fieldOfView = 120f;
    [SerializeField] private float m_chaseSpeed = 3f;
    [SerializeField] private LayerMask m_obstacleLayer;

    [Header("Alerted")]
    [SerializeField] private float m_alertedSpeed = 2.5f;
    [SerializeField] private float m_alertedDetectionMultiplier = 1.3f;
    [SerializeField] private float m_alertedLookAroundTime = 2f;

    private enum ZombieState {
        Wander, Alerted, Chase
    }
    private ZombieState m_state = ZombieState.Wander;

    private Transform m_player;
    private Rigidbody2D m_rb;
    private PathFollower m_pathFollower;
    private Vector2 m_alertTarget;
    private Vector2 m_wanderTarget;
    private bool m_isPaused = false;
    private Vector2 m_lastKnownPlayerPosition;

    private void OnEnable() {
        GameEvents.Instance.onNoise += OnNoise;
    }

    private void OnDisable() {
        GameEvents.Instance.onNoise -= OnNoise;
    }

    private void Awake() {
        m_currentHp = m_maximumHp;
        m_rb = GetComponent<Rigidbody2D>();
        m_pathFollower = GetComponent<PathFollower>();
        m_wanderTarget = GetRandomWanderPoint();
        StartCoroutine(MoanRoutine());
    }

    private void Start() {
        m_player = GameObject.FindWithTag("Player").transform;
    }

    private void FixedUpdate() {
        if (m_isPaused && m_state == ZombieState.Wander) {
            m_rb.linearVelocity = Vector2.zero;
            return;
        }

        if (CanSeePlayer()) {
            m_lastKnownPlayerPosition = m_player.position; // aktualizuj gdy widzi
            m_state = ZombieState.Chase;
        } else if (m_state == ZombieState.Chase) {
            // zgubił gracza — idź do ostatniej znanej pozycji
            m_alertTarget = m_lastKnownPlayerPosition;
            m_state = ZombieState.Alerted;
        }

        switch (m_state) {
            case ZombieState.Wander:
                UpdateWander();
                break;
            case ZombieState.Alerted:
                UpdateAlerted();
                break;
            case ZombieState.Chase:
                UpdateChase();
                break;
        }
    }

    private void UpdateWander() {
        Vector2 dir = m_wanderTarget - (Vector2) transform.position;

        if (dir.magnitude < m_waypointTolerance) {
            StartCoroutine(PauseAndWander());
            return;
        }

        // wander bez pathfindingu — brak ścian w otwartej przestrzeni
        RotateTowards(dir);
        m_rb.linearVelocity = dir.normalized * m_moveSpeed;
    }

    private void UpdateAlerted() {
        m_pathFollower.SetDestination(m_alertTarget);
        Vector2 dir = m_pathFollower.GetMoveDirection(m_alertedSpeed);

        if (dir == Vector2.zero) {
            StartCoroutine(LookAroundAndWander());
            return;
        }

        RotateTowards(dir);
        m_rb.linearVelocity = dir * m_alertedSpeed;
    }

    private void UpdateChase() {
        m_pathFollower.SetDestination(m_player.position);
        Vector2 dir = m_pathFollower.GetMoveDirection(m_chaseSpeed);

        if (dir == Vector2.zero) {
            // brak ścieżki — idź prosto jako fallback
            dir = ((Vector2) m_player.position - (Vector2) transform.position).normalized;
        }

        RotateTowards(dir);
        m_rb.linearVelocity = dir * m_chaseSpeed;
    }

    private void OnNoise(Vector2 position, float radius) {
        float distance = Vector2.Distance(transform.position, position);
        if (distance > radius)
            return;

        m_alertTarget = position;
        if (m_state != ZombieState.Chase)
            m_state = ZombieState.Alerted;
    }

    public void TakeHit(float damage, bool isHeadshot) {
        if (isHeadshot)
            m_headshotSoundFX.Play();

        m_currentHp -= damage;
        if (m_currentHp <= 0) {
            Destroy(gameObject);
            GameEvents.Instance.ZombieDies();
        }
    }

    private bool CanSeePlayer() {
        if (m_player == null)
            return false;

        Vector2 toPlayer = (Vector2) m_player.position - (Vector2) transform.position;
        float distance = toPlayer.magnitude;

        if (m_state == ZombieState.Chase) {
            if (distance > m_loseRange)
                return false;
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position, toPlayer.normalized, distance, m_obstacleLayer);
            return hit.collider == null;
        } else {
            float currentRange = m_state == ZombieState.Alerted
                ? m_detectionRange * m_alertedDetectionMultiplier
                : m_detectionRange;

            if (distance > currentRange)
                return false;

            float angleToPlayer = Vector2.Angle(transform.right, toPlayer);
            if (angleToPlayer > m_fieldOfView / 2f)
                return false;

            RaycastHit2D hit = Physics2D.Raycast(
                transform.position, toPlayer.normalized, distance, m_obstacleLayer);
            return hit.collider == null;
        }
    }

    private IEnumerator MoanRoutine() {
        while (true) {
            yield return new WaitForSeconds(m_moanInterval);
            if (Random.value < m_moanChance && !m_moanSoundsFX.IsPlaying)
                m_moanSoundsFX.Play();
        }
    }

    private IEnumerator PauseAndWander() {
        m_isPaused = true;
        m_rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(Random.Range(m_pauseTimeRange.x, m_pauseTimeRange.y));
        m_wanderTarget = GetRandomWanderPoint();
        m_isPaused = false;
    }

    private IEnumerator LookAroundAndWander() {
        m_isPaused = true;
        m_rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(m_alertedLookAroundTime);
        m_isPaused = false;
        m_state = ZombieState.Wander;
    }

    private Vector2 GetRandomWanderPoint() {
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        return (Vector2) transform.position + randomDir * Random.Range(2f, m_wanderRadius);
    }

    private void RotateTowards(Vector2 dir) {
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float currentAngle = m_rb.rotation;
        float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, m_rotationSpeed * Time.fixedDeltaTime);
        m_rb.MoveRotation(newAngle);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        Vector3 forward = transform.right;
        float halfFov = m_fieldOfView / 2f * Mathf.Deg2Rad;
        Vector3 leftDir = new Vector2(
            forward.x * Mathf.Cos(-halfFov) - forward.y * Mathf.Sin(-halfFov),
            forward.x * Mathf.Sin(-halfFov) + forward.y * Mathf.Cos(-halfFov));
        Vector3 rightDir = new Vector2(
            forward.x * Mathf.Cos(halfFov) - forward.y * Mathf.Sin(halfFov),
            forward.x * Mathf.Sin(halfFov) + forward.y * Mathf.Cos(halfFov));

        switch (m_state) {
            case ZombieState.Wander:
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, leftDir * m_detectionRange);
                Gizmos.DrawRay(transform.position, rightDir * m_detectionRange);
                Gizmos.DrawRay(transform.position, forward * m_detectionRange);
                break;

            case ZombieState.Alerted:
                Gizmos.color = new Color(1f, 0.5f, 0f);
                float alertedRange = m_detectionRange * m_alertedDetectionMultiplier;
                Gizmos.DrawRay(transform.position, leftDir * alertedRange);
                Gizmos.DrawRay(transform.position, rightDir * alertedRange);
                Gizmos.DrawRay(transform.position, forward * alertedRange);
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
                Gizmos.DrawLine(transform.position, m_alertTarget);
                Gizmos.DrawWireSphere(m_alertTarget, 0.3f);
                break;

            case ZombieState.Chase:
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, m_loseRange);
                if (m_player != null)
                    Gizmos.DrawLine(transform.position, m_player.position);
                break;
        }

#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.8f,
            m_state.ToString());
#endif
    }
}