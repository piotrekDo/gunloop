using UnityEngine;

public class BulletController : MonoBehaviour {
    [SerializeField] private float m_maximumLifetime;
    [SerializeField] private float m_hideDistance = .5f;
    [SerializeField] private float m_fadeInDuration = 0.1f;
    [SerializeField] private GameObject m_bulletHitFX;

    private float m_currentLifetime;
    private SpriteRenderer m_spriteRenderer;


    // headshot
    private Vector2 m_aimPosition;
    private float m_headshotRadius;
    private float m_headshotMultiplier;
    private float m_headshotAimRadius;
    private float m_headshotHitRadius;

    public void Init(Vector2 aimPosition, float headshotAimRadius, float headshotHitRadius, float headshotMultiplier) {
        m_aimPosition = aimPosition;
        m_headshotAimRadius = headshotAimRadius;
        m_headshotHitRadius = headshotHitRadius;
        m_headshotMultiplier = headshotMultiplier;
    }

    private void Awake() {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        if (m_spriteRenderer != null)
            m_spriteRenderer.color = Color.clear;
    }

    private void Update() {
        m_currentLifetime += Time.deltaTime;

        if (m_spriteRenderer != null && m_currentLifetime >= m_hideDistance) {
            float alpha = Mathf.Clamp01((m_currentLifetime - m_hideDistance) / m_fadeInDuration);
            m_spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
        }

        if (m_currentLifetime > m_maximumLifetime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.TryGetComponent(out EnemyZombieController zombie)) {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            Vector2 bulletDir = rb.linearVelocity.normalized;

            // jak blisko centrum wroga przechodzi linia lotu pocisku
            Vector2 toEnemy = (Vector2) collision.transform.position - (Vector2) transform.position;
            float distanceAlongDir = Vector2.Dot(toEnemy, bulletDir);
            Vector2 closestPoint = (Vector2) transform.position + bulletDir * distanceAlongDir;
            float missDistance = Vector2.Distance(closestPoint, collision.transform.position);

            float aimDistance = Vector2.Distance(m_aimPosition, collision.transform.position);
            bool isHeadshot = aimDistance < m_headshotAimRadius && missDistance < m_headshotHitRadius;

            float damage = isHeadshot ? 1f * m_headshotMultiplier : 1f;
            zombie.TakeHit(damage, isHeadshot);
            SpawnVfxHit();
        } else if (collision.TryGetComponent(out DoorController door)) {
            door.TakeHit(1);
            SpawnVfxHit();
        } else if (collision.gameObject.layer == LayerMask.NameToLayer("Wall")) {
            SpawnVfxHit();
        }
        if (!collision.gameObject.CompareTag("Player")) {
            Destroy(gameObject);
        }   
    }

    private void SpawnVfxHit() {
        if (m_bulletHitFX == null)
            return;
        GameObject hitEffect = Instantiate(m_bulletHitFX);
        hitEffect.transform.position = transform.position;
    }
}