using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float health;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilFactor;
    [SerializeField] protected bool isRecoiling = false;
    [SerializeField] protected PlayerController player;
    [SerializeField] protected float speed;
    [SerializeField] protected float damage;
    protected float recoilTimer;
    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerController.Instance;
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(health <= 0)
        {
            Destroy(gameObject);
        }

        if(isRecoiling)
        {
            if(recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0;
            }
        }
    }

    //Sets recoil in the direction that the hit comes from and zeros the health
    public virtual void EnemyHit(float _damageDone, Vector2 _hitDirection, float _hitForce)
    {
        health -= _damageDone;

        if(!isRecoiling)
        {
            rb.AddForce(-_hitForce * recoilFactor * _hitDirection);
        }
    }

    protected void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !PlayerController.Instance.pState.invincible)
        {
            Attack();
        }
    }

    protected virtual void Attack()
    {
        PlayerController.Instance.TakeDamage(damage);
    }
}
