using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    private readonly Dictionary<GameObject, IObjectPool> pools = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void CreatePool<T>(T prefab, int initialSize)
        where T : PooledObject
    {
        GameObject key = prefab.gameObject;

        if (pools.ContainsKey(key))
        {
            Debug.LogWarning($"Pool already exists for prefab: {key.name}");

            return;
        }

        GameObject poolParent = new GameObject($"{prefab.name}_Pool");

        poolParent.transform.SetParent(transform);

        ObjectPool<T> pool =
            new ObjectPool<T>(
                prefab,
                initialSize,
                transform
            );

        pools.Add(key, pool);
    }

    public T Spawn<T>(
        T prefab,
        Vector3 position,
        Quaternion rotation)
        where T : PooledObject
    {
        GameObject key = prefab.gameObject;

        if (!pools.TryGetValue(key, out IObjectPool pool))
        {
            Debug.LogError($"No pool exists for prefab: {key.name}");

            return null;
        }

        T obj = (T)pool.Get();

        obj.transform.SetPositionAndRotation(
            position,
            rotation
        );

        return obj;
    }

    public void Return(PooledObject obj)
    {
        GameObject key = obj.PoolPrefab;

        if (key == null)
        {
            Debug.LogError(
                $"{obj.name} has no PoolPrefab assigned!"
            );

            return;
        }

        if (!pools.TryGetValue(key, out IObjectPool pool))
        {
            Debug.LogError($"No pool exists for prefab: {key.name}");

            return;
        }

        pool.Return(obj);
    }

    public void ClearPools()
    {
        foreach (IObjectPool pool in pools.Values)
        {
            pool.Clear();
        }
    }
}    