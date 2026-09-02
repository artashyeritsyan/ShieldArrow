using Unity.VisualScripting;
using UnityEngine;

public class ChestScript : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private float blendValue = 0;
    private float targetValue;
    [SerializeField] private float transitionTime = 0.5f;

    void Start()
    {
        animator = GetComponent<Animator>();
        blendValue = 0;
    }

    void Update()
    {
        blendValue = Mathf.Lerp(blendValue, targetValue, transitionTime);
        animator.SetFloat("Blend", blendValue);
    }

    public void OpenChest()
    {
        targetValue = 1;
    }

    public void CloseChest()
    {
        targetValue = 0;
    }
}
