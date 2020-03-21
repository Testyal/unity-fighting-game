using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

class AttackController : MonoBehaviour
{
    [SerializeField] private GameObject lightPunch;

    private Move currentMove;
    
    public ADState ADState { get; set; }

    public (ADState, Func<MovementController, MovementState>) Tick()
    {
        switch (this.ADState)
        {
            case ADState.PreMove:
                return currentMove.Initialize();
            case ADState.Startup:
            case ADState.Active:
            case ADState.Recovery:
                return currentMove.Tick(this.ADState);
            default:
                return (this.ADState, controller => controller.State);
        }
    }

    public ADState StandingLP()
    {
        if (this.ADState != ADState.None) return this.ADState;

        this.currentMove = Instantiate(lightPunch, this.transform).GetComponent<Move>();

        return ADState.PreMove;
    }

    public ADState CrouchingLP()
    {
        if (this.ADState != ADState.None) return this.ADState;

        this.currentMove = Instantiate(lightPunch, this.transform).GetComponent<Move>();

        return ADState.PreMove;
    }

    public ADState JumpingLP()
    {
        if (this.ADState != ADState.None) return this.ADState;

        this.currentMove = Instantiate(lightPunch, this.transform).GetComponent<Move>();

        return ADState.PreMove;
    }
}