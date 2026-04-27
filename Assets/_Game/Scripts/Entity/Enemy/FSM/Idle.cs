using UnityEngine;
using UnityEngine.AI;

public class Idle : State
{
    public Idle(GameObject _npc, Animator _anim, Transform _target) : base(_npc, _anim, _target)
    {
        name = STATE.IDLE;
    }

    public override void Enter()
    {
        // anim.SetTrigger("isIdle");
        base.Enter();
    }

    public override void Update()
    {
        Debug.Log($"[Idle] dist: {Vector2.Distance(npc.transform.position, target.position)}");

        float detectionRange = 5;
        float distToTarget = Vector2.Distance(base.npc.transform.position, base.target.position);
        base.npc.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        if (distToTarget < detectionRange)
        {
            nextState = new Chase(npc, anim, target);
            stage = EVENT.EXIT;
            return;
        }
        base.Update();
    }

    public override void Exit()
    {
        // anim.ResetTrigger("isIdle");
        base.Exit();
    }

}