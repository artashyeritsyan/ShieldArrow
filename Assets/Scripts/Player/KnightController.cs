using System.Collections;
using UnityEngine;

public class KnightController : PlayerController
{
    [Header("Weapon")]
    private WeaponScript weaponScript;

    [Header("Attack")]
    [SerializeField] float attacStartDelay = 0.1f;
    [SerializeField] float attackTime = 0.3f;
    [SerializeField] float attackCooldown = 5;


    // Instead of Start()
    protected override void Start()
    {
        base.Start();

        canAttack = true;

        layerTargetWeight = 1;
    }

    protected override void Update()
    {
        base.Update();
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
            layerCurrentWeight = 0.5f;
            layerTargetWeight = 0;

            animator.SetTrigger("Attack");

            StartCoroutine(ResetAttackCooldown());
            StartCoroutine(WaitForAttackFinished());

            PlayAttackSound();
        }
    }

    IEnumerator WaitForAttackFinished()
    {
        yield return new WaitForSeconds(attacStartDelay);
        SetCanMove(false);

        yield return new WaitForSeconds(attackTime);
        layerTargetWeight = 1;
        SetCanMove(true);
    }

    IEnumerator ResetAttackCooldown()
    {
        canAttack = false;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

}
