using UnityEngine;
using UnityEngine.InputSystem;


enum MovementState
{
    Grounded,
    Jumping
}


class MovementRegime
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
            this.Transform.forward = sideInput * Vector3.right;
            this.Transform.Translate(groundedSpeed * delta * Mathf.Sign(sideInput), 0.0f, 0.0f, Space.World);
        }

        return MovementState.Grounded;
    }
}


enum JumpingDirection
{
    Left,
    None,
    Right
}

class JumpingMovement: MovementRegime
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
                                   + (this.jumpingSpeed * delta - this.gravity * jumpingTimeElapsed * delta) * Vector3.up;

        if (this.Transform.position.y < 0.0f)
        {
            this.Transform.position -= this.Transform.position.y * Vector3.up;
            this.jumpingTimeElapsed = 0.0f;

            return MovementState.Grounded;
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

    private GroundedMovement groundedMovement;
    private JumpingMovement jumpingMovement;

    private float sideInput;
    private void OnSideMovement(InputValue value)
    {
        this.sideInput = value.Get<float>();
    }
    
    // TODO: Big problem with this - A side input has to be given at least a frame before jump is pressed in order to do a diagonal jump. 
    private JumpingDirection jumpingDirection;
    private void OnJump()
    {
        if (this.sideInput > 0.1f) this.jumpingDirection = JumpingDirection.Right;
        else if (this.sideInput < -0.1f) this.jumpingDirection = JumpingDirection.Left;
        else this.jumpingDirection = JumpingDirection.None;

        this.state = MovementState.Jumping;
    }

    private void Start()
    {
        Transform transform = this.GetComponent<Transform>();

        this.groundedMovement = new GroundedMovement(transform, this.groundedSpeed);
        this.jumpingMovement = new JumpingMovement(transform, this.gravity, this.horizontalAirSpeed, this.jumpingSpeed);
    }
    private void FixedUpdate()
    {
        switch (this.state)
        {
            case MovementState.Grounded:
                this.state = groundedMovement.FixedUpdate(this.sideInput, Time.fixedDeltaTime);
                break;
            case MovementState.Jumping:
                this.state = jumpingMovement.FixedUpdate(this.jumpingDirection, Time.fixedDeltaTime);
                break;
        }
    }
}
