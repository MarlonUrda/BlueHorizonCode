using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float idleTimeMin = 1f;
    public float idleTimeMax = 3f;
    public float walkTimeMin = 2f;
    public float walkTimeMax = 5f;
    public float damageCooldown = 1f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public Collider2D headCollider;
    public Collider2D bodyCollider;
    public float bounceForce = 10f;

    private string _currentAnimation;

    const string IDLE = "MortIdle";
    const string WALK = "MortWalk";
    const string HURT = "MortHurt";

    public float attackCooldown;
    private bool canAttack = true;

    private bool movingRight = true;
    private bool hasTakenDamage = false;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isDead = false;

    public AudioManager _audioManager;
    public AudioClip _audioClip;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        StartCoroutine(MoveRoutine());
    }

    private void Update()
    {
        if (!isDead && (IsHittingWall() || !IsGrounded()))
        {
            Flip();
        }
    }

    private IEnumerator MoveRoutine()
    {
        while (!isDead)
        {
            float moveTime = Random.Range(walkTimeMin, walkTimeMax);
            float idleTime = Random.Range(idleTimeMin, idleTimeMax);

            // Move
            ChangeAnimation(WALK);
            for (float t = 0; t < moveTime; t += Time.deltaTime)
            {
                if (!isDead && (IsHittingWall() || !IsGrounded()))
                {
                    Flip();
                    break;
                }

                rb.velocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.velocity.y);
                yield return null;
            }

            // Idle
            ChangeAnimation(IDLE);
            rb.velocity = Vector2.zero;
            yield return new WaitForSeconds(idleTime);
        }
    }

    private bool IsHittingWall()
    {
        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
        Vector2 rayOrigin = movingRight ?
            new Vector2(transform.position.x + 0.5f, transform.position.y) :
            new Vector2(transform.position.x - 0.5f, transform.position.y);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, 0.1f, groundLayer);
        return hit.collider != null;
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.15f, groundLayer);
    }

    private void Flip()
    {
        movingRight = !movingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Verifica si el collider del enemigo que fue golpeado es el de la cabeza
            if (headCollider.bounds.Intersects(collision.bounds))
            {
                collision.GetComponentInParent<PlayerMovement>().ApplyForce();
                StartCoroutine(Die());
            }
            // Verifica si el collider del enemigo que fue golpeado es el del cuerpo
            else if (bodyCollider.bounds.Intersects(collision.bounds) && !hasTakenDamage)
            {
                if (!canAttack) return;

                canAttack = false;
                if (GameManager.Instance.lives > 0)
                {
                    collision.GetComponentInParent<PlayerMovement>().hitByEnemy();
                    hasTakenDamage = true;
                    GameManager.Instance.RemoveLife();
                    Debug.Log(GameManager.Instance.lives);
                    StartCoroutine(ResetDamageFlag());
                }

                if (GameManager.Instance.lives <= 0)
                {
                    collision.GetComponentInParent<PlayerMovement>().End();
                } 

                Invoke("RechargeAttack", attackCooldown);
            }
        }
    }

    private IEnumerator ResetDamageFlag()
    {
        yield return new WaitForSeconds(damageCooldown);
        hasTakenDamage = false;
    }

    private IEnumerator Die()
    {
        rb.velocity = Vector2.zero;
        isDead = true;
        _audioManager.PlaySound(_audioClip);
        ChangeAnimation(HURT);

        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

        Destroy(gameObject);
    }

    private void RechargeAttack()
    {
        canAttack = true;
    }

    private void ChangeAnimation(string newAnimation)
    {
        if(_currentAnimation == newAnimation) return;

        anim.Play(newAnimation);
        _currentAnimation = newAnimation;
    }
}
