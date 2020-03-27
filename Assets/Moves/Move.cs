using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;


enum MoveHeight
{
    Low,
    Mid,
    High
}

[RequireComponent(typeof(BoxCollider))]
abstract class Move : MonoBehaviour
{
    [Header("Move properties")]
    [SerializeField] private int damage;
    [SerializeField] public MoveHeight height;
    
    [Header("General frame data")]
    [SerializeField] private int startup;
    [SerializeField] private int active;
    [SerializeField] private int recovery;

    [Header("Opponent frame data")] 
    [SerializeField] public int onBlock;
    [SerializeField] private int onHit;
    
    private BoxCollider boxCollider;
    
    protected abstract Action<MovementController> EnterStartup();
    protected abstract Action<MovementController> EnterActive();
    protected abstract Action<MovementController> EnterRecovery();
    protected abstract Action<MovementController> EndMove();

    private void Start()
    {
        this.boxCollider = this.GetComponent<BoxCollider>();
        boxCollider.enabled = false;
    }

    private int elapsedFrames = 0;
    /// <summary>
    /// Ticks a move
    /// </summary>
    /// <param name="current">Current state of the attacking player.</param>
    /// <returns>State of the attacking player after the tick.</returns>
    /// <exception cref="Exception">Parameter <c>current</c> must be Startup, Active, or Recovery.</exception>
    public (AttackState, Action<MovementController>) Tick(AttackState current)
    {
        elapsedFrames++;
        switch (current)
        {
            case AttackState.Startup:
                return Startup();
            case AttackState.Active:
                return Active();
            case AttackState.Recovery:
                return Recovery();
            default:
                throw new Exception("Attempting to tick a move during an inappropriate AttackState. (Must be Startup, Active, or Recovery.)");
        }
    }

    public (AttackState, Action<MovementController>) Initialize()
    {
        return (AttackState.Startup, EnterStartup());
    }

    private (AttackState, Action<MovementController>) Startup()
    {
        elapsedFrames++;
        
        if (elapsedFrames < startup) return (AttackState.Startup, _ => { });

        elapsedFrames = 0;
        boxCollider.enabled = true;
        return (AttackState.Active, EnterActive());
    }

    private (AttackState, Action<MovementController>) Active()
    {
        elapsedFrames++;
        
        if (elapsedFrames < active) return (AttackState.Active, _ => { });
        
        elapsedFrames = 0;
        boxCollider.enabled = false;
        return (AttackState.Recovery, EnterRecovery());
    }

    private (AttackState, Action<MovementController>) Recovery()
    {
        elapsedFrames++;

        if (elapsedFrames < recovery) return (AttackState.Recovery, _ => { });
        
        elapsedFrames = 0;
        Destroy(this.gameObject);
        
        return (AttackState.None, EndMove());
    }


    public (DefenseState, Action<MovementController>) InitializeDefenderBlock()
    {
        return (DefenseState.Blocking, controller => controller.DisableMotion());
    }
    
    public (DefenseState, Action<MovementController>) InitializeDefenderHit()
    {
        return (DefenseState.Hit, controller => controller.EnterLanding(JumpingDirection.Left));
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
