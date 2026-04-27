using UnityEngine;
using UnityEngine.AI;

public class State
{
    public enum STATE
    {
        IDLE, PATROL, CHASING, ATTACK
    }

    public enum EVENT
    {
        ENTER, UPDATE, EXIT
    }

    public STATE name;

    protected EVENT stage;
    protected GameObject npc;
    protected Animator anim;
    protected Transform target;
    protected State nextState;

    float visDist = 10.0f;
    float visAngle = 30.0f;
    float shootDist = 7.0f;

    public State(GameObject _npc, Animator _anim, Transform _target)
    {
        npc = _npc;
        anim = _anim;
        stage = EVENT.ENTER;
        target = _target;
    }

    public virtual void Enter()
    {
        stage = EVENT.UPDATE;
    }

    public virtual void Update()
    {
        stage = EVENT.UPDATE;
    }

    public virtual void Exit()
    {
        stage = EVENT.EXIT;
    }

    public State Process()
    {
        if (stage == EVENT.ENTER) Enter();
        if (stage == EVENT.UPDATE) Update();
        if (stage == EVENT.EXIT)
        {
            Exit();
            return nextState;
        }

        return this;
    }

}