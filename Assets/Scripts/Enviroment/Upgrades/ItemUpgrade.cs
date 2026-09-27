using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum UpgradeType
{
    Speed,
    Damage,
    AttackSpeed,
    MaxHp,
}

public class ItemUpgrade : ItemScript
{
    [SerializeField] UpgradeType upgradeType;
    [SerializeField] float upgradeValue = 0.25f;

    [Header("UI text")]
    [SerializeField] GameObject canvas;
    [SerializeField] TextMeshProUGUI interactionText;

    [Header("Visuals")]
    //[SerializeField] private bool showOutline = true;
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlineWidth = 3f;
    private Outline outline;

    private readonly string[] upgradeTexts =
    {
        "Increase movement speed",
        "Increase damage",
        "Increase attack speed",
        "Increase maximum HP"
    };  


    public override void Start()
    {
        base.Start();

        if (canvas != null)
        {
            canvas.SetActive(false);
        }

        SetupOutline();
    }

    public override void Collected(PlayerController player)
    {
        base.Collected(player);

        switch (upgradeType)
        {
            case UpgradeType.Speed:
                player.GetStats().core.speed += upgradeValue;
                break;
            case UpgradeType.Damage:
                player.GetStats().core.damage += upgradeValue;
                break;
            case UpgradeType.MaxHp:
                player.GetStats().core.maxHP += upgradeValue;
                break;
            case UpgradeType.AttackSpeed:
                player.GetStats().core.attackSpeed+= upgradeValue;
                break;
        }

        Destroy(gameObject);
    }

    private void SetupOutline()
    {
        outline = GetComponent<Outline>();


        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }

        //outline.enabled = true;
        outline.OutlineColor = outlineColor;
        outline.OutlineWidth = outlineWidth;

        outline.enabled = false;
    }


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

        if (canvas != null)
        {
            interactionText.text = $"{upgradeTexts[(int)upgradeType]}\nPress {binding} to pick up";

            canvas.SetActive(true);
        }

        outline.enabled = true;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetInteraction(InteractionType.Upgrade, gameObject);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (canvas != null)
            {
                canvas.SetActive(false);
            }

            // TODO: Check this part, cause 2 players can be in trigger at the same time
            outline.enabled = false;

            PlayerController player = other.GetComponent<PlayerController>();
            player.ClearInteraction(InteractionType.Upgrade);
        }
    }

}
