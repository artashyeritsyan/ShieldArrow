using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    private Dictionary<Type, object> pools = new();

    private void Awake()
    {
        Instance = this;
    }

    public void CreatePool<T>(T prefab, int size) where T : Component
    {
        var type = typeof(T);

        if (pools.ContainsKey(type))
        {
             Debug.LogWarning($"Pool for type {type} already exists.");
            return;
        }

        var pool = new ObjectPool<T>(prefab, size, transform);
        pools.Add(type, pool);
    }

    public T Spawn<T>(Vector3 position, Quaternion rotation) where T : Component
    {
        var type = typeof(T);

        if (!pools.ContainsKey(type))
        {
            Debug.LogError($"No pool for {type}! Create it first.");
            return null;
        }

        var pool = (ObjectPool<T>)pools[type];
        var obj = pool.Get();

        obj.transform.position = position;
        obj.transform.rotation = rotation;

        return obj;
    }

    public void Return<T>(T obj) where T : Component
    {
        var type = typeof(T);

        if (!pools.ContainsKey(type))
        {
            Debug.LogError($"No pool for {type}!");
            return;
        }

        var pool = (ObjectPool<T>)pools[type];
        pool.Return(obj);
    }
}



