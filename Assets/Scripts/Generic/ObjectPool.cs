using System.Collections.Generic;
using UnityEngine;


public interface IObjectPool
{
    PooledObject Get();
    void Return(PooledObject obj);
}

public class ObjectPool<T> : IObjectPool where T : PooledObject
{
    private readonly T prefab;
    private readonly Transform parent;

    private readonly Queue<T> availableObjects = new();

    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < initialSize; i++)
        {
            T obj = CreateObject();
            availableObjects.Enqueue(obj);
        }
    }

    private T CreateObject()
    {
        T obj = Object.Instantiate(prefab, parent);

        obj.SetPoolPrefab(prefab.gameObject);

        obj.gameObject.SetActive(false);

        return obj;
    }

    public T GetTyped()
    {
        T obj;

        if (availableObjects.Count > 0)
        {
            obj = availableObjects.Dequeue();
        }
        else
        {
            obj = CreateObject();
        }

        obj.gameObject.SetActive(true);

        return obj;
    }

    public void ReturnTyped(T obj)
    {
        obj.gameObject.SetActive(false);

        availableObjects.Enqueue(obj);
    }

    PooledObject IObjectPool.Get()
    {
        return GetTyped();
    }

    void IObjectPool.Return(PooledObject obj)
    {
        ReturnTyped((T)obj);
    }
}