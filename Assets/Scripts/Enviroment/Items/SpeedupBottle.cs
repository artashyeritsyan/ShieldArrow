using UnityEngine;

public class SpeedupBottle : ItemScript
{
    [SerializeField] private float value = 1;
    [SerializeField] private float potionDuration = 10f;

    public override void Collected(PlayerController player)
    {
        base.Collected(player);

        player.SpeedupEffect(potionDuration, value);

        Destroy(gameObject);

    }
}
