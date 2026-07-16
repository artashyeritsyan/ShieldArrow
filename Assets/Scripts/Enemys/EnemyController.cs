using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyController : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float shootingSpeed = 1f;


    [Header("Weapon")]
    [SerializeField] WeaponScript weaponScript;

    [Header("Animation")]
    [SerializeField] protected Animator animator;
    [SerializeField] float animTransitionTime = 5f;
    private float blendValue;

    [Header("Sound Effects")]
    public AudioClip[] attackSoundEffects;

    [Header("Main Modifiers")]
    [SerializeField] bool isDead;
    [SerializeField] bool isInactive;
    bool isPause;
    bool canMove;
    bool canAttack;


    private void Start()
    {
        canAttack = true;
    }

    void Update()
    {
        var target = GetClosestPlayer();
        if (target == null)
            return;

        RotateTowards(target.transform.position);
        HandleAttack();
        HandleAnimation();
    }

    PlayerController GetClosestPlayer()
    {
        float minDist = float.MaxValue;
        PlayerController closest = null;

        foreach (var player in PlayerManager.Instance.GetPlayers())
        {
            if (player == null) continue;

            float dist = Vector3.Distance(transform.position, player.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                closest = player;
            }
        }
        Debug.Log("Find player" +  minDist);
        return closest;
    }

    public void GetDamage()
    {

    }

    void RotateTowards(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        dir.y = 0f;

        if (dir == Vector3.zero)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
    }

    void HandleAttack()
    {
        if (canAttack)
        {
            weaponScript.Shoot();
            canAttack = false;

            Debug.Log(canAttack);

            AttackAnimation();
            StartCoroutine(ResetAttackCooldown());

            PlayAttackSound();

        }
    }

    public void PlayAttackSound()
    {
        PlayRandomSound(attackSoundEffects);
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

    IEnumerator ResetAttackCooldown()
    {
        canAttack = false;
        // 1 stands for 1 second
        yield return new WaitForSeconds(1 / weaponScript.GetAttackSpeed());
        canAttack = true;
    }

    void AttackAnimation()
    {
        //layerCurrentWeight = 0.1f;
        //layerTargetWeight = 0;

        animator.SetTrigger("Attack");
        animator.SetFloat("AttackSpeed", weaponScript.GetAttackSpeed());
    }

    void HandleAnimation()
    {
        //float playerVector = Mathf.Sqrt(Mathf.Pow(moveDirection.x, 2) + Mathf.Pow(moveDirection.z, 2));
        //blendValue = Mathf.Lerp(blendValue, playerVector, Time.deltaTime * animTransitionTime);
        blendValue = 0f;
        animator.SetFloat("Blend", blendValue);

        if (isDead || isInactive)
        {
            //StopCoroutine(ResetAttackCooldown());
            StopAllCoroutines();
            canAttack = false;
        }
        animator.SetBool("Dead", isDead);
        animator.SetBool("Inactive", isInactive);
    }

}