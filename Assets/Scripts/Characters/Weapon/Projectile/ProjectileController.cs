using UnityEngine;
using UnityEngine.Rendering;

public class ProjectileController : MonoBehaviour
{
    [Header("Projectile Properties")]
    public float dmgCoef = 1.0f;
    private int finalDamage;
    private Rigidbody2D rb;
    private ProjectilePoolManager poolManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        poolManager = FindObjectOfType<ProjectilePoolManager>(); // You might want to use a more efficient method to reference the pool manager
    }

    public void Initialize(CharacterController firingCharacter)
    {
        finalDamage = Mathf.RoundToInt(firingCharacter.Atk * dmgCoef);
    }

    public void Launch(Vector2 direction, float speed)
    {
        rb.velocity = direction.normalized * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var target = collision.collider.GetComponent<CharacterController>();
        if (target != null)
        {
            target.takenDamage(finalDamage);
        }

        poolManager.ReturnProjectileController(this);
    }

}
