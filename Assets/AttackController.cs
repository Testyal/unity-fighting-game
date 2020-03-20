using System;
using UnityEngine;

class AttackController : MonoBehaviour
{
    [SerializeField] private GameObject lightPunch;

    private Move currentMove;

    public (ADState, Func<MovementController, MovementState>) Tick(ADState currentADState, MovementState currentMoveState)
    {
        Debug.Log("AD State: " + currentADState);
        switch (currentADState)
        {
            case ADState.PreMove:
                return currentMove.Initialize();
            case ADState.Startup:
            case ADState.Active:
            case ADState.Recovery:
                return currentMove.Tick(currentADState);
            default:
                return (currentADState, _ => currentMoveState);
        }
    }

    public ADState LightPunch(ADState currentState)
    {
        if (currentState != ADState.None) return currentState;
        
        this.currentMove = Instantiate(lightPunch).GetComponent<Move>();

        return ADState.PreMove;
    }
}