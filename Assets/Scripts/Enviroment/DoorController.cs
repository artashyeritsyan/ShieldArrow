using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] GameObject doorCanvas;
    [SerializeField] TextMeshProUGUI interactionText;

    private void Start()
    {
        GetComponent<Collider>().enabled = true;

        doorCanvas.SetActive(false);

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

        interactionText.text = $"Press {binding} to start";

        doorCanvas.SetActive(true);

        doorCanvas.transform.LookAt(
            doorCanvas.transform.position + Camera.main.transform.rotation * Vector3.forward,
            Camera.main.transform.rotation * Vector3.up);

        //CallDoorEvent();

        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            player.SetInteraction(InteractionType.Door, gameObject);
        }

        Debug.Log("Player entered the door collider " + doorType);
    }


    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Player")) {
            doorCanvas.SetActive(false);

            PlayerController player = other.GetComponent<PlayerController>();

            if (player != null)
            {
                player.ClearInteraction(InteractionType.Door);
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
                    Debug.LogWarning("MAKE PLAying");
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
