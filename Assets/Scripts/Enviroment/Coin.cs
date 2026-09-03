using UnityEngine;

public class Coin : ItemScript
{
    [SerializeField] private int value = 1;

    public override void Collected()
    {
        // TODO: add here some logic 
        // e.g AddCoins(value);

        base.Collected();
    }

}
