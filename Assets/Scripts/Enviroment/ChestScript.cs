using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class ChestScript : MonoBehaviour
{
    private Animator animator;
    private float blendValue = 0;
    private float targetValue = 0;
    [SerializeField] private float transitionTime = 0.5f;

    [SerializeField] AudioClip[] OpeningSounds;
    [SerializeField] AudioClip ClosingSound;

    [Header("Visuals")]
    [SerializeField] private bool showOutline = true;
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlineWidth = 3f;

    public bool IsOpen => (targetValue == 0) ? false : true;

    void Start()
    {
        animator = GetComponent<Animator>();
        blendValue = 0;
        targetValue = 0;

        SetupOutline();
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

    private void SetupOutline()
    {
        Outline outline = GetComponent<Outline>();

        if (!showOutline)
        {
            if (outline != null)
            {
                outline.enabled = false;
            }

            return;
        }
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }

        outline.enabled = true;
        outline.OutlineColor = outlineColor;
        outline.OutlineWidth = outlineWidth;
    }


}
