
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public GameObject PoolPrefab { get; private set; }

    public void SetPoolPrefab(GameObject prefab)
    {
        PoolPrefab = prefab;
    }

    public void ReturnToPool()
    {
        if (PoolPrefab == null)
        {
            // Object was placed manually in the scene
            gameObject.SetActive(false);
            return;
        }

        ObjectPoolManager.Instance.Return(this);
    }
}