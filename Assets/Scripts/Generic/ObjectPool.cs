using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class ObjectPool<T> where T : Component
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
        obj.gameObject.SetActive(false);

        return obj;
    }

    public T Get()
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


    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        availableObjects.Enqueue(obj);
    }

}
