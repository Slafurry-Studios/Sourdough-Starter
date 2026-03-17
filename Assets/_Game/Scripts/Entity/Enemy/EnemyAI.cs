using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float detectionRange;
    [SerializeField] private float attackRange;
    [SerializeField] private float moveSpeed;

    [Header("Obstacle Avoidance")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float avoidanceRadius = 0.5f;
    [SerializeField] private float avoidanceDistance = 1.5f;
    [SerializeField] private int rayCount = 8;
    [SerializeField] private float directionLockTime = 0.2f;  // ← tambah ini

    private Vector2 _lockedDir;
    private float _directionLockTimer = 0f;

    enum State { IDLE, PATROL, CHASING, ATTACK }
    private State _state = State.IDLE;

    private Transform _target;
    private Rigidbody2D rb;
    float distToTarget = float.MaxValue;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _target = p.transform;
    }

    void Update()
    {
        if (_target == null) return;

        distToTarget = Vector2.Distance(transform.position, _target.position);

        switch (_state)
        {
            case State.IDLE: HandleIdle(); break;
            case State.PATROL: HandlePatrol(); break;
            case State.CHASING: HandleChasing(); break;
            case State.ATTACK: HandleAttack(); break;
        }

        UpdateRotation();
    }


    // ================== STATES ==================

    void HandleIdle()
    {
        rb.velocity = Vector2.zero;
        if (distToTarget < detectionRange)
            _state = State.CHASING;
    }

    void HandlePatrol()
    {
        if (distToTarget < detectionRange)
            _state = State.CHASING;
    }

    void HandleChasing()
    {
        if (distToTarget > detectionRange)
        {
            _state = State.IDLE;
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 moveDir = GetSteeringDirection();
        rb.velocity = moveDir * moveSpeed;
    }

    void HandleAttack() { }


    // ================== OBSTACLE AVOIDANCE ==================

    Vector2 GetSteeringDirection()
    {
        Vector2 toTarget = ((Vector2)_target.position - (Vector2)transform.position).normalized;

        // Hitung ulang hanya kalau timer habis
        if (_directionLockTimer > 0f)
        {
            _directionLockTimer -= Time.deltaTime;

            // Kalau arah terkunci sudah clear, boleh langsung ke player lagi
            if (!IsBlocked(toTarget))
            {
                _lockedDir = toTarget;
                _directionLockTimer = 0f;
            }

            return _lockedDir;
        }

        // Jalan langsung ke player kalau clear
        if (!IsBlocked(toTarget))
        {
            _lockedDir = toTarget;
            return _lockedDir;
        }

        // Cari arah terbaik
        Vector2 bestDir = _lockedDir; // fallback ke arah sebelumnya
        float bestScore = float.MinValue;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = (360f / rayCount) * i;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;

            if (IsBlocked(dir)) continue;

            float score = Vector2.Dot(dir, toTarget);
            if (score > bestScore)
            {
                bestScore = score;
                bestDir = dir;
            }
        }

        // Lock arah yang dipilih
        _lockedDir = bestDir;
        _directionLockTimer = directionLockTime;
        return _lockedDir;
    }

    bool IsBlocked(Vector2 dir)
    {
        // CircleCast lebih akurat dari Raycast untuk enemy berbadan bulat/kotak
        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            avoidanceRadius,
            dir,
            avoidanceDistance,
            obstacleLayer
        );
        return hit.collider != null;
    }


    // ================== UTILS ==================

    private void UpdateRotation()
    {
        if (rb.velocity.sqrMagnitude < 0.01f) return;
        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }


    // ================== GIZMOS ==================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Visualisasi ray avoidance
        if (!Application.isPlaying) return;
        for (int i = 0; i < rayCount; i++)
        {
            float angle = (360f / rayCount) * i;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
            bool blocked = IsBlocked(dir);
            Gizmos.color = blocked ? Color.red : Color.green;
            Gizmos.DrawRay(transform.position, dir * avoidanceDistance);
        }
    }
}