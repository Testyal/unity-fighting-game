using System;
using UnityEngine;

abstract class Move : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage;
    
    [Header("General frame data")]
    [SerializeField] private int startup;
    [SerializeField] private int active;
    [SerializeField] private int recovery;

    [Header("Frame data on hit/block")] 
    [SerializeField] private int onHit;

    protected abstract Func<MovementController, MovementState> EnterStartup { get; }
    protected abstract Func<MovementController, MovementState> EnterActive { get; }
    protected abstract Func<MovementController, MovementState> EnterRecovery { get; }
    protected abstract Func<MovementController, MovementState> EndMove { get; }

    private int elapsedFrames = 0;
    /// <summary>
    /// Ticks a move
    /// </summary>
    /// <param name="current">Current state of the attacking player.</param>
    /// <returns>State of the attacking player after the tick.</returns>
    /// <exception cref="Exception">Parameter <c>current</c> must be Startup, Active, or Recovery.</exception>
    public (ADState, Func<MovementController, MovementState>) Tick(ADState current)
    {
        elapsedFrames++;
        switch (current)
        {
            case ADState.Startup:
                return Startup();
            case ADState.Active:
                return Active();
            case ADState.Recovery:
                return Recovery();
            default:
                throw new Exception("Attempting to tick a move during an inappropriate ADState. (Must be Startup, Active, or Recovery.)");
        }
    }
    
    public (ADState, Func<MovementController, MovementState>) Initialize()
    {
        return (ADState.Startup, EnterStartup);
    }

    private (ADState, Func<MovementController, MovementState>) Startup()
    {
        elapsedFrames++;

        if (elapsedFrames < startup) return (ADState.Startup, controller => controller.State);

        elapsedFrames = 0;
        return (ADState.Active, EnterActive);
    }

    private (ADState, Func<MovementController, MovementState>) Active()
    {
        elapsedFrames++;
        
        if (elapsedFrames < active) return (ADState.Active, controller => controller.State);
        
        elapsedFrames = 0;
        return (ADState.Recovery, EnterRecovery);
    }

    private (ADState, Func<MovementController, MovementState>) Recovery()
    {
        elapsedFrames++;

        if (elapsedFrames < recovery) return (ADState.Recovery, controller => controller.State);
        
        elapsedFrames = 0;
        Destroy(this.gameObject);
        
        return (ADState.None, EndMove);
    }
}
