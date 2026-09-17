using UnityEngine;

public class ShieldReflector : MonoBehaviour
{
    [SerializeField] private ShieldController shieldController;


    // Currently it can reflect in every case, except the same team arrow 
    public bool CanReflect(Team projectileTeam)
    {
        if (projectileTeam == shieldController.GetOwnerTeam())
            return true;

        if (shieldController == null)
            return false;

        if (shieldController.isEnemyShield)
            return true;

        // THIS LINE FOR REFLECTION 

        //if (!shieldController.GetIsAttacking())
        //    return false;

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