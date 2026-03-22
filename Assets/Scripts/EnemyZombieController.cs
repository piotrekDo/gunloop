using UnityEngine;

public class EnemyZombieController : MonoBehaviour {

    [Header("HP")]
    [SerializeField] private float m_maximumHp;
    [SerializeField] private float m_currentHp;

    [Header("Sound Effects")]
    [SerializeField] private SoundEffectHandler m_headshotSoundFX;

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f); // ta sama wartoœæ co headshotRadius
    }
    private void Awake() {
        m_currentHp = m_maximumHp;
    }

    public void TakeHit(float damage, bool isHeadshot) {
        if (isHeadshot) {
            m_headshotSoundFX.Play();
        }
        m_currentHp -= damage;
        //if (m_currentHp <= 0)
            //Destroy(gameObject);
    }
}
