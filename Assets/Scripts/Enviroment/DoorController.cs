using UnityEngine;

public enum EDoorType
{
    GameStart,
    EnterTheRoom,
    LeaveTheRoom,
    BossRoom
}

public class DoorController : MonoBehaviour
{
    [SerializeField] EDoorType doorType;

    private void Start()
    {
        GetComponent<Collider>().enabled = true;
    }

    public void SetDoorType(EDoorType doorType)
    {
        this.doorType = doorType;
    }

    public EDoorType GetDoorType()
    {
        return doorType;
    }


    //private void SubscribeToEvents()
    //{
    //    if (GameManager.Instance == null)
    //    {
    //        Debug.LogError("GameMana  ger.Instance is null.", this);
    //        return;
    //    }

    //    GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
    //    GameManager.Instance.OnLevelStateChanged += HandleLevelStateChanged;

    //}

    //private void OnDestroy()
    //{
    //    if (GameManager.Instance != null)
    //    {
    //        GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
    //        GameManager.Instance.OnLevelStateChanged -= HandleLevelStateChanged;
    //    }
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && GameManager.Instance.CurrentGameState != GameStates.Pause)
        {
            CallDoorEvent();
            //GameManager.Instance.NextLevel();

            Debug.Log("Player enter the door collider" + doorType);
        }
    }

    private void CallDoorEvent()
    {
        switch (doorType)
        {
            case EDoorType.GameStart:
                if (GameManager.Instance.CurrentLevelState == LevelStates.Lobby)
                {
                    GameManager.Instance.SetGameState(GameStates.Playing);
                }

                GameManager.Instance.SetLevelState(LevelStates.Aisle);
                break;
            case EDoorType.EnterTheRoom:
                GameManager.Instance.SetLevelState(LevelStates.Room);
                break;
            case EDoorType.LeaveTheRoom:
                GameManager.Instance.SetLevelState(LevelStates.Aisle);
                break;
            case EDoorType.BossRoom:
                GameManager.Instance.SetLevelState(LevelStates.Boss);
                break;
            default:
                break;


        }

        GetComponent<Collider>().enabled = false;
    }


}
