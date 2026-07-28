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

}