using System.Drawing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private float m_baseSize = 10f;      // rozmiar przy rozrzucie = 0
    [SerializeField] private float m_spreadMultiplier = 8f; // jak bardzo rozrzut powiêksza kó³ko

    private RectTransform m_rectTransform;

    private void Awake() {
        m_rectTransform = GetComponent<RectTransform>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        m_rectTransform.sizeDelta = new Vector2(m_baseSize, m_baseSize);
    }

    private void OnDisable() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update() {
        // Pod¹¿aj za mysz¹
        Vector2 mousePos = Mouse.current.position.ReadValue();
        m_rectTransform.position = mousePos;
    }

    public void SetSpread(float spreadDegrees) {
        float size = m_baseSize + spreadDegrees * m_spreadMultiplier;
        m_rectTransform.sizeDelta = new Vector2(size, size);
    }
}