using System;
using UnityEngine;


public class InputAttackController : MonoBehaviour
{
    private AttackController attackController;

    private void Start()
    {
        this.attackController = this.GetComponent<AttackController>();
    }

    private void OnLightPunch()
    {
        attackController.LightPunch();
    }
}
