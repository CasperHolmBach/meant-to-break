using UnityEngine;

public class Rocket : MonoBehaviour
{
    public float speed = 20f;
    public ParticleSystem explosionEffect;
    
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
        if (explosionEffect != null)
        {
            ParticleSystem explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            explosion.Play();
            Destroy(explosion.gameObject, explosion.main.duration);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            CharacterController controller = player.GetComponent<CharacterController>();

            if (controller != null)
            {
                Vector3 knockbackDir = (player.transform.position - transform.position).normalized;
                float distance = Vector3.Distance(player.transform.position, transform.position);

                float knockbackForce = Mathf.Clamp(50f / distance, 5f, 25f);
                player.GetComponent<FPSController>().ApplyKnockback(knockbackDir * knockbackForce);
            }
        }
        Destroy(gameObject);
    }
}
