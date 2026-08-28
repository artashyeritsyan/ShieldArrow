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

    [SerializeField] LevelGenerator LevelGenerator;
    [SerializeField] int currentRoomIndex = 0;


    [Header("Players Manager")]
    [SerializeField] PlayerManager playerManager;
    [SerializeField] float playersPositionOffset = 4;

    [Header("Camera")]
    // This is the camera position
    [SerializeField] Transform screenCenter;
    [SerializeField] float cameraSpeed = 1;
    private bool cameraMoving = false;
    public bool CameraMoving => cameraMoving;


    [Header("Positions")]
    [SerializeField] Vector3 cameraStartPosition;
    //[SerializeField] float roomTransitionDelta = 16;
    //[SerializeField] float aisleTransitionDelta = 20;

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
    }

    private void Start()
    {
        LevelGenerator.GenerateLevel();
    }

    private void Update()
    {
        if (cameraMoving)
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
        CheckCameraMoving();
    }

    public void SetLevelState(LevelStates newState)
    {
        if (CurrentLevelState == newState)
            return;

        if (CurrentLevelState == LevelStates.Lobby)
        {
            currentRoomIndex = 1;
        }
        else
        {
            currentRoomIndex += 1;
        }

        if (newState == LevelStates.Lobby)
            currentRoomIndex = 0;

        CurrentLevelState = newState;

        TransitionToNextLevel();
        OnLevelStateChanged?.Invoke(CurrentLevelState);

        CheckCameraMoving();
    }

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
    }

    private void TransitionToNextLevel()
    {
        screenCenter.position = LevelGenerator.GetScreenCenterOf(currentRoomIndex - 1);

        for (int i = 0; i < playerManager.GetPlayers().Count; i++) 
        {
            // Its for translating them to the next level, with some offset (to not be in center of screen))
            playerManager.GetPlayers()[i].transform.position = new Vector3(screenCenter.position.x + i, playerManager.GetPlayers()[i].transform.position.y, screenCenter.position.z - playersPositionOffset);
        }
        
    }

}