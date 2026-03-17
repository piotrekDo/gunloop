using UnityEngine;

public class BulletController : MonoBehaviour {
    [SerializeField] private float m_maximumLifetime;

    private float m_currentLifetime;
    private void Update() {

        m_currentLifetime += Time.deltaTime;

        if (m_currentLifetime > m_maximumLifetime) {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Destroy(gameObject);
    }

}
