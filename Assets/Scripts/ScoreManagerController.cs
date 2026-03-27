using UnityEngine;

public class ScoreManagerController : MonoBehaviour {

    [SerializeField] private GunController m_gunPrimary;
    [SerializeField] private GameManagerController m_gameManager;
    private void OnEnable() {
        GameEvents.Instance.onGunpickup += OnGunPickup;
    }
    private void OnDisable() {
        GameEvents.Instance.onGunpickup -= OnGunPickup;
    }

    private void OnGunPickup(GunController gun) {
        m_gunPrimary = gun;

    }
}