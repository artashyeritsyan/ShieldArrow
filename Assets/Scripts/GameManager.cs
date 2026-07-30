using System;
using UnityEngine;

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

    public LevelStates CurrentLevelState { get; private set; }

    public event Action<LevelStates> OnLevelStateChanged;

    [Header("Camera")]
    // This is the camera position
    [SerializeField] Transform screenCenter;


    [Header("Positions")]
    [SerializeField] Vector3 cameraStartPosition;
    [SerializeField] float lobbyTransitionDelta = 16;
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

    public void Update()
    {
        Debug.Log(CurrentLevelState);
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

            screenCenter.position += new Vector3(0, 0, lobbyTransitionDelta);
        }
        else if (CurrentLevelState == LevelStates.Aisle)
        {
            CurrentLevelState = LevelStates.Room;

            screenCenter.position += new Vector3(0, 0, aisleTransitionDelta);
        }

            OnLevelStateChanged?.Invoke(CurrentLevelState);
    }

}