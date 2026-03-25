using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviour {
    [Header("Gun Info")]
    [SerializeField] private string m_gunName;
    [SerializeField] private string m_gunType;
    [SerializeField] private int m_rpm;
    [SerializeField] private float m_bulletSpeed;
    [SerializeField] private int m_clipSize;
    [SerializeField] private int m_currentClipAmmo;
    [SerializeField] private int m_ammoLeft;
    [SerializeField] private float m_clipReloadSpeed;

    [Header("Accuracy")]
    [SerializeField] private float m_maxSpread;
    [SerializeField] private float m_minSpread;
    [SerializeField] private float m_spreadPerShot;
    [SerializeField] private float m_spreadDecayRate;

    [Header("Noise")]
    [SerializeField] private float m_noiseRadius = 15f;

    [Header("Headshot")]
    [SerializeField] private float m_headshotAimRadius = 0.3f;   // precyzja celowania
    [SerializeField] private float m_headshotHitRadius = 0.55f;  // trajektoria przez centrum
    [SerializeField] private float m_headshotMultiplier = 2f;

    [Header("Sound Effects")]
    [SerializeField] private SoundEffectHandler m_fireSoundFX;
    [SerializeField] private SoundEffectHandler m_gunEmptyFX;
    [SerializeField] private SoundEffectHandler m_gunReloadFX;
    [SerializeField] private SoundEffectHandler m_gunBoltFX;

    [Header("Bullet")]
    [SerializeField] private GameObject m_bulletPrefab;

    private float m_fireTimer = 0f;
    private float m_fireDelay;
    private bool m_isGunReloading = false;

    public string GunName => m_gunName;
    public string GunType => m_gunType;
    public int RPM => m_rpm;
    public float BulletSpeed => m_bulletSpeed;
    public int ClipSize => m_clipSize;
    public int CurrentClipAmmo => m_currentClipAmmo;
    public float ClipReloadSpeed => m_clipReloadSpeed;
    public float FireDelay => 60f / m_rpm;
    public float NoiseRadius => m_noiseRadius;
    public float SpreadPerShot => m_spreadPerShot;
    public float MinSpread => m_minSpread;
    public float MaxSpread => m_maxSpread;
    public float SpreadDecayRate => m_spreadDecayRate;

    private void Awake() {
        m_fireDelay = 60f / m_rpm;
        m_currentClipAmmo = m_clipSize + 1;
    }

    private void Update() {
        if (m_fireTimer > 0f)
            m_fireTimer -= Time.deltaTime;
    }

    public bool FireShoot(float spread, Vector2 originPos, Vector2 shootDir, Vector2 aimPosition) {
        if (m_fireTimer > 0f)
            return false;

        if (m_isGunReloading) {
            if (m_currentClipAmmo > 0)
                return TrySpawnBullet(spread, originPos, shootDir, aimPosition);
            return false;
        }

        if (m_currentClipAmmo <= 0) {
            m_gunEmptyFX.Play();
            m_fireTimer = 1f;
            return false;
        }

        return TrySpawnBullet(spread, originPos, shootDir, aimPosition);
    }

    public void ReloadGun() {
        if (m_isGunReloading || m_ammoLeft == 0)
            return;
        m_currentClipAmmo = m_currentClipAmmo > 0 ? 1 : 0;
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine() {
        m_isGunReloading = true;
        m_gunReloadFX.Play();
        yield return new WaitForSeconds(m_clipReloadSpeed);
        if (m_currentClipAmmo == 0) {
            m_gunBoltFX.Play();
            yield return new WaitForSeconds(1f);
        }
        int reloadValue = Mathf.Min(m_clipSize, m_ammoLeft);
        m_currentClipAmmo = m_currentClipAmmo + reloadValue;
        m_ammoLeft -= reloadValue;
        m_isGunReloading = false;
        m_fireTimer = 0f;
    }

    private bool TrySpawnBullet(float spread, Vector2 originPos, Vector2 shootDir, Vector2 aimPosition) {
        Vector2 spawnPos = originPos + shootDir * -.01f;
        float spreadAngle = Random.Range(-spread, spread);
        Vector2 direction = RotateVector(shootDir, spreadAngle);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject bullet = Instantiate(m_bulletPrefab);
        bullet.transform.position = spawnPos;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        BulletController bc = bullet.GetComponent<BulletController>();
        if (bc != null)
            bc.Init(aimPosition, m_headshotAimRadius, m_headshotHitRadius, m_headshotMultiplier);

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
            bulletRb.linearVelocity = direction * m_bulletSpeed;

        m_currentClipAmmo -= 1;
        m_fireSoundFX.Play();
        m_fireTimer = m_fireDelay;
        return true;
    }

    private Vector2 RotateVector(Vector2 v, float degrees) {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}