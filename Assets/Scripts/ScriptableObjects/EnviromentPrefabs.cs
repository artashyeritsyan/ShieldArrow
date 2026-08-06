using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnviromentPrefabs", menuName = "Scriptable Objects/EnviromentPrefabs")]
public class EnviromentPrefabs : ScriptableObject
{
    [SerializeField] GameObject[] prefabs;
    public GameObject[] Prefabs => prefabs;
    public int Size => prefabs.Length;
}
