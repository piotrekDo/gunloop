using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class HudController : MonoBehaviour {
    
    private UIDocument m_uiDocument;

    [SerializeField] private ScoreManagerController scoreManager;

    private VisualElement m_ammoDisplay;

    private void OnEnable() {
        m_uiDocument = gameObject.GetComponent<UIDocument>();
        VisualElement root = m_uiDocument.rootVisualElement;
        VisualElement mainContainer = root.Q<VisualElement>("MainContainer");
        mainContainer.dataSource = scoreManager;

    }
}
