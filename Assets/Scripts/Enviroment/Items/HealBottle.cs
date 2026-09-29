using UnityEngine;

public class HealBottle : ItemScript
{
    [SerializeField] private int value = 2;

    public override void Collected(PlayerController player)
    {
        base.Collected(player);

        player.GetStats().core.currentHP += value;
        if (player.GetStats().core.maxHP < player.GetStats().core.currentHP)
        {
            player.GetStats().core.currentHP = player.GetStats().core.maxHP;
        }


        Destroy(gameObject);

    }
}
