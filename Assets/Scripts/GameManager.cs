using System;
using UnityEngine;


public enum GameStates
{
    Menu,
    Playing,
    Pause
}
public enum LevelStates
{
    Lobby,
    Aisle,
    Room,
    Boss
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameStates CurrentGameState { get; private set; }
    public event Action<GameStates> OnGameStateChanged;

    [Header("Levels parameters")]
    public LevelStates CurrentLevelState { get; private set; }
    public event Action<LevelStates> OnLevelStateChanged;

    public event Action OnGameReset;
    public event Action OnGameOver;

    [SerializeField] LevelGenerator levelGenerator;

    [Header("Players Manager")]
    [SerializeField] PlayerManager playerManager;
    [SerializeField] float playersPositionOffset = 4;

    [Header("Camera")]
    // This is the camera position
    [SerializeField] Transform screenCenter;
    [SerializeField] float cameraSpeed = 1;
    [SerializeField] private bool cameraMoving = false;
    public bool CameraMoving => cameraMoving;


    [Header("Positions")]
    [SerializeField] Vector3 cameraStartPosition;

    [Header("Object Pools")]
    [SerializeField] private Coin coinPrefab;
    [SerializeField] private Coin coinsPrefab;

    //[SerializeField] private Diamond diamondPrefab;

    [Header("Coins")]
    int coins;

    private void Awake() 
    {
        Application.targetFrameRate = 120;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentGameState = GameStates.Menu;
        CurrentLevelState = LevelStates.Lobby;
        screenCenter.position = cameraStartPosition;


        // Initialise pools
        //ObjectPoolManager.Instance.CreatePool(diamondPrefab, 10);
    }

    private void Start()
    {
        levelGenerator.GenerateLevel();

        InitializePools();
    }

    private void Update()
    {
        if (cameraMoving)
        {
            screenCenter.position += Vector3.forward * cameraSpeed * Time.deltaTime;
        }
        Debug.Log("GameState: " + CurrentGameState);
    }

    void InitializePools()
    {
        ObjectPoolManager.Instance.CreatePool(coinPrefab, 10);
        //ObjectPoolManager.Instance.CreatePool(coinsPrefab, 10);
    }

    void ClearPools()
    {
        ObjectPoolManager.Instance.ClearPools();
    }

    public void SetGameState(GameStates newState)
    {
        if (CurrentGameState == newState)
            return;

        CurrentGameState = newState;
        OnGameStateChanged?.Invoke(CurrentGameState);
        CheckCameraMoving();
    }

    public void SetLevelState(LevelStates newState)
    {
        if (CurrentLevelState == newState)
            return;

        if (CurrentLevelState == LevelStates.Lobby)
        {
            levelGenerator.SetCurrentLevelIndex(0);
        }
        else
        {
            levelGenerator.SetCurrentLevelIndex(levelGenerator.CurrentRoomIndex + 1);
        }

        //if (newState == LevelStates.Lobby)
        //{
        //    levelGenerator.SetCurrentLevelIndex(0);
        //}

        CurrentLevelState = newState;

        OnLevelStateChanged?.Invoke(CurrentLevelState);
        TransitionToNextLevel();

        //if (CurrentLevelState == LevelStates.Aisle)
        //    //SpawnEnemies();

        CheckCameraMoving();
    }

    //void SpawnEnemies()
    //{
    //    // Have some bugs here. Out of range
    //    GameObject aisle = levelGenerator.GetLevel()[currentRoomIndex - 1];
    //    //aisle.
    //    Vector3 position = levelGenerator.GetScreenCenterOf(currentRoomIndex - 1);
    //}

    void CheckCameraMoving()
    {
        if (CurrentGameState == GameStates.Playing && CurrentLevelState == LevelStates.Aisle)
        {
            cameraMoving = true;
        }
        else
        {
            cameraMoving = false;
        }
        Debug.Log("Camera moving is " + cameraMoving);
    }

    //public void NextLevel()
    //{

    //    if (CurrentLevelState == LevelStates.Lobby)
    //    {
    //        CurrentLevelState = LevelStates.Aisle;
    //        CurrentGameState = GameStates.Playing;

    //        TransitionToNextLevel(roomTransitionDelta);
    //    }
    //    else if (CurrentLevelState == LevelStates.Room)
    //    {
    //        CurrentLevelState = LevelStates.Aisle;

    //        TransitionToNextLevel(roomTransitionDelta);
    //    }
    //    else if (CurrentLevelState == LevelStates.Aisle)
    //    {
    //        CurrentLevelState = LevelStates.Room;

    //        TransitionToNextLevel(aisleTransitionDelta);
    //    }

    //    OnGameStateChanged?.Invoke(CurrentGameState);
    //    OnLevelStateChanged?.Invoke(CurrentLevelState);
    //}

    public void SetCameraMoving(bool moving)
    {
        cameraMoving = moving;
        Debug.Log("Set Camera moving " + cameraMoving);
    }

    public void AddCoins(int amount)
    {
        coins += amount;
    }

    public int GetCoins()
    {
        return coins;
    }

    public void SetCoins(int value)
    {
        coins = value;
    }


    public void AllPlayersDead()
    {
        OnGameOver?.Invoke();
        cameraMoving = false;
    }


    public void ResetGame()
    {
        Debug.Log("Resetting Game");
        OnGameReset?.Invoke();
        cameraMoving = false;


        SetGameState(GameStates.Playing);
        SetLevelState(LevelStates.Lobby);
        screenCenter.position = cameraStartPosition;

        ClearPools();
        levelGenerator.ClearLevel();
        

        Debug.Log("Current Game State " + CurrentGameState);
        Debug.Log("Current Level State " + CurrentLevelState);

        levelGenerator.GenerateLevel();
    }

    private void TransitionToNextLevel()
    {
        screenCenter.position = levelGenerator.GetScreenCenterOf(levelGenerator.CurrentRoomIndex);

        for (int i = 0; i < playerManager.GetPlayers().Count; i++)
        {
            // Its for translating them to the next level, with some offset (to not be in center of screen))
            playerManager.GetPlayers()[i].transform.position = new Vector3(screenCenter.position.x + i, playerManager.GetPlayers()[i].transform.position.y, screenCenter.position.z - playersPositionOffset);
        }
        
    }

}