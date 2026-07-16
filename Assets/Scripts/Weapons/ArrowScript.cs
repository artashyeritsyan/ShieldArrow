using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class ArrowScript : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] float lifeTime = 5f;
    [SerializeField] float speed = 4f;
    // ricochet count means it will ricochet N times and will be destroyed on N+1 
    [SerializeField] int maxRicochetCount = 2;

    [SerializeField] bool isEnemyArrow = false;
    [SerializeField] bool showOutline = false;
    [SerializeField] Color outlineColor = Color.white;
    [SerializeField] float outlineWidth = 1.0f;

    private float ricochetsLeft;
    private Vector3 direction;
    [SerializeField] LayerMask hitMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        direction = transform.forward;
        StartCoroutine(DestroyArrow());
        ricochetsLeft = maxRicochetCount;

        if (showOutline)
        {
            if (gameObject.GetComponent<Outline>() != null)
            {
                gameObject.GetComponent<Outline>().enabled = true;
            }
            else
            {
                Outline outline = gameObject.AddComponent<Outline>();
                outline.enabled = true;
                gameObject.GetComponent<Outline>().OutlineColor = outlineColor;
            }
            gameObject.GetComponent<Outline>().OutlineWidth = outlineWidth;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float moveDist = speed * Time.deltaTime;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, moveDist, hitMask))
        {
            // HIT something
            OnHit(hit);
        }
        else
        {
            transform.position += direction * moveDist;
        }
    }


    void OnHit(RaycastHit hit)
    {
        if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Shield"))
        {
            if (ricochetsLeft > 0)
            {
                direction = Vector3.Reflect(direction, hit.normal);
                transform.forward = direction;
                transform.position = hit.point + direction * 0.01f;

                ricochetsLeft--;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else if (hit.collider.CompareTag("Enemy"))
        {
            //hit.collider.gameObject
            //ReturnToPool();
        }
        else {
            return;
        }
    }

    IEnumerator DestroyArrow()
    {
        yield return new WaitForSeconds(lifeTime);

        Destroy(gameObject);
    }

    void resetRicochets()
    {
        ricochetsLeft = maxRicochetCount;
    }
    bool turned = false;
    void Ricochet(Collision collision)
    {
        if (ricochetsLeft > 0)
        {
            Vector3 normal = collision.contacts[0].normal;
            Vector3 newDir = Vector3.Reflect(transform.forward, normal);
            transform.forward = newDir;

            ricochetsLeft--;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void ResetTurn() => turned = false;

    public void SetSpeed(float speed) { this.speed = speed; }
    public float GetSpeed() { return speed; }

    public void SetMaxRicochetCount(int count) { maxRicochetCount = count; }
    public int GetMaxRicochetCount() { return maxRicochetCount; }

    void OnTriggerEnter(Collider other)
    {
        //if (other.CompareTag("Enemy"))
        //{
        //    // damage logic
        //    Destroy(gameObject);
        //}
    }
    //void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Wall"))
    //    {
    //        Ricochet(collision);
    //    }
    //    if (collision.gameObject.CompareTag("Shield"))
    //    {
    //        Debug.Log("Shield");
    //        Ricochet(collision);
    //    }
    //}
}
