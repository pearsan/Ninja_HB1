using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : Character
{
    [FormerlySerializedAs("_rb")] [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;
    private bool isJumping = false;
    private bool isAttack;
    private bool isDead = false;
    
    private float horizontal;

    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;

    private int coin = 0;
    private Vector3 savePoint;

    [SerializeField] private Kunai kunaiPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private GameObject attackArea;

    private void Awake()
    {
        coin = PlayerPrefs.GetInt("coin", 0);
    }
    private void Update()
    {
        if (isDead)
            return;
        isGrounded = CheckGrounded();

        // horizontal = Input.GetAxisRaw("Horizontal");
        
        if (isAttack)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        if (isGrounded)
        {
            if (isJumping)
            {
                return;
            }
            if (Input.GetKeyDown(KeyCode.W) && isGrounded)
            {
                Jump();
            }
            if (Math.Abs(horizontal) > 0.1f)
            {
                ChangeAnim("run");
            }

            if (Input.GetKeyDown(KeyCode.Z) && isGrounded)
            {
                Attack();
            }

            if (Input.GetKeyDown(KeyCode.X) && isGrounded)
            {
                Throw();
            }
        }
        if (!isGrounded && rb.velocity.y < 0)
        {
            ChangeAnim("fall");
            isJumping = false;
        }
        if (Math.Abs(horizontal) > 0.1f)
        {
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
            transform.rotation = Quaternion.Euler(new Vector3(0, horizontal > 0 ? 0 : 180, 0));
        }
        else if (isGrounded)
        {
            ChangeAnim("idle");
            rb.velocity = Vector2.zero;
        }
    }

    public override void OnInit()
    {
        SavePoint();
        base.OnInit();
        isDead = false;
        isAttack = false;

        transform.position = savePoint;
        ChangeAnim("idle");
        DeactiveAttack();
        
        UIManager.Instance.SetCoin(coin);
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        OnInit();
    }

    protected override void OnDeath()
    {
        base.OnDeath();
    }

    private bool CheckGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, groundLayer);
        if (hit.collider != null)
        {
            Debug.DrawLine(transform.position, hit.point, Color.red);
        }
        // if (hit.collider != null)
        // {
        //     return true;
        // }
        // else
        // {
        //     return false;
        // }
        return hit.collider != null;
    }

    public void Attack()
    {

        ChangeAnim("attack");
        isAttack = true;
        Invoke(nameof(ResetAttack), 0.5f);
        ActiveAttack();
        Invoke(nameof(DeactiveAttack), 0.5f);
    }

    public void Throw()
    {
        ChangeAnim("throw");
        isAttack = true;
        Invoke(nameof(ResetAttack), 0.5f);

        Instantiate(kunaiPrefab, throwPoint.position, throwPoint.rotation);
    }

    private void ResetAttack()
    {
        ChangeAnim("idle");
        isAttack = false;
    }

    public void Jump()
    {
        isJumping = true;
        ChangeAnim("jump");
        rb.AddForce(jumpForce * Vector2.up);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Coin"))
        {
            coin++;
            PlayerPrefs.SetInt("coin", coin);
            UIManager.Instance.SetCoin(coin);
            Destroy(col.gameObject);
        }

        if (col.CompareTag("DeathZone"))
        {
            isDead = true;
            ChangeAnim("die");
            
            Invoke(nameof(OnInit), 1f);
        }
    }

    private void ActiveAttack()
    {
        attackArea.SetActive(true);
    }

    private void DeactiveAttack()
    {
        attackArea.SetActive(false);
    }

    public void SetMove(float horizontal)
    {
        this.horizontal = horizontal;
    }
    public void SavePoint()
    {
        savePoint = transform.position;
    }
}