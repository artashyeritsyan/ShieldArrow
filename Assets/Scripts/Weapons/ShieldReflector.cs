using UnityEngine;

public class ShieldReflector : MonoBehaviour
{
    [SerializeField] private ShieldController shieldController;

    public bool CanReflect(Team projectileTeam)
    {
        if (shieldController == null)
            return false;

        if (!shieldController.GetIsAttacking())
            return false;

        return projectileTeam != shieldController.GetOwnerTeam();
    }

    public GameObject GetOwner()
    {
        return shieldController.GetOwner();
    }

    public Team GetOwnerTeam()
    {
        return shieldController.GetOwnerTeam();
    }

    public CoreStats GetOwnerStats()
    {
        return shieldController.GetOwnerStats();
    }

    public void NotifyProjectileReflected()
    {
        shieldController.NotifyProjectileReflected();
    }
}