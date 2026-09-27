using System.Runtime.CompilerServices;
using UnityEngine;

public enum UpgradeType
{
    Speed,
    Damage,
    AttackSpeed,
    MaxHp,
}

public class SpeedUpgrade : ItemScript
{
    [SerializeField] UpgradeType upgradeType;
    [SerializeField] float upgradeValue = 0.25f;

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
}
