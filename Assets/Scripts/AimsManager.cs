using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class AimsManager : MonoBehaviour
{
    [SerializeField] GameObject aimUIPrefab;

    private List<GameObject> playersAims = new List<GameObject>();
    private readonly Dictionary<Transform, GameObject> aimsByTarget = new();

    public void AddPlayerAim(Transform target, Color color)
    {
        if (target == null || aimsByTarget.ContainsKey(target))
            return;

        foreach (GameObject aim in playersAims)
        {
            if (aim.GetComponent<Image>().color == color)
            {
                color = new Color(
                    Random.value,
                    Random.value,
                    Random.value
                );

                break;
            }
        }

        GameObject uiInstance = Instantiate(aimUIPrefab, transform);

        Image image = uiInstance.GetComponent<Image>();
        AimUIFollow uiFollow = uiInstance.GetComponent<AimUIFollow>();

        image.color = color;
        uiFollow.SetTarget(target);

        playersAims.Add(uiInstance);
        aimsByTarget.Add(target, uiInstance);
    }

    public void RemovePlayerAim(int playerIndex)
    {
        Destroy(playersAims[playerIndex]);
    }

    public void RemovePlayerAim(Transform aimTarget)
    {
        if (aimTarget == null)
            return;

        if (!aimsByTarget.TryGetValue(aimTarget, out GameObject aimUI))
        {
            return;
        }

        playersAims.Remove(aimUI);
        aimsByTarget.Remove(aimTarget);

        Destroy(aimUI);
    }

    public void SetActiveAim(int playerIndex, bool  isActive)
    {
        playersAims[playerIndex].SetActive(isActive);
    }

    public void SetActiveAllAims(bool  isActive)
    {
        foreach (GameObject aim in playersAims)
        {
            aim.SetActive(isActive);
        }
    }
}
