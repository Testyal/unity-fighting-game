using System;
using System.Diagnostics;
using UnityEngine;


enum ADStateAlt
{
    None,
    Attacking,
    Defending
}

class ADController 
{
    private ADStateAlt state;
    
    private AttackController attackController;
    private DefenseController defenseController;

    public ADController(AttackController attackController, DefenseController defenseController)
    {
        this.state = ADStateAlt.None;
        
        this.attackController = attackController;
        this.defenseController = defenseController;
    }

    public void Resolve(MovementState movementState)
    {
        var defenseState = defenseController.Resolve(movementState);
        var attackState = attackController.Resolve(movementState);

        switch (defenseState)
        {
            case DefenseState.Blocking:
            case DefenseState.Hit:
            case DefenseState.PreBlock:
            case DefenseState.PreHit:
                if (attackState != AttackState.None) attackController.Cancel();
                this.state = ADStateAlt.Defending;
                break;
            
            default:
                switch (attackState)
                {
                    case AttackState.PreMove:
                    case AttackState.Startup:
                    case AttackState.Active:
                    case AttackState.Recovery:
                        this.state = ADStateAlt.Attacking;
                        break;
                    
                    default:
                        this.state = ADStateAlt.None;
                        break;
                }

                break;
        }
    }

    public Action<MovementController> Tick()
    {
        switch (this.state)
        {
            case ADStateAlt.Attacking:
                return attackController.Tick();
            
            case ADStateAlt.Defending:
                return defenseController.Tick();
            
            default:
                return _ => { };
        }
    }

    public void WriteState(Side side)
    {
        DebugText.Write($"adState: {this.state}", 0, 1, Color.red, side);
        attackController.WriteState(side);
        defenseController.WriteState(side);
    }
}