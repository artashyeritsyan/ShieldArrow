using System.Collections;
using UnityEngine;

public class AssasinController : PlayerController
{
    [Header("Weapon")]
    private WeaponScript weaponScript;

    [Header("Attack")]
    [SerializeField] float attackStartDelay = 0.1f;
    [SerializeField] float attackTime = 0.3f;
    [SerializeField] float attackCooldown = 5;
    [SerializeField] float animFinishTime = 0.1f;



    protected override void Start()
    {
        base.Start();
        InitialiseWeapon();
        canAttack = true;
        layerTargetWeight = 1;
    }

    protected override void Update()
    {
        base.Update();

        //layerCurrentWeight = Mathf.Lerp(layerCurrentWeight, layerTargetWeight, animLayerTransitionTime * Time.deltaTime);
        //animator.SetLayerWeight(1, layerCurrentWeight);
    }

    public override void AdditiveAnimation()
    {

    }

    private void InitialiseWeapon()
    {
        weaponScript = transform.GetComponentInChildren<WeaponScript>();
        weapon = weaponScript.gameObject;
    }

    void PlayRandomSound(AudioClip[] sounds)
    {
        if (sounds.Length > 0)
        {
            int randomSound = Random.Range(0, sounds.Length);
            float randomVolume = UnityEngine.Random.Range(0.7f, 0.9f);
            float randomPitch = UnityEngine.Random.Range(0.8f, 1.2f);
            AudioManager.Play(sounds[randomSound], randomVolume, randomPitch);
        }
    }

    public override void PlayAttackSound()
    {
        PlayRandomSound(attackSoundEffects);
    }

    public override void Attack()
    {
        if (canAttack)
        {
            AttackAnimation();

            

            StartCoroutine(ResetAttackCooldown());
            StartCoroutine(WaitForAttackFinished());

            PlayAttackSound();
        }
    }

    void AttackAnimation()
    {
        layerCurrentWeight = 0.1f;
        layerTargetWeight = 0;

        animator.SetTrigger("Attack");
        animator.SetFloat("AttackSpeed", weaponScript.GetAttackSpeed());
    }

    IEnumerator WaitForAttackFinished()    {
        yield return new WaitForSeconds(attackStartDelay);
        SetCanMove(false);
        weaponScript.Shoot();

        yield return new WaitForSeconds(attackTime);
        SetCanMove(true);

        yield return new WaitForSeconds(animFinishTime);
        layerTargetWeight = 1;
    }

    IEnumerator ResetAttackCooldown()
    {
        canAttack = false;
        // 1 stands for 1 second
        yield return new WaitForSeconds(1 / weaponScript.GetAttackSpeed());
        canAttack = true;
    }
    
}
