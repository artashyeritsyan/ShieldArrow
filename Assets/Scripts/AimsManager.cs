using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class AimsManager : MonoBehaviour
{
    [SerializeField] GameObject aimUIPrefab;

    private List<GameObject> playersAims = new List<GameObject>();

    public void AddPlayerAim(Transform target, Color color)
    {
        foreach (GameObject aim in playersAims)
        {
            if (aim.GetComponent<Image>().color == color) {
                color = new Color(
                    Random.value,
                    Random.value,
                    Random.value
                );
                break;
            }
        }

        GameObject uiInstance = Instantiate(aimUIPrefab, transform);
        AimUIFollow ui = uiInstance.GetComponent<AimUIFollow>();
        ui.GetComponent<Image>().color = color;
        ui.SetTarget(target);

        playersAims.Add(uiInstance);
    }

    public void RemovePlayerAim(int playerIndex)
    {
        Destroy(playersAims[playerIndex]);
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
