using System;
using UnityEngine;

class AttackController : MonoBehaviour
{
    [SerializeField] private GameObject lightPunch;

    private Move currentMove;

    public (ADState, Func<MovementController, MovementState>) Tick(ADState currentState)
    {
        switch (currentState)
        {
            case ADState.PreMove:
                return currentMove.Initialize();
            case ADState.Startup:
            case ADState.Active:
            case ADState.Recovery:
                return currentMove.Tick(currentState);
            default:
                return (currentState, controller => controller.State);
        }
    }

    public ADState LightPunch(ADState currentState)
    {
        if (currentState != ADState.None) return currentState;
        
        this.currentMove = Instantiate(lightPunch).GetComponent<Move>();

        return ADState.PreMove;
    }
}