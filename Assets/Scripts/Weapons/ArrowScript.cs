using UnityEngine;
using System.Collections;

public class ArrowScript : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float speed = 4f;

    // Ricochet count means it ricochets N times
    // and is destroyed on collision N+1.
    [SerializeField] private int maxRicochetCount = 2;

    [SerializeField] private LayerMask hitMask;

    [Header("Visuals")]
    [SerializeField] private bool showOutline;
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlineWidth = 1f;

    private int ricochetsLeft; // CHANGED: float -> int
    private Vector3 direction;

    private GameObject owner; // ADDED
    private Team ownerTeam; // ADDED
    private DamageData damageData; // ADDED
    private CoreStats attackerStats; 
    private bool isInitialized;

    private Coroutine lifetimeCoroutine; // ADDED

    private void Start()
    {
        SetupProjectile(); // CHANGED
    }

    private void SetupProjectile() // ADDED
    {
        direction = transform.forward.normalized; // CHANGED
        ricochetsLeft = maxRicochetCount;

        SetupOutline(); // ADDED

        if (lifetimeCoroutine != null) // ADDED
        {
            StopCoroutine(lifetimeCoroutine); // ADDED
        }

        lifetimeCoroutine = StartCoroutine(DestroyArrow()); // CHANGED
    }

    public void Initialize( // ADDED
        GameObject projectileOwner, // ADDED
        Team projectileOwnerTeam, // ADDED
        DamageData projectileDamage, // ADDED
        CoreStats sourceStats) // ADDED
    {
        owner = projectileOwner; // ADDED
        ownerTeam = projectileOwnerTeam; // ADDED
        damageData = projectileDamage; // ADDED
        attackerStats = sourceStats; // ADDED
        isInitialized = true; // ADDED

        direction = transform.forward.normalized; // ADDED
        ricochetsLeft = maxRicochetCount; // ADDED
    }

    private void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        float moveDistance = speed * Time.deltaTime;

        if (Physics.Raycast(
                transform.position,
                direction,
                out RaycastHit hit,
                moveDistance,
                hitMask,
                QueryTriggerInteraction.Ignore))
        {
            if (!OnHit(hit))
            {
                transform.position += direction * moveDistance;
            }
        }
        else
        {
            transform.position += direction * moveDistance;
        }
    }

    private bool OnHit(RaycastHit hit)
    {
        if (IsOwnerCollider(hit.collider))
            return true;

        ShieldReflector shield =
            hit.collider.GetComponentInParent<ShieldReflector>();

        if (shield != null)
        {
            HandleShieldHit(hit, shield);
            return true;
        }

        IDamageable target =
            hit.collider.GetComponentInParent<IDamageable>();

        if (target != null && target.Team != ownerTeam)
        {
            target.TakeDamage(
                damageData,
                attackerStats);

            DestroyProjectile();
            return true;
        }

        if (hit.collider.CompareTag("Wall"))
        {
            HandleRicochet(hit);
            return true;
        }

        return false;
    }

    private void HandleShieldHit(
    RaycastHit hit,
    ShieldReflector shield)
    {
        if (!shield.CanReflect(ownerTeam))
        {
            // Inactive shield: arrow is blocked.
            DestroyProjectile();
            return;
        }

        ReflectFromShield(
            hit.normal,
            shield.GetOwner(),
            shield.GetOwnerTeam(),
            shield.GetOwnerStats());

        shield.NotifyProjectileReflected();
    }

    public void ReflectFromShield(
    Vector3 surfaceNormal,
    GameObject newOwner,
    Team newOwnerTeam,
    CoreStats newOwnerStats)
    {
        direction = Vector3
            .Reflect(direction, surfaceNormal)
            .normalized;

        transform.forward = direction;

        owner = newOwner;
        ownerTeam = newOwnerTeam;
        attackerStats = newOwnerStats;

        transform.position += direction * 0.02f;
    }



    private void HandleRicochet(RaycastHit hit) // ADDED
    {
        if (ricochetsLeft <= 0)
        {
            DestroyProjectile(); // CHANGED
            return;
        }

        direction = Vector3
            .Reflect(direction, hit.normal)
            .normalized; // CHANGED

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

    private bool IsOwnerCollider(Collider other) // ADDED
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

        DestroyProjectile(); // CHANGED
    }

    private void DestroyProjectile() // ADDED
    {
        if (lifetimeCoroutine != null) // ADDED
        {
            StopCoroutine(lifetimeCoroutine); // ADDED
            lifetimeCoroutine = null; // ADDED
        }

        Destroy(gameObject);
    }

    private void SetupOutline() // ADDED
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
        ricochetsLeft = count; // ADDED
    }

    public int GetMaxRicochetCount()
    {
        return maxRicochetCount;
    }
}