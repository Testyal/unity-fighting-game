using UnityEngine;
using UnityEngine.InputSystem;

public enum MovementState
{
    // Ground movement
    Stationary,
    Walking,
    Reversing,
    
    Jumping,
    
    Crouching,
    
    Disabled
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
    
    private MovementState moveState;
    private ADState adState;

    private void Awake()
    {
        movementController = GetComponent<MovementController>();
        attackController = GetComponent<AttackController>();
    }

    private void FixedUpdate()
    {
        this.moveState = movementController.Tick(moveState);
        
        var (attackADState, attackMoveFunction) = attackController.Tick(adState, moveState);

        this.adState = attackADState;
        this.moveState = attackMoveFunction(movementController);
    }

    private void OnMotion(InputValue value)
    {
        this.moveState = movementController.Motion(moveState, value.Get<Vector2>());
    }
    
    private void OnLightPunch()
    {
        this.adState = attackController.LightPunch(adState);
    }
}