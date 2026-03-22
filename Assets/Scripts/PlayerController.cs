using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {
    [Header("Move")]
    [SerializeField] private float m_moveSpeed = 5f;
    [SerializeField] private float m_runSpeed = 2f;
    [SerializeField] private float m_moveSpeedSpreadFactor = .5f;

    [Header("Firing")]
    [SerializeField] private GunController m_gun;

    [Header("UI")]
    [SerializeField] private Crosshair m_crosshair;

    [Header("Other")]
    [SerializeField] private Animator m_animator;
    [SerializeField] private float k_reloadClipLength = 2f;

    private Rigidbody2D m_rb;
    private bool m_isRunning;
    private bool m_isFiring;
    private float m_currentSpread = 0f;

    private void Awake() {
        m_rb = GetComponent<Rigidbody2D>();
        m_rb.gravityScale = 0f;
        m_rb.freezeRotation = false;
    }

    private void OnDrawGizmos() {
    if (!Application.isPlaying) return;
    
    // Czerwona kropka = aktualny punkt spawnu pocisku
    Vector2 muzzlePos = (Vector2)transform.position 
        + (Vector2)transform.right * 1.5f 
        + (Vector2)transform.up * -0.4f;
    
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(muzzlePos, 0.1f);
    
    // ¯ó³ta kropka = œrodek postaci
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, 0.1f);
}

    private void OnEnable() {
        ApplyGun(m_gun);
    }

    private void OnDisable() {
    }


    private void Update() {
        if (m_gun != null) {
            m_currentSpread = Mathf.Max(m_gun.MinSpread, m_currentSpread - m_gun.SpreadDecayRate * Time.deltaTime);

            float moveSpreadFloor = m_isRunning
                ? m_gun.MaxSpread
                : m_gun.MinSpread + m_rb.linearVelocity.magnitude * m_moveSpeedSpreadFactor;
            m_currentSpread = Mathf.Max(m_currentSpread, moveSpreadFloor);

            m_currentSpread = Mathf.Clamp(m_currentSpread, m_gun.MinSpread, m_gun.MaxSpread);

            m_crosshair.SetSpread(m_currentSpread);

        if (m_isFiring) {
            // Oblicz kierunek od ŒRODKA gracza do myszy
            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
            Vector2 shootDir = (mouseWorld - (Vector2)transform.position).normalized;

            bool shotFired = m_gun.FireShoot(m_currentSpread, transform.position, shootDir, mouseWorld);
            if (shotFired)
                m_currentSpread = Mathf.Min(m_currentSpread + m_gun.SpreadPerShot, m_gun.MaxSpread);
        }
        }
    }

    private void FixedUpdate() {
        ReadInput();
        RotateToMouse();
    }

    public void EquipGun(GunController gun) {
        if (gun == null)
            return;
        ApplyGun(gun);
    }

    private void ApplyGun(GunController gun) {
        if (gun == null)
            return;
        m_gun = gun;
        m_currentSpread = m_gun.MinSpread;
        GameEvents.Instance.PickupGun(gun);
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
            horizontal -= 1f;
        if (keyboard.dKey.isPressed)
            horizontal += 1f;

        m_isRunning = keyboard.leftShiftKey.isPressed;

        Vector2 moveDir = new Vector2(horizontal, vertical);
        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        float speed = m_isRunning ? m_runSpeed : m_moveSpeed;

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

    private void OnReload() {
        float speed = k_reloadClipLength / m_gun.ClipReloadSpeed;
        m_animator.SetFloat("reloadTime", speed);
        m_animator.SetBool("isReloading", true);

        m_currentSpread = Mathf.Min(m_currentSpread + (m_gun.MaxSpread / 2), m_gun.MaxSpread);

        m_gun.ReloadGun();
    }

    private void OnReloadComplete() {
        m_animator.SetBool("isReloading", false);
    }

    private void RotateToMouse() {
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);

        Vector2 direction = mouseWorld - (Vector2) transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

}