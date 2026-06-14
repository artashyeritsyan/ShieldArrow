using System.Collections;
using Unity.Jobs;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class WeaponScript : MonoBehaviour
{
    const float SECOND = 1f;

    [Header("Weapon")]
    [SerializeField] Transform shootPoint;
    [SerializeField] GameObject bullet;

    [Header("Parameters")]
    [SerializeField] float attackSpeed = 2f;
    private bool canShoot;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canShoot = true;

    }

    public void Shoot()
    {
        if (canShoot)
        {
            Instantiate(bullet, shootPoint.position, shootPoint.rotation);
            StartCoroutine(ShootCooldown());
        }
    }

    IEnumerator ShootCooldown()
    {
        canShoot = false;

        yield return new WaitForSeconds(SECOND / Mathf.Max(attackSpeed, 0.01f));

        //Debug.Log(SECOND / stats.core.attackSpeed);
        canShoot = true;
    }


    public float GetAttackSpeed()
    {
        return attackSpeed;
    }

    public void SetAttackSpeeed(float newAttackSpeed)
    {
        attackSpeed = newAttackSpeed;
    }

    public bool GetCanShoot()     {
        return canShoot;
    }   

    private void OnEnable()
    {
        canShoot = true;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

}
