using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class ArrowScript : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] float lifeTime = 5f;
    [SerializeField] float arrowSpeed = 4f;
    // ricochet count means it will ricochet N times and will be destroyed on N+1 
    [SerializeField] int maxRicochetCount = 4;
    private float ricochetsLeft;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(DestroyArrow());

        ricochetsLeft = maxRicochetCount;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * arrowSpeed);
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

    public void SetSpeed(float speed) { arrowSpeed = speed; }
    public float GetSpeed() { return arrowSpeed; }

    public void SetMaxRicochetCount (int count) { maxRicochetCount = count; }
    public int GetMaxRicochetCount() { return maxRicochetCount; }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // damage logic
            Destroy(gameObject);
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Ricochet(collision);
        }
    }
}
