using UnityEngine;

public class EnemyZombieController : MonoBehaviour {
    [SerializeField] private float m_maximumHp;

    private float m_currentHp;

    private void Awake() {
        m_currentHp = m_maximumHp;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        m_currentHp -= 1;

        if (m_currentHp <= 0)
            Destroy(gameObject);
    }
}
