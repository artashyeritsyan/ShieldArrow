using System.Collections.Generic;
using UnityEngine;

public class ShopItemsGenerator : MonoBehaviour
{
    [SerializeField] GameObject[] items;
    
    public void SpawnShopItems(Transform[] slots, int roomNumber)
    {
        List<GameObject> availableItems = new List<GameObject>(items);

        foreach (Transform slot in slots)
        {
            if (availableItems.Count == 0)
                break;

            int itemIndex = Random.Range(0, availableItems.Count);

            GameObject item = Instantiate(availableItems[itemIndex], slot);

            item.transform.localPosition = Vector3.zero;
            //item.transform.localRotation = Quaternion.identity;

            ItemUpgrade itemUpgrade = item.GetComponent<ItemUpgrade>();

            //TODO: Add here another modifiers of price. (Choosen door)
            itemUpgrade.SetPrice(itemUpgrade.BasePrice * roomNumber);

            availableItems.RemoveAt(itemIndex);
        }
    }
}
