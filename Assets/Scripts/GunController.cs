using UnityEngine;

public class GunController : MonoBehaviour {

    [Header("Gun Info")]
    [SerializeField] private string m_gunName;
    [SerializeField] private string m_gunType;
    [SerializeField] private int m_rpm;
    [SerializeField] private float m_bulletSpeed;
    [SerializeField] private int m_clipSize;
    [SerializeField] private float m_clipReloadSpeed;

    [Header("Spread")]
    [SerializeField] private float m_spreadPerShot;   // rozrzut dodawany przy ka¿dym strzale (stopnie)
    [SerializeField] private float m_maxSpread;       // maksymalny rozrzut (stopnie)
    [SerializeField] private float m_spreadDecayRate; // tempo malenia rozrzutu na sekundê (stopnie/s)

    public string GunName => m_gunName;
    public string GunType => m_gunType;
    public int RPM => m_rpm;
    public float BulletSpeed => m_bulletSpeed;
    public int ClipSize => m_clipSize;
    public float ClipReloadSpeed => m_clipReloadSpeed;
    public float FireDelay => 60f / m_rpm;

    public float SpreadPerShot => m_spreadPerShot;
    public float MaxSpread => m_maxSpread;
    public float SpreadDecayRate => m_spreadDecayRate;
}