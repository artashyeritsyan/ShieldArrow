using UnityEngine;
using DG.Tweening;

public class ItemScript : MonoBehaviour
{
    [Header("DOTween parameters")]
    [SerializeField] float rotationPeriod = 2f;
    [SerializeField] float bouncingTime = 1f;
    [SerializeField] float bouncingMaxOffset = 0.4f;


    [Header("Sound Effects")]
    [SerializeField] AudioClip[] CollectSoundEffects;

    void Start()
    {
        if (GameManager.Instance.CurrentGameState != GameStates.Pause)
        {
            StartTweening();
        }
    }


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

        
    }

    protected virtual void ReturnToPool()
    {
        ObjectPoolManager.Instance.Return(this);
    }

    protected virtual void PlaySound() {
        int soundIndex = Random.Range(0, CollectSoundEffects.Length);

        float volume = Random.Range(1f, 1.4f);
        float pitch = Random.Range(0.8f, 1.2f);

        AudioManager.Play(CollectSoundEffects[soundIndex], volume, pitch);
    }

    private void StartTweening()
    {
        transform.DOMoveY(transform.position.y + bouncingMaxOffset, bouncingTime).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

        transform.DORotate(new Vector3(0f, 360f, 0f), rotationPeriod, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collected(other.GetComponent<PlayerController>());
        }

    }

}
