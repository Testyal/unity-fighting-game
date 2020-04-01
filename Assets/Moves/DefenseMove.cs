using System;
using UnityEngine;


class DefenseMove: MonoBehaviour
{
    [Header("Opponent frame data")] 
    [SerializeField] public int onBlock;
    [SerializeField] private int onHit;
    
    public (DefenseState, Action<MovementController>) InitializeDefenderBlock()
    {
        return (DefenseState.Blocking, controller => controller.DisableMotion());
    }

    public (DefenseState, Action<MovementController>) InitializeDefenderHit()
    {
        return (DefenseState.Hit, controller => controller.JumpUpwards());
    }

    private int elapsedDefenderFrames = 0;
    public (DefenseState, Action<MovementController>) TickDefenderBlock()
    {
        elapsedDefenderFrames++;

        if (elapsedDefenderFrames == onBlock)
        {
            Destroy(this.gameObject);
            return (DefenseState.None, controller => controller.EnableMotion());
        }

        return (DefenseState.Blocking, _ => { });
    }

    public (DefenseState, Action<MovementController>) TickDefenderHit()
    {
        elapsedDefenderFrames++;

        if (elapsedDefenderFrames == onHit)
        {    
            Destroy(this.gameObject);
            return (DefenseState.None, controller => controller.EnableMotion());
        }

        return (DefenseState.Hit, _ => { });
    }
}
