using UnityEngine;
using DG.Tweening;

public class ItemScript : PooledObject
{
    [Header("DOTween parameters")]
    [SerializeField] float rotationPeriod = 2f;
    [SerializeField] float scalingTime = 0.5f;
    [SerializeField] float scalingMaxOffset = 1.2f;
    [SerializeField] Transform tweeningObject;

    //[SerializeField] bool isInteractable = false;

    [Header("Sound Effects")]
    [SerializeField] AudioClip[] CollectSoundEffects;

    [Header("Visual Effects")]
    public GameObject pickupEffect;


    public virtual void Start()
    {
        if (GameManager.Instance.CurrentGameState != GameStates.Pause)
        {
            StartTweening();
        }
    }


    //protected void ReturnToPool()
    //{
    //    ObjectPoolManager.Instance.Return(this);
    //}


    public virtual void OnSpawn() 
    {
        StartTweening();
    }
    public virtual void OnDespawn() 
    {
        transform.DOKill();
    }


    public virtual void Collected(PlayerController player)
    {
        // TODO: MAke the Object pool for collectable items` Coins and etc.
        //Destroy(gameObject);
        PlaySound();

        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, transform.rotation);
        }
    }

    protected virtual void PlaySound() {
        int soundIndex = Random.Range(0, CollectSoundEffects.Length);

        float volume = Random.Range(1f, 1.4f);
        float pitch = Random.Range(0.8f, 1.2f);

        AudioManager.Play(CollectSoundEffects[soundIndex], volume, pitch);
    }

    private void StartTweening()
    {
        //transform.DOMoveY(transform.position.y + scalingMaxOffset, scalingTime).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

        if (tweeningObject == null)
        {
            tweeningObject = transform;
        }

        tweeningObject.DORotate(new Vector3(0f, 360f, 0f), rotationPeriod, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental)
            .SetLink(tweeningObject.gameObject);

        tweeningObject.DOScale(tweeningObject.localScale * scalingMaxOffset, scalingTime)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
            .SetLink(tweeningObject.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //if (!isInteractable)
            //{
                Collected(other.GetComponent<PlayerController>());
            //}
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }

}
