using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public enum EDoorType
{
    GameStart,
    EnterTheRoom,
    LeaveTheRoom,
    BossRoom,
    //LevelFinished
}

public class DoorController : MonoBehaviour
{
    [SerializeField] EDoorType doorType;
    [SerializeField] GameObject doorCanvas;
    [SerializeField] TextMeshProUGUI interactionText;

    private void Start()
    {
        GetComponent<Collider>().enabled = true;

        doorCanvas.SetActive(false);

        GameManager.Instance.OnGameReset += HandleGameReset;
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
        if (!other.CompareTag("Player") || GameManager.Instance.CurrentGameState == GameStates.Pause)
            return;

        PlayerInput playerInput = other.GetComponent<PlayerInput>();

        if (playerInput == null)
            return;

        InputAction interactAction = playerInput.actions["Interact"];

        if (interactAction == null)
        {
            Debug.LogWarning("Interact action not found.");
            return;
        }

        string binding = interactAction.GetBindingDisplayString(
            InputBinding.DisplayStringOptions.DontIncludeInteractions,
            playerInput.currentControlScheme
        );

        switch (GameManager.Instance.CurrentLevelState)
        {
            case LevelStates.Lobby:
                interactionText.text = $"Press {binding} to start";
                break;
            case LevelStates.Aisle:
                if (doorType == EDoorType.BossRoom)
                {
                    interactionText.text = $"Press {binding} to enter Boss room";
                }
                interactionText.text = $"Press {binding} to enter room";

                break;
            case LevelStates.Room:
                interactionText.text = $"Press {binding} to leave room";

                break;
            case LevelStates.Boss:

                // TODO: Change it to "Next level" after creating new levels ))
                interactionText.text = $"Press {binding} to finish level";
                break;
            default:
                break;
        }

        

        if (GameManager.Instance.CurrentLevelState == LevelStates.Boss)
        {
        }

        doorCanvas.SetActive(true);

        doorCanvas.transform.LookAt(
            doorCanvas.transform.position + Camera.main.transform.rotation * Vector3.forward,
            Camera.main.transform.rotation * Vector3.up);


        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            player.SetInteraction(InteractionType.Door, gameObject);
        }

        Debug.Log("Player Entered the door collider " + doorType);
    }


    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Player")) {
            //doorCanvas.SetActive(false);
            Debug.Log("Player Leaved the door collider " + doorType);

            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                player.ClearInteraction(InteractionType.Door);
                doorCanvas.SetActive(false);
            }

        }

        if (other.CompareTag("UpperBorder"))
        {
            if (GameManager.Instance.CurrentGameState == GameStates.Playing &&
                GameManager.Instance.CurrentLevelState == LevelStates.Aisle && GameManager.Instance.CameraMoving)
            {
                GameManager.Instance.SetCameraMoving(false);
            }
        }
    }

    public void Interact()
    {
        CallDoorEvent();
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
                GameManager.Instance.SetCameraMoving(true);
                break;
            case EDoorType.EnterTheRoom:
                GameManager.Instance.SetLevelState(LevelStates.Room);
                GameManager.Instance.SetCameraMoving(false);
                break;
            case EDoorType.LeaveTheRoom:
                if (GameManager.Instance.CurrentLevelState == LevelStates.Boss)
                {
                    // Call GameWin Function. (It should show the win menu, then restarts)
                    // Maybe opens new character, or new levels (didnt implemented yet
                    GameManager.Instance.SetCameraMoving(false);
                    GameManager.Instance.GameWin();
                    break;
                }

                GameManager.Instance.SetLevelState(LevelStates.Aisle);
                GameManager.Instance.SetCameraMoving(true);

                break;
            case EDoorType.BossRoom:
                GameManager.Instance.SetLevelState(LevelStates.Boss);
                GameManager.Instance.SetCameraMoving(false);

                break;
            default:
                break;

        }

        GetComponent<Collider>().enabled = false;
    }
    void HandleGameReset()
    {
        //Debug.Log("Door COllider Turned ONN");
        GetComponent<Collider>().enabled = true;
        doorCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        //GameManager.Instance.OnGameReset += HandleGameReset;
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameReset -= HandleGameReset;


    }
        


}
