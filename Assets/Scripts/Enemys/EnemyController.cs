using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float shootingSpeed = 1f; 
    [SerializeField] WeaponScript wp;

    float shootCooldown;

    void Update()
    {
        var target = GetClosestPlayer();
        if (target == null)
            return;

        RotateTowards(target.transform.position);
        HandleShooting();
    }

    PlayerController GetClosestPlayer()
    {
        float minDist = float.MaxValue;
        PlayerController closest = null;

        foreach (var player in PlayerManager.Instance.players)
        {
            if (player == null) continue;

            float dist = Vector3.Distance(transform.position, player.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                closest = player;
            }
        }

        return closest;
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

    void HandleShooting()
    {
        shootCooldown -= Time.deltaTime;

        if (shootCooldown <= 0f)
        {
            wp.Shoot();
            shootCooldown = 1f / shootingSpeed;
        }
    }
}