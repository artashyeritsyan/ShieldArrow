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

    public LevelStates CurrentLevelState { get; private set; }
    public event Action<LevelStates> OnLevelStateChanged;

    [Header("Players Manager")]
    [SerializeField] PlayerManager playerManager;
    [SerializeField] float playersPositionOffset = 4;

    [Header("Camera")]
    // This is the camera position
    [SerializeField] Transform screenCenter;
    [SerializeField] float cameraSpeed = 1;


    [Header("Positions")]
    [SerializeField] Vector3 cameraStartPosition;
    [SerializeField] float roomTransitionDelta = 16;
    [SerializeField] float aisleTransitionDelta = 20;

    private void Awake() 
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentLevelState = LevelStates.Lobby;
        screenCenter.position = cameraStartPosition;
    }

    private void Update()
    {
        if (CurrentGameState == GameStates.Playing && CurrentLevelState == LevelStates.Aisle)
        {
            screenCenter.position += Vector3.forward * cameraSpeed * Time.deltaTime;
        }
    }
    public void SetGameState(GameStates newState)
    {
        if (CurrentGameState == newState)
            return;

        CurrentGameState = newState;
        OnGameStateChanged?.Invoke(CurrentGameState);
    }

    public void SetLevelState(LevelStates newState)
    {
        if (CurrentLevelState == newState)
            return;

        CurrentLevelState = newState;
        OnLevelStateChanged?.Invoke(CurrentLevelState);
    }

    public void NextLevel()
    {
        if (CurrentLevelState == LevelStates.Lobby)
        {
            CurrentLevelState = LevelStates.Aisle;
            CurrentGameState = GameStates.Playing;

            TransitionToNextLevel(roomTransitionDelta);
        }
        else if (CurrentLevelState == LevelStates.Room)
        {
            CurrentLevelState = LevelStates.Aisle;

            TransitionToNextLevel(roomTransitionDelta);
        }
        else if (CurrentLevelState == LevelStates.Aisle)
        {
            CurrentLevelState = LevelStates.Room;

            TransitionToNextLevel(aisleTransitionDelta);
        }

        OnGameStateChanged?.Invoke(CurrentGameState);
        OnLevelStateChanged?.Invoke(CurrentLevelState);
    }

    private void TransitionToNextLevel(float transitionDelta)
    {
        screenCenter.position += new Vector3(0, 0, transitionDelta);

        for (int i = 0; i < playerManager.GetPlayers().Count; i++) 
        {
            // Its for translating them to the next level, with some offset (to not be in center of screen))
            playerManager.GetPlayers()[i].transform.position = new Vector3(screenCenter.position.x + i, playerManager.GetPlayers()[i].transform.position.y, screenCenter.position.z - playersPositionOffset);
        }
        
    }

}