using System;
using UnityEditor;
using UnityEngine;

enum AttackState
{
    None,
    
    PreMove,
    Startup,
    Active,
    Recovery
}


class AttackController : MonoBehaviour
{
    [SerializeField] private GameObject lightPunch;

    private Move currentMove;
    
    private AttackState state = AttackState.None;

    public Action<MovementController> Tick()
    {
        (AttackState, Action<MovementController>) output; 
        switch (this.state)
        {
            case AttackState.PreMove:
                output = currentMove.Initialize();
                break;
            case AttackState.Startup: 
            case AttackState.Active:
            case AttackState.Recovery:
                output = currentMove.Tick(this.state);
                break;
            default:
                output = (this.state, _ => { });
                break;
        }

        this.state = output.Item1;
        
        return output.Item2;
    }

    public AttackState Resolve(MovementState movementState)
    {
        if (this.bufferedMove != null) BeginMove(bufferedMove);
        this.bufferedMove = null;

        return this.state;
    }

    private GameObject bufferedMove;
    private void OnLightPunch()
    {
        this.bufferedMove = lightPunch;
    }

    private void BeginMove(GameObject move)
    {
        if (this.state != AttackState.None) return;

        this.currentMove = Instantiate(move, this.transform).GetComponent<Move>();
        this.state = AttackState.PreMove;
    }

    public void StandingLP() => BeginMove(lightPunch);

    public void CrouchingLP() => BeginMove(lightPunch);
    
    public void JumpingLP() => BeginMove(lightPunch);

    public void WriteState()
    {
        DebugText.Write($"attackState: {this.state}", 1, 2, Color.red);
    }
}