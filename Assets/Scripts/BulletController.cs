using UnityEngine;

public class BulletController : MonoBehaviour {
    [SerializeField] private float m_maximumLifetime;
    [SerializeField] private GameObject m_bulltetHitFX;

    private float m_currentLifetime;
    private void Update() {

        m_currentLifetime += Time.deltaTime;

        if (m_currentLifetime > m_maximumLifetime) {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.TryGetComponent(out EnemyZombieController zombie)) {
            SpawnVfxHit();
        }
        Destroy(gameObject);
    }

    private void SpawnVfxHit() {
        GameObject hitEffect = GameObject.Instantiate(m_bulltetHitFX);
        hitEffect.transform.position = transform.position;
    }

}
