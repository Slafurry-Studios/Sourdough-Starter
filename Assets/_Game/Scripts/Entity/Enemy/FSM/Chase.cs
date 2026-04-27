using UnityEngine;

public class Chase : State
{

    public Chase(GameObject _npc, Animator _anim, Transform _target) : base(_npc, _anim, _target)
    {
        name = STATE.CHASING;
    }

    public override void Enter()
    {
        // anim.SetTrigger("isIdle");
        base.Enter();
    }

    public override void Update()
    {
        Debug.Log($"[Chase] dist: {Vector2.Distance(npc.transform.position, target.position)}");

        float distToTarget = Vector2.Distance(base.npc.transform.position, base.target.position);
        float detectionRange = 5;
        if (distToTarget > detectionRange)
        {
            nextState = new Idle(npc, anim, target);
            stage = EVENT.EXIT;
            base.npc.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            return;
        }
        Vector2 moveDir = base.target.position - base.npc.transform.position;
        float moveSpeed = 5f;
        base.npc.GetComponent<Rigidbody2D>().velocity = moveDir * moveSpeed;

        base.Update();
    }

    public override void Exit()
    {
        // anim.ResetTrigger("isIdle");
        base.Exit();
    }
}