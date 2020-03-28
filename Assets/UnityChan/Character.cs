using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ADState
{
    None,
    
    // During attack.
    PreMove,
    Startup,
    Active,
    Recovery,
    
    // During defense.
    Defending,
    Blocking,
    Hit
}


[RequireComponent(typeof(MovementController))]
public class Character: MonoBehaviour
{
    [SerializeField] private int health;

    public Side Side => movementController.side;
    
    private MovementController movementController;
    private ADController adController;

    private void Awake()
    {
       this.movementController = GetComponent<MovementController>();
        
        var attackController = GetComponent<AttackController>();
        var defenseController = GetComponent<DefenseController>();
        
        this.adController = new ADController(attackController, defenseController);
    }

    private void FixedUpdate()
    {
        var movementState = movementController.Tick();
        
        adController.Resolve(movementState);
        var movementCallback = adController.Tick();

        movementCallback(movementController);
    }

    private void OnGUI()
    {
        movementController.WriteState(this.Side);
        adController.WriteState(this.Side);
    }
}


class DebugText
{
    public static void Write(string text, int depth, int height, Color color, Side side)
    {
        GUI.color = color;
        GUI.Label(new Rect(5.0f + 200.0f * (side == Side.Left ? 0 : 1), 20.0f * height, 500.0f, 20.0f), new string(' ', depth) + text);
    }
}