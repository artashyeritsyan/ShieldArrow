
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
        ObjectPoolManager.Instance.Return(this);
    }
}