using System;
using UnityEngine;

class UnityChan_sLP : Move
{
    [Header("Unity-chan sLP specific")]
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private Material activeMaterial;

    private MeshRenderer cube;

    private void Awake()
    {
        cube = GetComponentInChildren<MeshRenderer>();
    }

    protected override Action<MovementController> EnterStartup()
    {
        return controller => controller.DisableMotion();
    }

    protected override Action<MovementController> EnterActive()
    {
        cube.material = activeMaterial;

        return _ => { };
    }

    protected override Action<MovementController> EnterRecovery()
    {
        cube.material = inactiveMaterial;

        return _ => { };
    }

    protected override Action<MovementController> EndMove()
    {
        return controller => controller.EnableMotion();
    }
}