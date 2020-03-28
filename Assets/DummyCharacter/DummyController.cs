using System;
using UnityEngine;


class DummyController : MonoBehaviour
{
    private AttackController attackController;
    private MovementController movementController;

    private void Awake()
    {
        this.attackController = this.GetComponent<AttackController>();
        this.movementController = this.GetComponent<MovementController>();
    }

    private void FixedUpdate()
    {
        //if (attackController.State == AttackState.None) attackController.LightPunch();
        if (movementController.State == MovementState.Stationary) movementController.JumpUpwards();
    }
}