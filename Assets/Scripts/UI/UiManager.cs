using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

    [Header("Screen Fade")]
    [SerializeField] Image screenFade;
    [SerializeField] float screenFadeTime = 0.4f;
    [SerializeField] float screenFadeInTime = 0.1f;
    [SerializeField] float screenFadeOutTime = 0.2f;
    [SerializeField] TextMeshProUGUI fps;

    [Header("Coins")]
    [SerializeField] TextMeshProUGUI coinsText;


    private int playersCount;
    void Start()
    {
        SwitchPanelTo(MenuPanel);
        SubscribeToEvents();

        Color color = screenFade.color;
        color.a = 0;

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
            var players = PlayerManager.Instance.GetPlayers();

            playersCount = players.Count;

            playersHP[0].SetActive(false);
            playersHP[1].SetActive(false);

            foreach (PlayerController player in players)
            {
                if (player == null)
                    continue;

                PlayerInput playerInput = player.GetComponent<PlayerInput>();

                if (playerInput == null)
                    continue;

                int uiIndex = playerInput.playerIndex;

                if (uiIndex < 0 || uiIndex >= playersHP.Length)
                    continue;

                playersHP[uiIndex].SetActive(true);

                playersHpBar[uiIndex].color =
                    player.GetColor() * 0.6f;
            }
        }

        UpdatePlayersHP();

        coinsText.text = GameManager.Instance.GetCoins().ToString();
    }

    //void UpdatePlayersHP()
    //{
    //    for (int i = 0; i < playersCount; i++)
    //    {
    //        float currentHP = PlayerManager.Instance.GetPlayers()[i].GetStats().core.currentHP;
    //        float maxHP = PlayerManager.Instance.GetPlayers()[i].GetStats().core.maxHP;
    //        float targetFill = currentHP / maxHP;

    //        playersHpBar[i].fillAmount = Mathf.Lerp(playersHpBar[i].fillAmount, targetFill, hpBarTransitionTime * Time.deltaTime);
            
    //        playersHpText[i].text = currentHP + "/" + maxHP;
    //    }
    //}

    void UpdatePlayersHP()
    {
        var players = PlayerManager.Instance.GetPlayers();

        foreach (PlayerController player in players)
        {
            if (player == null)
                continue;

            PlayerInput playerInput = player.GetComponent<PlayerInput>();

            if (playerInput == null)
                continue;

            int uiIndex = playerInput.playerIndex;
            if (uiIndex < 0 || uiIndex >= playersHpBar.Length)
                continue;

            float currentHP = player.GetStats().core.currentHP;
            float maxHP = player.GetStats().core.maxHP;
            float targetFill = currentHP / maxHP;

            playersHpBar[uiIndex].fillAmount = Mathf.Lerp(playersHpBar[uiIndex].fillAmount, targetFill, hpBarTransitionTime * Time.deltaTime);
            playersHpText[uiIndex].text = $"{currentHP}/{maxHP}";
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

    void ScreenFade()
    {

        Sequence fadeSequence = DOTween.Sequence();

        fadeSequence
            .Append(screenFade.DOFade(1f, screenFadeInTime))
            .AppendInterval(screenFadeTime)
            .Append(screenFade.DOFade(0f, screenFadeOutTime));
    }
     
    void LevelStateChanged(LevelStates levelState)
    {
        ScreenFade();
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
