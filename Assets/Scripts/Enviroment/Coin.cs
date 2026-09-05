using UnityEngine;

public class Coin : ItemScript
{
    [SerializeField] private int value = 1;

    public override void Collected(PlayerController player)
    {
        // TODO: add here some logic 
        // e.g AddCoins(value);
        base.Collected(player);

        ObjectPoolManager.Instance.Return(this);


        GameManager.Instance.AddCoins(value);
    }

}
