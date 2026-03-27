using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour {
    [Header("Pathfinding")]
    [SerializeField] private LayerMask m_wallLayer;
    [SerializeField] private float m_waypointTolerance = 0.3f;
    [SerializeField] private float m_recalculateInterval = 0.5f;

    private Pathfinder m_pathfinder;
    private List<Vector2> m_path;
    private int m_currentWaypointIndex;
    private Vector2 m_lastDestination;
    private bool m_isRecalculating = false;
    private const float k_destinationChangeTolerance = 0.3f;

    public bool HasPath => m_path != null && m_currentWaypointIndex < m_path.Count;

    private void Awake() {
        m_pathfinder = new Pathfinder(m_wallLayer);
    }

    public void SetDestination(Vector2 target) {
        if (Vector2.Distance(target, m_lastDestination) < k_destinationChangeTolerance)
            return;

        m_lastDestination = target;

        if (!m_isRecalculating) {
            m_isRecalculating = true;
            StartCoroutine(RecalculatePath());
        }
        // jeœli ju¿ dzia³a — coroutine sam pobierze nowy m_lastDestination
    }

    private IEnumerator RecalculatePath() {
        while (true) {
            List<Vector2> newPath = m_pathfinder.FindPath(transform.position, m_lastDestination);
            if (newPath != null && newPath.Count > 0) {
                m_path = newPath;
                m_currentWaypointIndex = 0;
            }
            yield return new WaitForSeconds(m_recalculateInterval);
        }
    }

    public Vector2 GetMoveDirection() {
        if (!HasPath)
            return Vector2.zero;

        Vector2 current = transform.position;
        Vector2 waypoint = m_path[m_currentWaypointIndex];

        if (Vector2.Distance(current, waypoint) < m_waypointTolerance) {
            m_currentWaypointIndex++;
            if (!HasPath)
                return Vector2.zero;
            waypoint = m_path[m_currentWaypointIndex];
        }

        return (waypoint - current).normalized;
    }

    public void StopPath() {
        StopAllCoroutines();
        m_isRecalculating = false;
        m_path = null;
    }

    private void OnDrawGizmos() {
        if (m_path == null)
            return;
        Gizmos.color = Color.cyan;
        for (int i = m_currentWaypointIndex; i < m_path.Count - 1; i++)
            Gizmos.DrawLine(m_path[i], m_path[i + 1]);

        if (HasPath) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(m_path[m_currentWaypointIndex], 0.1f);
        }
    }
}