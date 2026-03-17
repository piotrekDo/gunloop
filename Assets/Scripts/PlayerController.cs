using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {
    [Header("Move")]
    [SerializeField] private float m_moveSpeed = 5f;
    [SerializeField] private float m_strafeSpeed = 4f;
    [SerializeField] private float m_runSpeed = 2f;

    [Header("Firing")]
    [SerializeField] private GunController m_gun;
    [SerializeField] private GameObject m_bulletPrefab;

    [Header("Sound Effects")]
    [SerializeField] private SoundEffectHandler m_fireSoundFX;

    [Header("UI")]
    [SerializeField] private Crosshair m_crosshair;

    [Header("Other")]
    [SerializeField] private Animator m_animator;

    private Rigidbody2D m_rb;
    private bool m_isRunning;
    private bool m_isFiring;
    private float m_fireTimer = 0f;
    private float m_fireDelay = 0f;
    private float m_currentSpread = 0f;

    private void Awake() {
        m_rb = GetComponent<Rigidbody2D>();
        m_rb.gravityScale = 0f;
        m_rb.freezeRotation = false;
    }

    private void OnEnable() {
        ApplyGun(m_gun);
    }

    private void Update() {
        if (m_fireTimer > 0f)
            m_fireTimer -= Time.deltaTime;

        if (m_gun != null && m_currentSpread > 0f)
            m_currentSpread = Mathf.Max(0f, m_currentSpread - m_gun.SpreadDecayRate * Time.deltaTime);
        Debug.Log(m_currentSpread);
        m_crosshair.SetSpread(m_currentSpread);

        if (m_isFiring && m_fireTimer <= 0f)
            TrySpawnBullet();
    }

    private void FixedUpdate() {
        ReadInput();
        RotateToMouse();
    }

    public void EquipGun(GunController gun) {
        if (gun == null)
            return;
        m_gun = gun;
        ApplyGun(gun);
    }

    private void ApplyGun(GunController gun) {
        if (gun == null)
            return;
        m_fireDelay = gun.FireDelay;
        m_fireTimer = 0f;
        m_currentSpread = 0f;
    }

    private void ReadInput() {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        float vertical = 0f;
        float horizontal = 0f;

        if (keyboard.wKey.isPressed)
            vertical += 1f;
        if (keyboard.sKey.isPressed)
            vertical -= 1f;
        if (keyboard.aKey.isPressed)
            horizontal += 1f;
        if (keyboard.dKey.isPressed)
            horizontal -= 1f;

        m_isRunning = keyboard.leftShiftKey.isPressed;

        Vector2 input = new Vector2(horizontal, vertical);
        if (input.sqrMagnitude > 1f)
            input.Normalize();

        Vector2 forward = transform.right;
        Vector2 strafe = transform.up;
        Vector2 moveDir = forward * input.y + strafe * input.x;

        float speed;
        if (input.x != 0 || input.y < 0)
            speed = m_strafeSpeed;
        else
            speed = m_moveSpeed;

        if (m_isRunning)
            speed = m_runSpeed;

        m_rb.linearVelocity = moveDir * speed;
    }

    private void OnFire(InputValue value) {
        m_isFiring = value.Get<float>() > 0f;
    }

    private void OnMove(InputValue value) {
        Vector2 move = value.Get<Vector2>();
        bool isMoving = move.sqrMagnitude > 0.01f;
        m_animator.SetBool("isMoving", isMoving);
    }

    private void RotateToMouse() {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);

        Vector2 direction = mouseWorld - (Vector2) transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void TrySpawnBullet() {
        if (m_gun == null) {
            Debug.LogWarning("PlayerController: brak GunController!");
            return;
        }

        Vector2 spawnPos = transform.position + transform.right * 1.5f + transform.up * -0.4f;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        Vector2 baseDir = (mouseWorld - spawnPos).normalized;

        float spreadAngle = Random.Range(-m_currentSpread, m_currentSpread);
        Vector2 direction = RotateVector(baseDir, spreadAngle);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject bullet = Instantiate(m_bulletPrefab);
        bullet.transform.position = spawnPos;
        bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
            bulletRb.linearVelocity = direction * m_gun.BulletSpeed;

        m_currentSpread = Mathf.Min(m_currentSpread + m_gun.SpreadPerShot, m_gun.MaxSpread);

        m_fireSoundFX.Play();
        m_fireTimer = m_fireDelay;
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