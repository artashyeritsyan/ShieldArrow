using UnityEngine;

public class ArcherController : PlayerController
{
    [Header("Weapon")]
    private WeaponScript weaponScript;

    private void Start()
    {
        InitialiseWeapon();
    }

    private void InitialiseWeapon()
    {
        weaponScript = transform.GetComponentInChildren<WeaponScript>();
        weapon = weaponScript.gameObject;
    }

    public override void Attack()
    {
        weaponScript.Shoot();
    }
}
