using System.Collections;
using UnityEngine;

public class EnemyZombieController : MonoBehaviour {

    [Header("HP")]
    [SerializeField] private float m_maximumHp;
    [SerializeField] private float m_currentHp;

    [Header("Sound Effects")]
    [SerializeField] private SoundEffectHandler m_headshotSoundFX;
    [SerializeField] private SoundEffectHandler m_moanSoundsFX;
    [SerializeField] private float m_moanInterval = 2f;  // co ile sekund losuje
    [SerializeField] private float m_moanChance = 0.5f;  // 50% szans

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.3f); // ta sama wartoœæ co headshotRadius
    }
    private void Awake() {
        m_currentHp = m_maximumHp;
        StartCoroutine(MoanRoutine());
    }

    public void TakeHit(float damage, bool isHeadshot) {
        if (isHeadshot) {
            m_headshotSoundFX.Play();
        }
        m_currentHp -= damage;
        //if (m_currentHp <= 0)
            //Destroy(gameObject);
    }

    private IEnumerator MoanRoutine() {
        while (true) {
            yield return new WaitForSeconds(m_moanInterval);
            if (Random.value < m_moanChance && !m_moanSoundsFX.IsPlaying)
                m_moanSoundsFX.Play();
        }
    }
}
