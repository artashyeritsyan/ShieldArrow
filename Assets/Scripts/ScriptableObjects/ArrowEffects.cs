using UnityEngine;

[CreateAssetMenu(fileName = "ArrowEffects", menuName = "Scriptable Objects/ArrowEffects")]
public class ArrowEffects : ScriptableObject
{
    [SerializeField] public GameObject wallImpactEffect;
    [SerializeField] public GameObject shieldImpactEffect;
    [SerializeField] public GameObject mysthicImpactEffect;
    [SerializeField] public GameObject enemyHitEffect;

    [SerializeField] public GameObject constHitEffect;

}
