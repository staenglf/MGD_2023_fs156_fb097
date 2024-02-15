using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement Settings")]
    [SerializeField] private float walkSpeed = 1;
    [Space(5)]
    
    [Header("Vertical Movement Settings")]
    [SerializeField] private float jumpForce = 45;
    private int jumpBufferCounter = 0;
    [SerializeField] private int jumpBufferFrames;
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime;
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps;
    [Space(5)]
    
    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;
    [Space(5)]
    
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;
    [SerializeField] GameObject dashEffect;
    private bool canDash = true;
    private bool dashed;
    [Space(5)]

    [Header("Attack Settings")]
    bool attack = false;
    float timeBetweenAttack, timeSinceAttack;
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] float damage; // the damage the player does to an enemy
    
    bool restoreTime;
    float restoreTimeSpeed;
    
    [Space(5)] 

    [Header("Recoil")]
    [SerializeField] int recoilXSteps = 5;
    [SerializeField] int recoilYSteps = 5;
    [SerializeField] float recoilXSpeed = 100;
    [SerializeField] float recoilYSpeed = 100;
    int stepsXRecoiled, stepsYRecoiled;
    [Space(5)]
    
    [Header("Health Settings")]
    public int health;
    public int maxHealth;
    [SerializeField] GameObject bloodSpurt;
    [SerializeField] float hitFlashSpeed;
    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate onHealthChangedCallback;
    
    float healTimer;
    [SerializeField] float timeToHeal;
    [Space(5)]
    
    [Header("Mana Settings")]
    [SerializeField] UnityEngine.UI.Image manaStorage;

    [SerializeField] float mana;
    [SerializeField] float manaDrainSpeed;
    [SerializeField] float manaGain;
    [Space(5)]
    
    [Header("Spell Settings")]
    // stats for spell
    [SerializeField] float manaSpellCost = 0.3f;
    [SerializeField] float timeBetweenCast = 0.5f;
    [SerializeField] float spellDamage; // for upspellexplosion and downspellfireball
    [SerializeField] float downSpellForce; // for desolate dive only
    
    // the objects cast by the spell
    [SerializeField] GameObject sideSpellFireball;
    [SerializeField] GameObject upSpellExplosion;
    [SerializeField] GameObject downSpellFireball;
    float timeSinceCast;
    float castOrHealTimer;
    [Space(5)]
    
    
    // Mobile Touch Inputs
    
    
    [HideInInspector] public PlayerStateList pState;
    Animator anim;
    
    
    //Input System
    InputSystem controls;
    
    bool jumpbool = false;
    // bool movementright = false;
    bool attackbool = false;
    bool castbool = false;
    bool dashbool = false;
    
    
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float xAxis, yAxis;
    private float gravity;
    public static PlayerController Instance;

    //Draws red lines out the attack area
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }

    //Checks if a GameObject exists by start
    private void Awake()
    {
        controls = new InputSystem();
        controls.Enable();
        
        
        // Touch Input System Bools and Calling Functions
        controls.Land.Jump.performed += ctx =>
        {
            jumpbool = true;
        };
        
        controls.Land.Attack.performed += ctx =>
        {
            attackbool = true;
            Attack();
        };
        
        controls.Land.Dash.performed += ctx =>
        {
            dashbool = true;
            StartDash();
        };
        
        controls.Land.Cast.performed += ctx =>
        {
            castbool = true;
            CastSpell();
        };
        
        controls.Land.Movement.performed += ctx =>
        {
            xAxis = ctx.ReadValue<float>();
            yAxis = ctx.ReadValue<float>();
            Debug.Log("direction" + xAxis);
        };
        
        
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        //DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        pState = GetComponent<PlayerStateList>();
        
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        
        anim = GetComponent<Animator>();

        gravity = rb.gravityScale;

        Mana = mana;
        manaStorage.fillAmount = Mana;

        Health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (pState.cutscene) return;
        GetInputs();
        UpdateJumpVariables();
        RestoreTimeScale();
        
        if (pState.dashing) return;
        Flip();
        Move();
        Jump();
        StartDash();
        Attack();
        
        // FlashWhileInvincible();
        Heal();
        CastSpell();


        if (health == 0)
        {
            SceneManager.LoadScene(4);
        }
    }
    
    // Only for the upwards and downwards spell cast
    private void OnTriggerEnter2D(Collider2D _other)
    {
        if(_other.GetComponent<Enemy>() != null && pState.casting)
        {
            _other.GetComponent<Enemy>().EnemyHit(spellDamage, (_other.transform.position - transform.position).normalized, -recoilYSpeed);
        }
    }


    // When game is paused player wont be able to dash or recoil
    void FixedUpdate()
    {
        if (pState.cutscene) return;
        if (pState.dashing) return;
        Recoil();
    }

    //Sets the input buttons
    void GetInputs()
    {
        // xAxis = Input.GetAxisRaw("Horizontal");
        // yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetButtonDown("Attack");

        if(castbool)
        {
            castOrHealTimer += Time.deltaTime;
        } 
        else
        {
            castOrHealTimer = 0;
        }
    }

    //Turns the Player Character by 180 degrees on Y Axis if the direction is changed
    void Flip()
    {
        if(xAxis < 0)
        {
            transform.localScale = new Vector2(-5, transform.localScale.y);
            pState.lookingRight = false;
        }
        else if(xAxis > 0) 
        {
            transform.localScale = new Vector2(5, transform.localScale.y);
            pState.lookingRight = true;
        }
    }

    //Set the walking speed
    private void Move()
    {
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y); 
        anim.SetBool("Walking", rb.velocity.x != 0 && Grounded());
            
        if (controls.Land.Movement.WasReleasedThisFrame())
        {
            xAxis = 0;
            yAxis = 0;
        }
    }

    //Checks if a Dash is made on ground
    void StartDash()
    {
        if (dashbool && canDash && !dashed)
        {
            StartCoroutine(Dash());
            dashed = true;
            dashbool = false;
        }

        if (Grounded())
        {
            dashed = false;
        }
    }
    
    //Executes the Dash animation
    IEnumerator Dash()
    {
        canDash = false;
        pState.dashing = true;
        anim.SetTrigger("Dashing");
        rb.gravityScale = 0;
        int _dir = pState.lookingRight ? 1 : -1;
        rb.velocity = new Vector2(_dir * dashSpeed, 0);
        
        if (Grounded()) Instantiate(dashEffect, transform);
        yield return new WaitForSeconds(dashTime);
        
        rb.gravityScale = gravity;
        pState.dashing = false;
        
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public IEnumerator WalkIntoNewScene(Vector2 _exitDir, float _delay)
    {
        if(_exitDir.y > 0)
        {
            rb.velocity = jumpForce * _exitDir;
        }

        if(_exitDir.x != 0)
        {
            xAxis = _exitDir.x > 0 ? 1 : -1;
            Move();
        }

        Flip();
        yield return new WaitForSeconds(_delay);
        pState.cutscene = false;
    }

    //Executes the Attack Animation
    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if (attackbool && timeSinceAttack >= timeBetweenAttack) 
        {
            timeSinceAttack = 0;
            anim.SetTrigger("Attacking");
            attackbool = false;
            if(yAxis == 0 || yAxis < 0 && Grounded())
            {
                Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, recoilXSpeed);
            }
            else if(yAxis > 0)
            {
                Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, recoilYSpeed);
            }
            else if(yAxis < 0 && !Grounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, recoilYSpeed);
            }
        }
    }

    //Checks if a attackable object is in the attack area
    private void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
    {
        Collider2D[] objectToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);
       
        if (objectToHit.Length > 0)
        {
            _recoilDir = true;
        }

        for (int i = 0; i < objectToHit.Length; i++)
        {
            if (objectToHit[i].GetComponent<Enemy>() != null)
            {
                objectToHit[i].GetComponent<Enemy>().EnemyHit(damage, (transform.position - objectToHit[i].transform.position).normalized, _recoilStrength);
                
                if (objectToHit[i].CompareTag("Enemy"))
                {
                    Mana += manaGain;
                }
            }
        }
    }

    //Sets a recoil for the Player
    void Recoil()
    {
        if (pState.recoilingX)
        {
            if (pState.lookingRight)
            {
                rb.velocity = new Vector2(-recoilXSpeed, 0);
            }
            else
            {
                rb.velocity = new Vector2(recoilXSpeed, 0);
            }
        }

        if (pState.recoilingY)
        {
            rb.gravityScale = 0;
            if (yAxis < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
            }
            airJumpCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }

        //Stop the Recoil
        if(pState.recoilingX && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }

        if (pState.recoilingY && stepsYRecoiled < recoilYSteps)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }

        if(Grounded()) 
        {
            StopRecoilY();
        }
    }

    //Stops Recoil on X-Axis
    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoilingX = false;
    }

    //Stops Recoil on Y-Axis
    void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilingY = false;
    }

    //Reduces Health by hit
    public void TakeDamage(float _damage)
    {
        Health -= Mathf.RoundToInt(_damage);
        StartCoroutine(StopTakingDamage());
    }

    IEnumerator StopTakingDamage()
    {
        pState.invincible = true;
        GameObject _bloodSpurtParticles = Instantiate(bloodSpurt, transform.position, Quaternion.identity);
        Destroy(_bloodSpurtParticles, 1.5f);
        anim.SetTrigger("TakeDamage");
        yield return new WaitForSeconds(1f);
        pState.invincible = false;
    }

    void FlashWhileInvincible()
    {
        sr.material.color = pState.invincible
            ? Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time * hitFlashSpeed, 1.0f))
            : Color.white;

    }
    
    
    void RestoreTimeScale()
    {
        if (restoreTime)
        {
            if (Time.timeScale < 1)
            {
                Time.timeScale += Time.deltaTime * restoreTimeSpeed;
            }
            else
            {
                Time.timeScale = 1;
                restoreTime = false;
            }
        }
    }
    
    public void HitStopTime(float _newTimeScale, int _restoreSpeed, float _delay)
    {
        restoreTimeSpeed = _restoreSpeed;
        Time.timeScale = _newTimeScale;
        if (_delay > 0)
        {
            StopCoroutine(StartTimeAgain(_delay));
            StartCoroutine(StartTimeAgain(_delay));
        }
        else
        {
            restoreTime = true;
        }
    }

    IEnumerator StartTimeAgain(float _delay)
    {
        restoreTime = true;
        yield return new WaitForSeconds(_delay);
    }
    
    
    
    

    //Sets the health of the player
    public int Health
    {
        get { return health; }
        set
        {
            if (Health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);

                if (onHealthChangedCallback != null)
                {
                    onHealthChangedCallback.Invoke();
                }
            }
        }
    }
    
    void Heal()
    {
        if (Input.GetButton("Cast/Heal") && castOrHealTimer > 0.05f && Health < maxHealth && Mana > 0 && !pState.jumping && !pState.dashing)
        {
            pState.healing = true;
            anim.SetBool("Healing", true);

            // Healing
            healTimer += Time.deltaTime;
            if (healTimer >= timeToHeal)
            {
                Health++;
                healTimer = 0;
            }

            // Drain mana while Healing
            Mana -= Time.deltaTime * manaDrainSpeed;
        }
        else
        {
            pState.healing = false;
            anim.SetBool("Healing", false);
            healTimer = 0;
        }
    }
    
    // Mana is used for Healing and Attack Spellcasting
    float Mana
    {
        get { return mana; }
        set
        {
            //if mana stats change
            if (mana != value)
            {
                mana = Mathf.Clamp(value, 0, 1);
                manaStorage.fillAmount = Mana;
            }
        }
    }
    
    
    void CastSpell()
    {
        if (castbool && castOrHealTimer <= 0.05f && timeSinceCast >= timeBetweenCast && Mana >= manaSpellCost)
        {
            pState.casting = true;
            timeSinceCast = 0;
            castbool = false;
            StartCoroutine(CastCoroutine());
        }
        else
        {
            timeSinceCast += Time.deltaTime;
        }
        
        if (!castbool)
        {
            castOrHealTimer = 0;
        }
        

        if(Grounded())
        {
            // Disabling the downspell if player is on the ground
            downSpellFireball.SetActive(false);
        }
        // If down spell is active, force player down until he is grounded
        if(downSpellFireball.activeInHierarchy)
        {
            rb.velocity += downSpellForce * Vector2.down;
        }
    }
    
    
    
    IEnumerator CastCoroutine()
    {
        anim.SetBool("Casting", true);
        
        // value set here depends on animation can be adjusted
        yield return new WaitForSeconds(0.15f);

        //side cast
        if (yAxis == 0 || (yAxis < 0 && Grounded()))
        {
            GameObject _fireBall = Instantiate(sideSpellFireball, SideAttackTransform.position, Quaternion.identity);

            //flip the fireball
            if(pState.lookingRight)
            {
                // if facing right, fireball goes right
                _fireBall.transform.eulerAngles = Vector3.zero;
            }
            else
            {
                //if not facing right, rotate the fireball 180 degrees
                _fireBall.transform.eulerAngles = new Vector2(_fireBall.transform.eulerAngles.x, 180); 
            }
            pState.recoilingX = true;
        }

        // upwards spell
        else if( yAxis > 0)
        {
            Instantiate(upSpellExplosion, transform);
            rb.velocity = Vector2.zero;
        }

        // downwards spell
        else if(yAxis < 0 && !Grounded())
        {
            downSpellFireball.SetActive(true);
        }

        Mana -= manaSpellCost;
        
        yield return new WaitForSeconds(0.35f); //time from cast to end of animation
        anim.SetBool("Casting", false);
        pState.casting = false;
    }
    
    
    //Checks if a player stands on a ground
    public bool Grounded()
    {
        if(Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround) || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround) || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Executes the Jump and Double Jump Mechanic
    void Jump()
    {
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && !pState.jumping)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce);
            pState.jumping = true;
        }
        
        if (!Grounded() && airJumpCounter < maxAirJumps && jumpbool)
        {
            pState.jumping = true;
            airJumpCounter++;
            rb.velocity = new Vector3(rb.velocity.x, jumpForce);
        }
        
        /*
        if (jumpbool && pState.jumping && rb.velocity.y > 3)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            pState.jumping = false;
            Debug.Log("Jump3");
        }
        */
        
        
        if (jumpbool)
        {
            anim.SetBool("Jumping", !Grounded());
            Debug.Log("Jump3" + jumpbool);
            jumpbool = false;
        }
        
        
    }

    //Overwrites the Jump Function by different executions
    private void UpdateJumpVariables()
    {
        if (Grounded())
        {
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (jumpbool)
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter--;
        }
    }
    private void OnEnable()
    {
        controls.Enable();
    }
    
    private void OnDisable()
    {
        controls.Disable();
    }
    
}
