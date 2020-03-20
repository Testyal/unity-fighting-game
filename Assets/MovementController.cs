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

class JumpingMovement : MovementRegime
{
    private readonly float jumpingSpeed;
    private readonly float gravity;
    private readonly float horizontalAirSpeed;

    public JumpingMovement(Transform transform, float gravity, float horizontalAirSpeed, float jumpingSpeed)
        : base(transform)
    {
        this.gravity = gravity;
        this.horizontalAirSpeed = horizontalAirSpeed;
        this.jumpingSpeed = jumpingSpeed;
    }

    int Sign(JumpingDirection direction)
    {
        switch (direction)
        {
            case JumpingDirection.Left: return -1;
            case JumpingDirection.None: return 0;
            case JumpingDirection.Right: return +1;
        }

        return 0;
    }

    private float jumpingTimeElapsed = 0.0f;

    public MovementState FixedUpdate(JumpingDirection direction, float delta)
    {
        jumpingTimeElapsed += delta;
        this.Transform.position += (Sign(direction) * horizontalAirSpeed * delta) * Vector3.right
                                   + (this.jumpingSpeed * delta - this.gravity * jumpingTimeElapsed * delta) *
                                   Vector3.up;

        if (this.Transform.position.y < 0.0f)
        {
            this.Transform.position -= this.Transform.position.y * Vector3.up;
            this.jumpingTimeElapsed = 0.0f;

            return MovementState.Stationary;
        }

        return MovementState.Jumping;
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

    private MovementState state;
    public MovementState State => state;

    private GroundedMovement groundedMovement;
    private JumpingMovement jumpingMovement;
    
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

    private void Start()
    {
        Transform transform = this.GetComponent<Transform>();

        this.groundedMovement = new GroundedMovement(transform, this.groundedSpeed);
        this.jumpingMovement = new JumpingMovement(transform, this.gravity, this.horizontalAirSpeed, this.jumpingSpeed);
    }
    
    public MovementState Tick(MovementState currentState)
    {
        this.state = currentState;
        Debug.Log("Movement State: " + currentState);
        switch (currentState)
        {
            case MovementState.Stationary:
            case MovementState.Walking:
            case MovementState.Reversing:
                return groundedMovement.FixedUpdate(this.sideAxis, Time.fixedDeltaTime);
            case MovementState.Jumping:
                return jumpingMovement.FixedUpdate(this.direction, Time.fixedDeltaTime);
            default:
                return currentState;
        }
    }
}