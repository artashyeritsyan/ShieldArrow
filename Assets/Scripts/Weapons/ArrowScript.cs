using UnityEngine;
using System.Collections;
using System.Linq.Expressions;

public class ArrowScript : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float speed = 4f;

    // Ricochet count means it ricochets N times
    // and is destroyed on collision N+1.
    [SerializeField] private int maxRicochetCount = 2;

    [SerializeField] private float sphereCastRadius = 0.05f;
    [SerializeField] private float castExtraDistance = 0.05f;

    [SerializeField] private LayerMask hitMask;

    [Header("Sounds")]
    [SerializeField] private AudioClip[] wallHitSoundEffects;
    [SerializeField] private AudioClip[] shieldHitSoundEffects;

    [Header("Visuals")]
    [SerializeField] private bool showOutline;
    //[SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlineWidth = 1f;

    [Header("VFX")]
    [SerializeField] ArrowEffects arrowEffect;
   

    private int ricochetsLeft; // CHANGED: float -> int
    private Vector3 direction;

    private GameObject owner;
    private Team ownerTeam;
    private DamageData damageData;
    private CoreStats attackerStats; 
    private bool isInitialized;

    private Rigidbody rb;

    private Coroutine lifetimeCoroutine;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        SetupProjectile();
    }

    private void SetupProjectile()
    {
        direction = transform.forward.normalized;
        ricochetsLeft = maxRicochetCount;

        SetupOutline();

        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
        }

        lifetimeCoroutine = StartCoroutine(DestroyArrow());
    }

    public void Initialize( 
        GameObject projectileOwner,
        Team projectileOwnerTeam,
        DamageData projectileDamage,
        CoreStats sourceStats) 
    {
        owner = projectileOwner;
        ownerTeam = projectileOwnerTeam;
        damageData = projectileDamage;
        attackerStats = sourceStats;
        isInitialized = true;

        direction = transform.forward.normalized; 
        ricochetsLeft = maxRicochetCount;
    }

    private void FixedUpdate()
    {
        if (!isInitialized)
        {
            return;
        }

        //float moveDistance = speed;
        float moveDistance = speed * Time.fixedDeltaTime;

        float castDistance = moveDistance + castExtraDistance;

        if (Physics.SphereCast(
                transform.position,
                sphereCastRadius,
                direction,
                out RaycastHit hit,
                castDistance,
                hitMask,
                QueryTriggerInteraction.Ignore))
        {
            if (OnHit(hit)) return;
        }

        //transform.position += direction * moveDistance;
        rb.MovePosition(rb.position + direction * moveDistance);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        float moveDistance = speed * Time.fixedDeltaTime;
        float castDistance = moveDistance + castExtraDistance;

        Vector3 start = transform.position;
        Vector3 end = start + direction.normalized * castDistance * 2f;

        Gizmos.DrawLine(start, end);

        Gizmos.DrawWireSphere(start, sphereCastRadius);
        Gizmos.DrawWireSphere(end, sphereCastRadius);
    }

    private bool OnHit(RaycastHit hit)
    {
        //if (IsOwnerCollider(hit.collider))
        //    return true;

        ShieldReflector shield = hit.collider.GetComponentInParent<ShieldReflector>();

        if (shield != null)
        {
            HandleShieldHit(hit, shield);
            return true;
        }

        IDamageable target = hit.collider.GetComponentInParent<IDamageable>();

        if (target != null && target.Team != ownerTeam)
        {
            target.TakeDamage(damageData, attackerStats);
            PlayVFX(hit.point, arrowEffect.enemyHitEffect);
            DestroyProjectile();
            return true;
        }

        if (hit.collider.CompareTag("Wall"))
        {
            HandleRicochet(hit);
            PlayRandomSound(wallHitSoundEffects);

            PlayVFX(hit.point, arrowEffect.wallImpactEffect);

            return true;
        }

        // somnitelno no okay
        if (hit.collider.CompareTag("UpperBorder"))
        {
            PlayVFX(hit.point, arrowEffect.mysthicImpactEffect);
            DestroyProjectile();
        }

        return false;
    }

    private void HandleShieldHit(RaycastHit hit, ShieldReflector shield)
    {
        PlayRandomSound(shieldHitSoundEffects);

        PlayVFX(hit.point, arrowEffect.shieldImpactEffect);

        // This part is now useless
        if (!shield.CanReflect(ownerTeam))
        {
            // Inactive shield: arrow is blocked.
            DestroyProjectile();
            return;
        }

        ReflectFromShield(hit.normal, shield.GetOwner(), shield.GetOwnerTeam(),
            shield.GetOwnerStats());

        shield.NotifyProjectileReflected();
    }

    public void ReflectFromShield(
    Vector3 surfaceNormal,
    GameObject newOwner,
    Team newOwnerTeam,
    CoreStats newOwnerStats)
    {
        direction = Vector3.Reflect(direction, surfaceNormal).normalized;

        transform.forward = direction;

        owner = newOwner;
        ownerTeam = newOwnerTeam;
        Debug.Log("OWNER TEAM = " + newOwnerTeam);
        attackerStats = newOwnerStats;
        SetupOutline();

        transform.position += direction * 0.02f;
    }

    private void HandleRicochet(RaycastHit hit)
    {
        if (ricochetsLeft <= 0)
        {
            DestroyProjectile();
            return;
        }

        direction = Vector3.Reflect(direction, hit.normal).normalized;

        transform.forward = direction;

        // Move the arrow slightly away from the surface to prevent
        // immediately hitting the same collider again.
        transform.position = hit.point + direction * 0.01f;

        ricochetsLeft--;
    }

    private bool TryDamageTarget(Collider other)
    {
        if (!isInitialized)
        {
            return false;
        }

        IDamageable target =
            other.GetComponentInParent<IDamageable>();

        if (target == null)
        {
            return false;
        }

        // Do not damage allies.
        if (target.Team == ownerTeam)
        {
            return false;
        }

        target.TakeDamage(
            damageData,
            attackerStats);

        DestroyProjectile();

        return true;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (!isInitialized)
    //    {
    //        Debug.LogWarning(
    //            $"{name} was not initialized before hitting an object.");

    //        return;
    //    }

    //    if (IsOwnerCollider(other))
    //    {
    //        return;
    //    }

    //    TryDamageTarget(other);
    //}

    private void PlayRandomSound(AudioClip[] sounds)
    {
        if (sounds == null || sounds.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, sounds.Length);
        float randomVolume = Random.Range(0.5f, 0.7f);
        float randomPitch = Random.Range(0.9f, 1.1f);

        AudioManager.Play(sounds[randomIndex], randomVolume, randomPitch);
    }
    private bool IsOwnerCollider(Collider other)
    {
        if (owner == null)
        {
            return false;
        }

        if (other.gameObject == owner)
        {
            return true;
        }

        return other.transform.IsChildOf(owner.transform);
    }

    private IEnumerator DestroyArrow()
    {
        yield return new WaitForSeconds(lifeTime);

        DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        Destroy(gameObject);
    }

    void PlayVFX(Vector3 position, GameObject particle)
    {
        if (arrowEffect == null)
        {
            Debug.LogWarning("ArrowEffect scriptable object didnt set to an object");
            return;
        }
        if (particle != null)
        {
            Instantiate(particle, position, Quaternion.identity);
            // spawn in any case
            //Instantiate(arrowEffect.constHitEffect, position, Quaternion.identity);

        }
            
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
        if (ownerTeam == Team.Player)
        {
            outline.OutlineColor = owner.GetComponent<PlayerController>().GetColor();
        }
        else if (ownerTeam == Team.Enemy)
        {
            outline.OutlineColor = owner.GetComponent<EnemyController>().GetColor();
        }
        else {
            outline.OutlineColor = Color.white;
        }
            //outline.OutlineColor = outlineColor;
        outline.OutlineWidth = outlineWidth;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public float GetSpeed()
    {
        return speed;
    }

    public void SetMaxRicochetCount(int count)
    {
        maxRicochetCount = count;
        ricochetsLeft = count;
    }

    public int GetMaxRicochetCount()
    {
        return maxRicochetCount;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("UpperBorder"))
    //    {
    //        DestroyProjectile();
    //    }
    //}
}