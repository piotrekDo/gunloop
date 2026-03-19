using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Crosshair : MonoBehaviour {
    [Header("Settings")]
    [SerializeField] private float m_baseSize = 40f;
    [SerializeField] private float m_spreadMultiplier = 8f;
    [SerializeField] private float m_ringThickness = 2f;  
    [SerializeField] private Color m_color = Color.white;

    private RectTransform m_rectTransform;
    private Image m_image;

    private void Awake() {
        m_rectTransform = GetComponent<RectTransform>();
        m_image = GetComponent<Image>();

        m_image.sprite = CreateRingSprite(64, m_ringThickness, m_color);
        m_image.color = m_color;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        m_rectTransform.sizeDelta = new Vector2(m_baseSize, m_baseSize);
    }

    private void OnDisable() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update() {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        m_rectTransform.position = mousePos;
    }

    public void SetSpread(float spreadDegrees) {
        float size = m_baseSize + spreadDegrees * m_spreadMultiplier;
        m_rectTransform.sizeDelta = new Vector2(size, size);
    }

    private static Sprite CreateRingSprite(int texSize, float thickness, Color col) {
        var tex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float center = texSize / 2f;
        float outerR = center - 1f;
        float innerR = outerR - thickness;

        Color[] pixels = new Color[texSize * texSize];

        for (int y = 0; y < texSize; y++) {
            for (int x = 0; x < texSize; x++) {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));

                // antyaliasing na krawêdziach
                float outerAlpha = Mathf.Clamp01(outerR - dist + 1f);
                float innerAlpha = Mathf.Clamp01(dist - innerR + 1f);
                float alpha = Mathf.Min(outerAlpha, innerAlpha);

                pixels[y * texSize + x] = new Color(col.r, col.g, col.b, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex,
            new Rect(0, 0, texSize, texSize),
            new Vector2(0.5f, 0.5f));
    }
}