using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class Variant : MonoBehaviour
{
    [SerializeField] private GameObject standing;
    [SerializeField] private GameObject crouching;
    [SerializeField] private GameObject jumping;

    private Move standingMove;
    private Move crouchingMove;
    private Move jumpingMove;

    private void Awake()
    {
        this.standingMove = this.standing.GetComponent<Move>();
        this.crouchingMove = this.crouching.GetComponent<Move>();
        this.jumpingMove = this.jumping.GetComponent<Move>();
    }

    public GameObject ResolveGameObject(MovementState movementState)
    {
        switch (movementState)
        {
            case MovementState.Reversing:
            case MovementState.Stationary:
            case MovementState.Walking:
                return standing;
            
            case MovementState.Crouching:
            case MovementState.CrouchingBlock:
                return crouching;
            
            case MovementState.Jumping:
                return jumping;
            
            default:
                return null;
        }
    }
    
    public Move ResolveMove(MovementState movementState)
    {
        switch (movementState)
        {
            case MovementState.Reversing:
            case MovementState.Stationary:
            case MovementState.Walking:
                return standingMove;
            
            case MovementState.Crouching:
            case MovementState.CrouchingBlock:
                return crouchingMove;
            
            case MovementState.Jumping:
                return jumpingMove;
            
            default:
                return null;
        }
    }
}
