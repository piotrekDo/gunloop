using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour {
    [SerializeField] private Transform m_target;
    [SerializeField] private float m_zoomSpeed = 1f;
    [SerializeField] private float m_minSize = 3f;
    [SerializeField] private float m_maxSize = 10f;

    private Camera m_camera;
    private void Awake() {
        m_camera = GetComponent<Camera>();
    }

    private void Update() {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0f) {
            m_camera.orthographicSize -= scroll * m_zoomSpeed;
            m_camera.orthographicSize = Mathf.Clamp(m_camera.orthographicSize, m_minSize, m_maxSize);
        }
    }

    private void LateUpdate() {
        transform.position = new Vector3(m_target.position.x, m_target.position.y, transform.position.z);
    }

    private void OnZoom(InputValue value) {
        float scroll = value.Get<Vector2>().y;
        m_camera.orthographicSize -= scroll * m_zoomSpeed * Time.deltaTime;
        m_camera.orthographicSize = Mathf.Clamp(m_camera.orthographicSize, m_minSize, m_maxSize);
    }
}