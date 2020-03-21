using System;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

abstract class MovementRegime
{
    private readonly Transform transform;
    public Transform Transform => transform;

    public MovementRegime(Transform transform)
    {
        this.transform = transform;
    }
}


class GroundedMovement: MovementRegime
{
    private readonly float groundedSpeed;

    public GroundedMovement(Transform transform, float groundedSpeed)
        : base(transform)
    {
        this.groundedSpeed = groundedSpeed;
    }

    public MovementState FixedUpdate(float sideInput, float delta)
    {
        if (Mathf.Abs(sideInput) > 0.1f)
        {
            this.Transform.Translate(groundedSpeed * delta * Mathf.Sign(sideInput), 0.0f, 0.0f, Space.World);
        }

        if (sideInput > 0.1f) return MovementState.Walking;
        if (sideInput < -0.1f) return MovementState.Reversing;
        return MovementState.Stationary;
    }
}


enum JumpingDirection
{
    Left,
    None,
    Right
}

interface IAirMovement
{
    MovementState Tick(JumpingDirection direction, float delta, float airTimeElapsed);
}

class AirMovementController
{
    private JumpingMovement jumpingMovement;
    private LandingMovement landingMovement;

    private float airTimeElapsed = 0.0f;

    public AirMovementController(Transform transform, float gravity, float horizontalAirSpeed, float jumpingSpeed)
    {
        this.jumpingMovement = new JumpingMovement(transform, gravity,  horizontalAirSpeed,  jumpingSpeed);
        this.landingMovement = new LandingMovement(transform, gravity, horizontalAirSpeed, jumpingSpeed);
    }

    public MovementState Tick(MovementState state, JumpingDirection direction, float delta)
    {
        this.airTimeElapsed += delta;
        Debug.Log(this.airTimeElapsed);

        MovementState newState;
        switch (state)
        {
            case MovementState.Jumping:
                newState = jumpingMovement.Tick(direction, delta, this.airTimeElapsed);
                break;
            case MovementState.Landing:
                newState = landingMovement.Tick(direction, delta, this.airTimeElapsed);
                break;
            default:
                newState = state;
                break;
        }

        if (newState != MovementState.Jumping && newState != MovementState.Landing)
        {
            this.airTimeElapsed = 0.0f;
        }

        return newState;
    }
}

class JumpingMovement : MovementRegime, IAirMovement
{
    protected readonly float jumpingSpeed;
    protected readonly float gravity;
    protected readonly float horizontalAirSpeed;

    public JumpingMovement(Transform transform, float gravity, float horizontalAirSpeed, float jumpingSpeed)
        : base(transform)
    {
        this.gravity = gravity;
        this.horizontalAirSpeed = horizontalAirSpeed;
        this.jumpingSpeed = jumpingSpeed;
    }

    protected int Sign(JumpingDirection direction)
    {
        switch (direction)
        {
            case JumpingDirection.Left: return -1;
            case JumpingDirection.None: return 0;
            case JumpingDirection.Right: return +1;
        }

        return 0;
    }
    
    public MovementState Tick(JumpingDirection direction, float delta, float airTimeElapsed)
    {
        this.Transform.position += (Sign(direction) * horizontalAirSpeed * delta) * Vector3.right
                                   + (this.jumpingSpeed * delta - this.gravity * airTimeElapsed * delta) *
                                   Vector3.up;

        if (this.Transform.position.y < 0.0f)
        {
            this.Transform.position -= this.Transform.position.y * Vector3.up;

            return MovementState.Stationary;
        }

        return MovementState.Jumping;
    }
}


class LandingMovement : JumpingMovement, IAirMovement
{
    public LandingMovement(Transform transform, float gravity, float horizontalAirSpeed, float jumpingSpeed)
        : base(transform, gravity, horizontalAirSpeed, jumpingSpeed)
    {
    }
    
    public MovementState Tick(JumpingDirection direction, float delta, float airTimeElapsed)
    {
        this.Transform.position += (Sign(direction) * horizontalAirSpeed * delta) * Vector3.right
                                   + (this.jumpingSpeed * delta - this.gravity * airTimeElapsed * delta) *
                                   Vector3.up;

        if (this.Transform.position.y < 0.0f)
        {
            this.Transform.position -= this.Transform.position.y * Vector3.up;

            return MovementState.Disabled;
        }

        return MovementState.Landing;
    }
}


public class MovementController : MonoBehaviour
{
    [Header("Grounded movement")]
    [SerializeField] private float groundedSpeed;
    
    [Header("Jumping movement")]
    [SerializeField] private float gravity;
    [SerializeField] private float horizontalAirSpeed;
    [SerializeField] private float jumpingSpeed;

    public MovementState State { get; set; }

    private GroundedMovement groundedMovement;
    private AirMovementController airMovementController;

    private float sideAxis;
    private JumpingDirection direction;
    public MovementState Motion(MovementState currentState, Vector2 inputAxis)
    {
        this.sideAxis = inputAxis.x;
        
        switch (currentState)
        {
            case MovementState.Disabled:
                return MovementState.Disabled;
            
            case MovementState.Jumping:
                return MovementState.Jumping;
            
            case MovementState.Landing:
                return MovementState.Landing;
            
            case MovementState.Walking:
            case MovementState.Reversing:
            case MovementState.Stationary:
                if (inputAxis.y > 0.1f)
                {
                    if (this.sideAxis > 0.1f) this.direction = JumpingDirection.Right;
                    else if (this.sideAxis < -0.1f) this.direction = JumpingDirection.Left;
                    else this.direction = JumpingDirection.None;

                    return MovementState.Jumping;
                }
                
                if (inputAxis.x > 0.1f) return MovementState.Walking;
                if (inputAxis.x < -0.1f) return MovementState.Reversing;
                return MovementState.Stationary;
            
            default:
                return MovementState.Stationary;
        }
    }

    private void Awake()
    {
        Transform transform = this.GetComponent<Transform>();

        this.groundedMovement = new GroundedMovement(transform, this.groundedSpeed);
        this.airMovementController = new AirMovementController(transform, this.gravity, this.horizontalAirSpeed, this.jumpingSpeed);
    }
    
    public MovementState Tick()
    {
        switch (State)
        {
            case MovementState.Stationary:
            case MovementState.Walking:
            case MovementState.Reversing:
                return groundedMovement.FixedUpdate(this.sideAxis, Time.fixedDeltaTime);
            case MovementState.Jumping:
            case MovementState.Landing:
                return airMovementController.Tick(this.State, this.direction, Time.fixedDeltaTime);
            default:
                return State;
        }
    }
    
    public MovementState EnterLanding()
    {
        return MovementState.Landing;
    }

    public MovementState EnterJumping()
    {
        return MovementState.Jumping;
    }
}