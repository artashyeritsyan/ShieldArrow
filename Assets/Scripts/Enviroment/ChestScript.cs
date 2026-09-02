using Unity.VisualScripting;
using UnityEngine;

public class ChestScript : MonoBehaviour
{
    private Animator animator;
    private float blendValue = 0;
    private float targetValue = 0;
    [SerializeField] private float transitionTime = 0.5f;

    [SerializeField] AudioClip[] OpeningSounds;
    [SerializeField] AudioClip ClosingSound;

    public bool IsOpen => (targetValue == 0) ? false : true;

    void Start()
    {
        animator = GetComponent<Animator>();
        blendValue = 0;
        targetValue = 0;
    }

    void Update()
    {
        blendValue = Mathf.Lerp(blendValue, targetValue, transitionTime);
        animator.SetFloat("Blend", blendValue);
    }


    public void OpenChest()
    {
        targetValue = 1;

        int randomSound = Random.Range(0, OpeningSounds.Length);
        AudioManager.Play(OpeningSounds[randomSound]);
    }

    public void CloseChest()
    {
        targetValue = 0;

        AudioManager.Play(ClosingSound);
    }
}
