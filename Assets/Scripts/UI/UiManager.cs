using UnityEngine;

public class UiManager : MonoBehaviour
{
    [Header("Ui Panels")]
    [SerializeField] GameObject MenuPanel;
    [SerializeField] GameObject PausePanel;
    [SerializeField] GameObject OverlayPanel;


    void Start()
    {
        SwitchPanelTo(MenuPanel);
        SubscribeToEvents();
    }


    void SwitchPanelTo(GameObject panel)
    {
        MenuPanel.SetActive(false);
        PausePanel.SetActive(false);
        OverlayPanel.SetActive(false);

        panel.SetActive(true);
    }

    public void StartButton()
    {
        SwitchPanelTo(OverlayPanel);

        GameManager.Instance.SetGameState(GameStates.Playing);
    }

    void GameStateChanged(GameStates gameState)
    {
        switch (gameState)
        {
            case GameStates.Menu:
                SwitchPanelTo(MenuPanel);
                break;
            case GameStates.Playing:
                SwitchPanelTo(OverlayPanel);
                break;
            case GameStates.Pause:
                SwitchPanelTo(PausePanel);
                break;
            default:
                SwitchPanelTo(MenuPanel);
                break;
        }
    }

    void LevelStateChanged(LevelStates levelState)
    {

    }

    private void SubscribeToEvents()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null.", this);
            return;
        }

        GameManager.Instance.OnGameStateChanged += GameStateChanged;
        GameManager.Instance.OnLevelStateChanged += LevelStateChanged;

    }


    private void OnDisable() 
    {
        GameManager.Instance.OnGameStateChanged -= GameStateChanged;
        GameManager.Instance.OnLevelStateChanged -= LevelStateChanged;
    }

}
