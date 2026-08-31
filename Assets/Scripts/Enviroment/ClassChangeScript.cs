using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClassChangeScript : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] TextMeshProUGUI interactionText;


    private void Start()
    {
        GetComponent<Collider>().enabled = true;

        canvas.SetActive(false);

    }

    // TODO: Add the boolean that allows changing class if the player enters into the trigger

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") ||
            GameManager.Instance.CurrentGameState == GameStates.Pause)
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

        interactionText.text = $"Press {binding} to change class";

        canvas.SetActive(true);

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetInteraction(InteractionType.ClassChange);
        }


        //canvas.transform.LookAt(
        //    canvas.transform.position + Camera.main.transform.rotation * Vector3.forward,
        //    Camera.main.transform.rotation * Vector3.up);

    }


    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            canvas.SetActive(false);

            PlayerController player = other.GetComponent<PlayerController>();
            player.ClearInteraction(InteractionType.ClassChange);
        }
    }

}
