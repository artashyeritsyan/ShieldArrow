using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("Parameters")]
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float shootingSpeed = 1f;
    [SerializeField] float shotsInRow = 1f;
    [SerializeField] float delayBetweenShots = 0.7f;

    [Header("Stats")] // ADDED
    [SerializeField] private CoreStats coreStats; // ADDED

    public CoreStats CoreStats => coreStats; // ADDED
    public Team Team => Team.Enemy; // ADDED

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
        coreStats.currentHP = coreStats.maxHP; // ADDED
        canAttack = true;
    }

    void Update()
    {
        if (isDead || isInactive || isPause) // ADDED
            return; // ADDED

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
            if (player == null)
                continue;

            float dist = Vector3.Distance(
                transform.position,
                player.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                closest = player;
            }
        }

        Debug.Log("Find player " + minDist);
        return closest;
    }

    public void TakeDamage( // ADDED
        DamageData damageData, // ADDED
        CoreStats attackerStats) // ADDED
    {
        if (isDead) // ADDED
            return; // ADDED

        DamageSystem.ProcessDamage( // ADDED
            damageData, // ADDED
            attackerStats, // ADDED
            coreStats); // ADDED

        OnDamageReceived(); // ADDED

        if (coreStats.currentHP <= 0f) // ADDED
        {
            Die(); // ADDED
        }
    }

    private void OnDamageReceived() // ADDED
    {
        Debug.Log( // ADDED
            $"{name} received damage. Remaining HP: " + // ADDED
            $"{coreStats.currentHP}/{coreStats.maxHP}"); // ADDED

        if (animator != null) // ADDED
        {
            animator.SetTrigger("Hit"); // ADDED
        }

        // Update your health bar here.
        // Example:
        // healthBar.SetValue(coreStats.currentHP, coreStats.maxHP);
    }

    private void Die() // ADDED
    {
        if (isDead) // ADDED
            return; // ADDED

        isDead = true; // ADDED
        canAttack = false; // ADDED
        canMove = false; // ADDED

        StopAllCoroutines(); // ADDED

        if (animator != null) // ADDED
        {
            animator.SetBool("Dead", true); // ADDED
        }

        if (weaponScript != null) // ADDED
        {
            weaponScript.enabled = false; // ADDED
        }

        Collider enemyCollider = GetComponent<Collider>(); // ADDED

        if (enemyCollider != null) // ADDED
        {
            enemyCollider.enabled = false; // ADDED
        }

        Destroy(gameObject, 2f); // ADDED
    }

    void RotateTowards(Vector3 targetPos)
    {
        Vector3 dir =
            (targetPos - transform.position).normalized;

        dir.y = 0f;

        if (dir == Vector3.zero)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * rotationSpeed);
    }

    void HandleAttack()
    {
        if (!canAttack || isDead || isInactive) // CHANGED
            return; // ADDED

        if (weaponScript != null)
        {
            weaponScript.Shoot();
            PlayAttackSound();

            if (shotsInRow > 1)
            {
                StartCoroutine(ShootInRow());
            }
            else
            {
                StartCoroutine(ResetAttackCooldown());
            }

            canAttack = false;
        }
        else
        {
            canAttack = false;
            StartCoroutine(ResetAttackCooldown());
            PlayAttackSound();
        }

        AttackAnimation();

        Debug.Log(canAttack);
    }

    IEnumerator ShootInRow()
    {
        for (int i = 1; i < shotsInRow; i++)
        {
            yield return new WaitForSeconds(delayBetweenShots);

            if (isDead || isInactive) // ADDED
                yield break; // ADDED

            weaponScript.Shoot();
            PlayAttackSound();

            if (animator != null) // CHANGED
            {
                animator.SetTrigger("Attack");
            }
        }

        StartCoroutine(ResetAttackCooldown());

        if (animator != null) // CHANGED
        {
            animator.SetTrigger("Reload");
        }
    }

    public void PlayAttackSound()
    {
        PlayRandomSound(attackSoundEffects);
    }

    void PlayRandomSound(AudioClip[] sounds)
    {
        if (sounds == null || sounds.Length == 0) // CHANGED
            return; // ADDED

        int randomSound = Random.Range(0, sounds.Length);
        float randomVolume = Random.Range(0.7f, 0.9f);
        float randomPitch = Random.Range(0.8f, 1.2f);

        AudioManager.Play(
            sounds[randomSound],
            randomVolume,
            randomPitch);
    }

    IEnumerator ResetAttackCooldown()
    {
        canAttack = false;

        float cooldown;

        if (weaponScript != null)
        {
            cooldown = 1f / weaponScript.GetAttackSpeed(); // CHANGED
        }
        else
        {
            cooldown = 1f / shootingSpeed; // CHANGED
        }

        yield return new WaitForSeconds(cooldown);

        if (!isDead && !isInactive) // ADDED
        {
            canAttack = true;
        }
    }

    void AttackAnimation()
    {
        if (animator == null) // ADDED
            return; // ADDED

        animator.SetTrigger("Attack");

        if (weaponScript != null)
        {
            animator.SetFloat(
                "AttackSpeed",
                weaponScript.GetAttackSpeed());
        }
    }

    void HandleAnimation()
    {
        if (animator == null) // ADDED
            return; // ADDED

        blendValue = 0f;
        animator.SetFloat("Blend", blendValue);

        if (isDead || isInactive)
        {
            StopAllCoroutines();
            canAttack = false;
        }

        animator.SetBool("Dead", isDead);
        animator.SetBool("Inactive", isInactive);
    }
}