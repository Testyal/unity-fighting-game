using System;
using UnityEngine;


enum DefenseState
{
    // During normal/attacking situations.
    None,
    
    // Prepare to defend.
    PreBlock,
    PreHit,
    
    // During defense against move.
    Blocking,
    Hit
}


class DefenseController : MonoBehaviour
{
    private DefenseState state = DefenseState.None;
    private DefenseMove move;

    private Move collidedMove;

    private void OnTriggerEnter(Collider other)
    {
        this.collidedMove = other.gameObject.GetComponent<Move>();
    }

    /// <summary>
    /// Resolves any collisions with opponent moves.
    /// </summary>
    /// <param name="movementState">Current movement state of the character.</param>
    public DefenseState Resolve(MovementState movementState)
    {
        this.ResolveCollisions(movementState);

        return this.state;
    }

    private void ResolveCollisions(MovementState movementState)
    {
        if (this.collidedMove == null) return;

        switch (this.collidedMove.height)
        {
            case MoveHeight.High:
                if (movementState == MovementState.Reversing) this.state = DefenseState.PreBlock;
                else this.state = DefenseState.PreHit;
                break;

            case MoveHeight.Mid:
                if (movementState == MovementState.Reversing || movementState == MovementState.CrouchingBlock)
                    this.state = DefenseState.PreBlock;
                else this.state = DefenseState.PreHit;
                break;

            case MoveHeight.Low:
                if (movementState == MovementState.CrouchingBlock) this.state = DefenseState.PreBlock;
                else this.state = DefenseState.PreHit;
                break;

            default:
                this.state = DefenseState.None;
                break;
        }
    }

    /// <summary>
    /// Initializes defense, or ticks defense forward by one frame. Called when the current AD state is defending.
    /// </summary>
    /// <returns>A callback to change the movement properties of the defending character.</returns>
    ///
    /// After 
    /// 
    /// <example>A typical move will disable the character's movement for a certain number of frames.
    /// An unusual move may cause the character to be launched upwards on the fifth frame.</example>
    public Action<MovementController> Tick()
    {
        switch (this.state)
        {
            case DefenseState.PreHit:
                return this.BeginHit();

            case DefenseState.PreBlock:
                return this.BeginBlock();

            case DefenseState.Hit:
                var (defenseStateHit, movementCallbackHit) = this.move.TickDefenderHit();
                this.state = defenseStateHit;

                return movementCallbackHit;

            case DefenseState.Blocking:
                var (defenseStateBlock, movementCallbackBlock) = this.move.TickDefenderBlock();
                this.state = defenseStateBlock;

                return movementCallbackBlock;


            default:
                return _ => { };
        }
    }

    private Action<MovementController> BeginHit()
    {
        this.move = Instantiate(collidedMove.defenseObject, this.transform).GetComponent<DefenseMove>();
        this.collidedMove = null;

        var (defenseState, movementCallback) = this.move.InitializeDefenderHit();
        this.state = defenseState;

        return movementCallback;

    }

    private Action<MovementController> BeginBlock()
    {
        this.move = Instantiate(collidedMove.defenseObject, this.transform).GetComponent<DefenseMove>();
        this.collidedMove = null;

        var (defenseState, movementCallback) = this.move.InitializeDefenderBlock();
        this.state = defenseState;

        return movementCallback;
    }

    public void WriteState(Side side)
    {
        DebugText.Write($"defenseState: {this.state}", 1, 3, Color.red, side);
    }
}