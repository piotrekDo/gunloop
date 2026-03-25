using UnityEngine;

public class DoorController : MonoBehaviour {

    [SerializeField] int m_maxHitpoints;
    [SerializeField] private int m_currentHitpoints;

    private void Awake() {
        m_currentHitpoints = m_maxHitpoints;
    }

    public void TakeHit(int dmg) {
        m_currentHitpoints -= dmg;
        if (m_currentHitpoints <= 0) {
            Destroy(gameObject);
        }
    }
}
