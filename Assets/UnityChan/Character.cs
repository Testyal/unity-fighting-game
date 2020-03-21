using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public enum MovementState
{
    // Ground movement
    Stationary,
    Walking,
    Reversing,
    Disabled,
    
    // Air movement 
    Jumping,
    Landing,
    
    // Crouching Movement
    Crouching,
    CrouchingDisabled
}

public enum ADState
{
    None,
    
    // During attack.
    PreMove,
    Startup,
    Active,
    Recovery,
    
    // During defense.
    Blocking,
    Hit
}

[RequireComponent(typeof(MovementController), typeof(AttackController))]
class Character: MonoBehaviour
{
    [SerializeField] private int health;

    private MovementController movementController;
    private AttackController attackController;
    private DefenseController defenseController;
    
    private MovementState MoveState
    {
        get { return movementController.State; }
        set { movementController.State = value; }
    }

    private ADState ADState
    {
        get { return attackController.ADState; }
        set { attackController.ADState = value; }
    }
    
    private void Awake()
    {
        movementController = GetComponent<MovementController>();
        attackController = GetComponent<AttackController>();
    }

    private void FixedUpdate()
    {
        this.MoveState = movementController.Tick();
        
        var (attackADState, attackMoveFunction) = attackController.Tick();

        this.ADState = attackADState;
        this.MoveState = attackMoveFunction(movementController);
    }

    private void OnGUI()
    {
        DebugText.Draw(MoveState, ADState);
    }

    private void OnMotion(InputValue value)
    {
        this.MoveState = movementController.Motion(MoveState, value.Get<Vector2>());
    }
    
    private void OnLightPunch()
    {
        switch (MoveState)
        {
            case MovementState.Stationary:
            case MovementState.Walking:
            case MovementState.Reversing:
                this.ADState = attackController.StandingLP();
                break;
            
            case MovementState.Crouching:
                this.ADState = attackController.CrouchingLP();
                break;
            
            case MovementState.Jumping:
                this.ADState = attackController.JumpingLP();
                break;
        }
    }
}


class DebugText
{
    public static void Draw(MovementState moveState, ADState adState)
    {
        GUI.color = Color.green;
        GUI.Label(new Rect(5.0f, 0.0f, 500.0f, 20.0f), $"moveState: {moveState}");
        
        GUI.color = Color.red;
        GUI.Label(new Rect(5.0f, 20.0f, 500.0f, 20.0f), $"adState: {adState}");
    }
}