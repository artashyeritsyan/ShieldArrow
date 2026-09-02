using UnityEngine;
using DG.Tweening;

public class ItemScript : MonoBehaviour
{
    [SerializeField] float rotationTime = 2f;

    void Start()
    {
        if (GameManager.Instance.CurrentGameState != GameStates.Pause)
        {
            transform.DOMoveY(transform.position.y + 0.4f, 0.8f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

            transform.DORotate(new Vector3(0f, 360f, 0f), rotationTime, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
        }
    }


}
