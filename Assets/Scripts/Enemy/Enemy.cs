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

    // Creates the objects by start
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerController.Instance;
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
    
    // Enemy gets a hit by player
    protected void OnCollisionStay2D(Collision2D _other)
    {
        if(_other.gameObject.CompareTag("Player") && !PlayerController.Instance.pState.invincible)
        {
            Attack();
            PlayerController.Instance.HitStopTime(0, 5, 0.5f);
        }
    }

    // Player gets a hit by enemy
    protected virtual void Attack()
    {
        PlayerController.Instance.TakeDamage(damage);
    }
}
