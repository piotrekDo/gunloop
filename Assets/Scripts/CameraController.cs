using UnityEngine;

public class CameraFollow : MonoBehaviour {
    [SerializeField] private Transform m_target;

    private void LateUpdate() {
        transform.position = new Vector3(m_target.position.x, m_target.position.y, transform.position.z);
    }
}