using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    Animator anim;

    public Transform target;
    State currentState;


    void Start()
    {
        anim = GetComponent<Animator>();
        currentState = new Idle(gameObject, anim, target);
    }

    void Update()
    {
        currentState = currentState.Process();
    }

}