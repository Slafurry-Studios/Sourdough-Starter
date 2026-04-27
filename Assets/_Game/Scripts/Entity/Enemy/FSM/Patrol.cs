using UnityEngine;
using UnityEngine.AI;

public class Patrol : State
{
    int currentIndex = -1;

    public Patrol(GameObject _npc, Animator _anim, Transform _target) : base(_npc, _anim, _target)
    {
        name = STATE.PATROL;
    }

    public override void Enter()
    {
        // anim.SetTrigger("isWalking");
        base.Enter();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        // anim.ResetTrigger("isWalking");
        base.Exit();
    }
    
}