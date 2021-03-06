﻿using System;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;


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
    CrouchingBlock,
    CrouchingDisabled
}

abstract class MovementRegime
{
    protected Transform transform;
    
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
            this.transform.Translate(groundedSpeed * delta * Mathf.Sign(sideInput), 0.0f, 0.0f, Space.World);
        }

        if (sideInput > 0.1f) return MovementState.Walking;
        if (sideInput < -0.1f) return MovementState.Reversing;
        return MovementState.Stationary;
    }
}


public enum JumpingDirection
{
    Left,
    None,
    Right
}

interface IAirMovement
{
    MovementState Tick(JumpingDirection direction, float delta, float airTimeElapsed);
}

class AirMovementController: MovementRegime
{
    private Jump jump;

    private float airTimeElapsed = 0.0f;

    public AirMovementController(Transform transform)
        : base(transform)
    {
    }

    public void Jump(Jump jump)
    {
        this.jump = jump;
    }

    public void Jump(MovementState behaviorOnLanding, float jumpingSpeed, float gravity, float horizontalAirSpeed,
        JumpingDirection direction)
    {
        this.jump = new Jump(behaviorOnLanding, jumpingSpeed,  gravity,  horizontalAirSpeed, direction);
    }

    public void ChangeLandingBehavior(MovementState behavior)
    {
        this.jump.behaviorOnLanding = behavior;
    }

    public MovementState Tick(MovementState state, float delta)
    {
        this.airTimeElapsed += delta;

        MovementState newState = this.jump.Tick(ref this.transform, delta, this.airTimeElapsed);

        if (newState != MovementState.Jumping && newState != MovementState.Landing)
        {
            this.airTimeElapsed = 0.0f;
        }

        return newState;
    }
}


class Jump
{
    public MovementState behaviorOnLanding;

    private readonly float jumpingSpeed;
    private readonly float gravity;
    private readonly float horizontalAirSpeed;
    private readonly JumpingDirection direction;

    public Jump(MovementState behaviorOnLanding, float jumpingSpeed, float gravity, float horizontalAirSpeed,
        JumpingDirection direction)
    {
        this.behaviorOnLanding = behaviorOnLanding;
        this.jumpingSpeed = jumpingSpeed;
        this.gravity = gravity;
        this.horizontalAirSpeed = horizontalAirSpeed;
        this.direction = direction;
    }

    private static int Sign(JumpingDirection direction)
    {
        switch (direction)
        {
            case JumpingDirection.Left: return -1;
            case JumpingDirection.None: return 0;
            case JumpingDirection.Right: return +1;
        }

        return 0;
    }
    
    public MovementState Tick(ref Transform player, float delta, float elapsed)
    {
        player.position += (Sign(direction) * horizontalAirSpeed * delta) * Vector3.right
                           + (this.jumpingSpeed * delta - this.gravity * elapsed * delta) * Vector3.up;
        
        if (player.position.y < 0.0f)
        {
            player.position -= player.position.y * Vector3.up;

            return behaviorOnLanding;
        }

        return MovementState.Jumping;
    }
}

/*
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
*/


public enum Side
{
    Left,
    Right
}

public class MovementController : MonoBehaviour
{
    [Header("Grounded movement")]
    [SerializeField] private float groundedSpeed;
    
    [Header("Jumping movement")]
    [SerializeField] private float gravity;
    [SerializeField] private float horizontalAirSpeed;
    [SerializeField] private float jumpingSpeed;

    private MovementState state;
    public MovementState State => this.state;

    public Side side = Side.Left;

    private GroundedMovement groundedMovement;
    private AirMovementController airMovementController;

    private float sideAxis;
    private JumpingDirection direction;
    public void Motion(Vector2 inputAxis)
    {
        this.sideAxis = inputAxis.x;
        
        switch (this.state)
        {
            case MovementState.Disabled:
                this.state = MovementState.Disabled;
                break;
            
            case MovementState.Jumping:
                this.state = MovementState.Jumping;
                break;
            
            case MovementState.Landing:
                this.state = MovementState.Landing;
                break;
            
            case MovementState.Walking:
            case MovementState.Reversing:
            case MovementState.Stationary:
                if (inputAxis.y > 0.1f)
                {
                    if (this.sideAxis > 0.1f) this.direction = JumpingDirection.Right;
                    else if (this.sideAxis < -0.1f) this.direction = JumpingDirection.Left;
                    else this.direction = JumpingDirection.None;

                    airMovementController.Jump(MovementState.Stationary, jumpingSpeed, gravity, horizontalAirSpeed, direction);
                    
                    this.state = MovementState.Jumping;
                }
                else if (inputAxis.x > 0.1f) this.state = MovementState.Walking;
                else if (inputAxis.x < -0.1f) this.state = MovementState.Reversing;
                else this.state = MovementState.Stationary;
                break;
                
            default:
                this.state = MovementState.Stationary;
                break;
        }
    }

    private void Awake()
    {
        Transform transform = this.GetComponent<Transform>();

        this.groundedMovement = new GroundedMovement(transform, this.groundedSpeed);
        this.airMovementController = new AirMovementController(transform);
    }
    
    public MovementState Tick()
    {
        switch (this.state)
        {
            case MovementState.Stationary:
            case MovementState.Walking:
            case MovementState.Reversing:
                this.state = groundedMovement.FixedUpdate(this.sideAxis, Time.fixedDeltaTime);
                break;
            
            case MovementState.Jumping:
            case MovementState.Landing:
                this.state = airMovementController.Tick(this.state, Time.fixedDeltaTime);
                break;
        }

        return this.state;
    }

    public void JumpBackwards(float horizontalAirSpeed, float gravity, float jumpingSpeed)
    {
        JumpingDirection backwardDirection = this.side == Side.Left ? JumpingDirection.Left : JumpingDirection.Right;

        airMovementController.Jump(MovementState.Disabled, jumpingSpeed, gravity, horizontalAirSpeed, backwardDirection);

        this.state = MovementState.Jumping;
    }

    public void JumpBackwards()
    {
        this.JumpBackwards(this.horizontalAirSpeed, this.gravity, this.jumpingSpeed);
    }

    public void JumpUpwards()
    {
        airMovementController.Jump(MovementState.Stationary, this.jumpingSpeed, this.gravity, this.horizontalAirSpeed, JumpingDirection.None);

        this.state = MovementState.Jumping;
    }

    public void DisableMotion()
    {
        switch (this.state)
        {
            case MovementState.Crouching:
                this.state = MovementState.CrouchingDisabled;
                break;
            
            case MovementState.Walking:
            case MovementState.Stationary:
            case MovementState.Reversing:
                this.state = MovementState.Disabled;
                break;
            
            case MovementState.Jumping:
                this.state = MovementState.Landing;
                break;
        }
    }

    public void EnableMotion()
    {
        switch (this.state)
        {
            case MovementState.CrouchingDisabled:
                this.state = MovementState.Crouching;
                break;
            
            case MovementState.Disabled:
                this.state = MovementState.Stationary;
                break;
            
            case MovementState.Landing:
                this.state = MovementState.Jumping;
                break;
            
            case MovementState.Jumping:
                this.airMovementController.ChangeLandingBehavior(MovementState.Stationary);
                break;
        }
    }

    public void WriteState(Side side)
    {
        DebugText.Write($"movementState: {this.state}", 0, 0, Color.green, side);
    }
}
