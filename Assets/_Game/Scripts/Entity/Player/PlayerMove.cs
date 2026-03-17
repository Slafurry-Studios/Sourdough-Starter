using System;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerEnergy playerEnergy;

    // ==================== MOVEMENT ====================
    private Vector2 moveInput;
    private bool isSprinting;
    private float currentMoveSpeed;

    [SerializeField] private float normalSpeed = 10f;
    [SerializeField] private float sprintSpeed = 25f;
    [SerializeField] private float sprintEnergyPerSecond = 8f;

    void Awake()
    {
        rb = GetComponentInParent<Rigidbody2D>();
        playerEnergy = GetComponentInParent<PlayerEnergy>();
    }

    void Update()
    {
        // Baca input di Update, bukan FixedUpdate
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        isSprinting = Input.GetKey(KeyCode.LeftShift);
    }

    void FixedUpdate()
    {
        HandleSprintEnergy();
        Move();
    }

    private void HandleSprintEnergy()
    {
        if (!isSprinting || moveInput == Vector2.zero) return;

        float energyCost = sprintEnergyPerSecond * Time.fixedDeltaTime;
        bool success = playerEnergy.UseEnergy(Mathf.CeilToInt(energyCost));

        if (!success)
        {
            Debug.Log("[PlayerMove] Energy depleted, sprint stopped");
            isSprinting = false;
        }
    }

    private void Move()
    {
        if (moveInput == Vector2.zero) return;

        currentMoveSpeed = isSprinting ? sprintSpeed : normalSpeed;
        rb.MovePosition(rb.position + moveInput * currentMoveSpeed * Time.fixedDeltaTime);

        float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public float GetCurrentSpeed() => currentMoveSpeed;
}