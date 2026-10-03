using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public enum ShopItemType
{
    UpgradeSpeed,
    UpgradeDamage,
    UpgradeAttackSpeed,
    UpgradeMaxHp,
    HealHp
}

public class ItemUpgrade : ItemScript
{
    [SerializeField] ShopItemType upgradeType;
    [SerializeField] float upgradeValue = 2;

    [Header("UI text")]
    [SerializeField] GameObject canvas;
    [SerializeField] TextMeshProUGUI interactionText;

    [Header("Visuals")]
    //[SerializeField] private bool showOutline = true;
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlineWidth = 3f;
    private Outline outline;

    [Header("Shop")]
    [SerializeField] bool isFree = false;
    [SerializeField] int price = 5;

    [SerializeField] int basePrice = 5;
    public int BasePrice => basePrice;
    

    private readonly string[] upgradeTexts =
    {
        "Speed",
        "Damage",
        "Attack speed",
        "Max HP",
        "HP"
    };  


    public override void Start()
    {
        base.Start();

        if (canvas != null)
        {
            canvas.SetActive(false);
        }
        price = basePrice;

        if (upgradeType != ShopItemType.HealHp)
            SetupOutline();
    }

    public int GetPrice() { return price; }

    public void SetPrice(int newPrice)
    {
        price = newPrice;
    }

    public override void Collected(PlayerController player)
    {
        if (!isFree && price > 0)
        {
            if (!GameManager.Instance.PurchaseFor(price))
            {
                return;
            }
        }

        base.Collected(player);

        switch (upgradeType)
        {
            case ShopItemType.UpgradeSpeed:
                player.GetStats().core.speed += upgradeValue;
                break;
            case ShopItemType.UpgradeDamage:
                player.GetStats().core.damage += upgradeValue;
                break;
            case ShopItemType.UpgradeMaxHp:
                player.GetStats().core.maxHP += upgradeValue;
                break;
            case ShopItemType.UpgradeAttackSpeed:
                player.AddAttackSpeedMultiplier(upgradeValue);
                //player.GetStats().core.attackSpeed += upgradeValue;
                break;
            case ShopItemType.HealHp:
                player.GetStats().core.currentHP += upgradeValue;
                if (player.GetStats().core.maxHP < player.GetStats().core.currentHP)
                {
                    player.GetStats().core.currentHP = player.GetStats().core.maxHP;
                }
                break;
        }

        player.ClearInteraction(InteractionType.Upgrade);
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
            if (isFree || price == 0)
            {
                interactionText.text = $"+{upgradeValue} to {upgradeTexts[(int)upgradeType]}\nPress {binding} to pick up";
            }
            else
            {
                interactionText.text = $"+{upgradeValue} to {upgradeTexts[(int)upgradeType]}\nPress {binding} to buy for {price} gold";
            }

            canvas.SetActive(true);
        }

        if (outline != null) 
        { 
            outline.enabled = true;
        }
            

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
            if (outline != null)
            {
                outline.enabled = false;
            }

            PlayerController player = other.GetComponent<PlayerController>();
            player.ClearInteraction(InteractionType.Upgrade);
        }
    }

}
