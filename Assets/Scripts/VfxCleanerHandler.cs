using UnityEngine;

public class VfxCleanerHandler : MonoBehaviour {

    private float m_currentTime;
    [SerializeField] float m_lifeTime;

    private void Update() {
        if (m_currentTime > m_lifeTime) {
            GameObject.Destroy(gameObject);
        }

        m_currentTime += Time.deltaTime;
    }
}
