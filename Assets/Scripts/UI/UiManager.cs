using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [Header("Ui Panels")]
    [SerializeField] GameObject MenuPanel;
    [SerializeField] GameObject PausePanel;
    [SerializeField] GameObject OverlayPanel;

    [Header("PlayersHP")]
    [SerializeField] GameObject[] playersHP;
    [SerializeField] Image[] playersHpBar;
    [SerializeField] TextMeshProUGUI[] playersHpText;

    [SerializeField] float hpBarTransitionTime = 5f;


    [SerializeField] TextMeshProUGUI fps;


    private int playersCount;
    void Start()
    {
        SwitchPanelTo(MenuPanel);
        SubscribeToEvents();

        // Add the second player logic as well
        playersHP[0].SetActive(false);
        playersHP[1].SetActive(false);
        //secondPlayerHP.SetActive(false);
    }

    private void Update()
    {
        int currentFPS = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
        fps.text = currentFPS.ToString();

        if (GameManager.Instance.CurrentGameState == GameStates.Playing && GameManager.Instance.CurrentLevelState == LevelStates.Lobby)
        {
            playersCount = PlayerManager.Instance.GetPlayers().Count;
            switch (playersCount)
            {
                case 1:
                    playersHP[0].SetActive(true);
                    playersHpBar[0].color = PlayerManager.Instance.GetPlayers()[0].GetPlayerColor() * 0.6f;
                    playersHP[1].SetActive(false);

                    break;
                case 2:
                    //playersHP[0].SetActive(true);
                    //playersHpBar[0].color = PlayerManager.Instance.GetPlayers()[0].GetPlayerColor() * 0.6f;
                    playersHP[1].SetActive(true);
                    playersHpBar[1].color = PlayerManager.Instance.GetPlayers()[1].GetPlayerColor() * 0.6f;
                    break;
                default:
                    playersHP[0].SetActive(false);
                    playersHP[1].SetActive(false);
                    break;
            }
        }

        //if (playersCount > 0)
            UpdatePlayersHP();
        
    }

    void UpdatePlayersHP()
    {
        for (int i = 0; i < playersCount; i++)
        {
            float currentHP = PlayerManager.Instance.GetPlayers()[i].GetStats().core.currentHP;
            float maxHP = PlayerManager.Instance.GetPlayers()[i].GetStats().core.maxHP;
            float targetFill = currentHP / maxHP;

            playersHpBar[i].fillAmount = Mathf.Lerp(playersHpBar[i].fillAmount, targetFill, hpBarTransitionTime * Time.deltaTime);
            
            playersHpText[i].text = currentHP + "/" + maxHP;
        }

        
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
