using UnityEngine;

public class Rocket : MonoBehaviour
{
    public float speed = 20f;
    public GameObject explosionEffect;
    
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    void Explode()
    {
        // Spawn explosion effect
        if (explosionEffect)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
