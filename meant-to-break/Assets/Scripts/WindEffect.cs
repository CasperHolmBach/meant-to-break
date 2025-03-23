using UnityEngine;

public class WindEffect : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float lifetime;
    private float timer;
    
    public void Initialize(Vector3 direction, float speed, float lifetime)
    {
        this.direction = direction;
        this.speed = speed;
        this.lifetime = lifetime;
        timer = 0f;
    }
    
    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}