using UnityEngine;

public class WeaponEffect : ItemScript
{
    [SerializeField] private int value = 3;
    [SerializeField] private float potionDuration = 20f;

    public override void Collected(PlayerController player)
    {
        base.Collected(player);

        player.SpreadAttackEffect(potionDuration, value);

        Destroy(gameObject);

    }
}
