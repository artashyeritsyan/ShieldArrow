using UnityEngine;
using System.Collections;

public class ArrowScript : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] float lifeTime = 5f;
    [SerializeField] float arrowSpeed = 4f;
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

    public void SetSpeed(float speed) { arrowSpeed = speed; }
    public float GetSpeed() { return arrowSpeed; }

    public void SetMaxRicochetCount (int count) { maxRicochetCount = count; }
    public int GetMaxRicochetCount() { return maxRicochetCount; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Trigger!");
            transform.Rotate(0f, -90f, 0f);
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    //if (collision.gameObject != gameObject)
    //    //{
    //    //    Debug.Log("Collide!");
    //    //}
    //    if (collision.gameObject.CompareTag("Wall"))
    //    {

    //        transform.rotation = Quaternion.Euler(transform.localRotation.x, transform.localRotation.y - 90f, transform.localRotation.z);
    //        Debug.Log("Ricochet");
    //    }
    //        transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y - 90f, transform.rotation.z);
    //}
}
