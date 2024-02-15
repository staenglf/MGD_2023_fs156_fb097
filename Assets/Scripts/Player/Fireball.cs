using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    [SerializeField] float damage;
    [SerializeField] float hitForce;
    [SerializeField] int speed;
    [SerializeField] float lifetime = 1;
    
    // Destroys after lifetime
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // Moves the fireball by every update
    private void FixedUpdate()
    {
        transform.position += speed * transform.right;
    }
    
    // Detect a hit of the spell
    private void OnTriggerEnter2D(Collider2D _other)
    {
        if(_other.tag == "Enemy")
        {
            _other.GetComponent<Enemy>().EnemyHit(damage, (_other.transform.position - transform.position).normalized, -hitForce);
        }
    }
}