using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour {

    [Header("Gun Info")]
    [SerializeField] private string m_gunName;
    [SerializeField] private string m_gunType;
    [SerializeField] private int m_rpm;
    [SerializeField] private float m_bulletSpeed;
    [SerializeField] private int m_clipSize;
    [SerializeField] private int m_currentAmmo;
    [SerializeField] private float m_clipReloadSpeed;

    [Header("Spread")]
    [SerializeField] private float m_spreadPerShot;   // rozrzut dodawany przy ka¿dym strzale (stopnie)
    [SerializeField] private float m_maxSpread;       // maksymalny rozrzut (stopnie)
    [SerializeField] private float m_spreadDecayRate; // tempo malenia rozrzutu na sekundê (stopnie/s)

    [Header("Sound Effects")]
    [SerializeField] private SoundEffectHandler m_fireSoundFX;

    [Header("Bullet")]
    [SerializeField] private GameObject m_bulletPrefab;

    private float m_fireTimer = 0f;
    private float m_fireDelay;

    public string GunName => m_gunName;
    public string GunType => m_gunType;
    public int RPM => m_rpm;
    public float BulletSpeed => m_bulletSpeed;
    public int ClipSize => m_clipSize;
    public int CurrentAmmo => m_currentAmmo;
    public float ClipReloadSpeed => m_clipReloadSpeed;
    public float FireDelay => 60f / m_rpm;
    public float SpreadPerShot => m_spreadPerShot;
    public float MaxSpread => m_maxSpread;
    public float SpreadDecayRate => m_spreadDecayRate;

    private void Update() {
        if (m_fireTimer > 0f)
            m_fireTimer -= Time.deltaTime;
    }

    private void Awake() {
        m_fireDelay = 60f / m_rpm;
        m_currentAmmo = m_clipSize + 1;
    }

    public bool FireShoot(float spread) {
        if (m_fireTimer > 0f) return false;
        return TrySpawnBullet(spread);
    }

    private bool TrySpawnBullet(float spread) {
        Vector2 spawnPos = transform.position + transform.right * 1.5f + transform.up * -0.4f;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        Vector2 baseDir = (mouseWorld - spawnPos).normalized;

        float spreadAngle = Random.Range(-spread, spread);
        Vector2 direction = RotateVector(baseDir, spreadAngle);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject bullet = Instantiate(m_bulletPrefab);
        bullet.transform.position = spawnPos;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
            bulletRb.linearVelocity = direction * m_bulletSpeed;

        m_currentAmmo -= 1;
        m_fireSoundFX.Play();
        m_fireTimer = m_fireDelay;
        return true;
    }

    private Vector2 RotateVector(Vector2 v, float degrees) {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }
}