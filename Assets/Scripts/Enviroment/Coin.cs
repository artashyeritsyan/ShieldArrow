using UnityEngine;

public class Coin : ItemScript
{
    [SerializeField] private int value = 1;

    public override void Collected(PlayerController player)
    {
        base.Collected(player);
        // TODO: add here some logic 
        // e.g AddCoins(value);
        Debug.Log($"Collected {value} coins");

        //ObjectPoolManager.Instance.Return(this);
        GameManager.Instance.AddCoins(value);

        ReturnToPool();
    }

}
