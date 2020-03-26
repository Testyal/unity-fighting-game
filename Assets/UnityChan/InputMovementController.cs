using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(MovementController))]
public class InputMovementController: MonoBehaviour
{
    private MovementController movementController;

    private void Awake()
    {
        this.movementController = this.GetComponent<MovementController>();
    }
    
    private void OnMotion(InputValue value)
    {
        movementController.Motion(value.Get<Vector2>());
    }
}