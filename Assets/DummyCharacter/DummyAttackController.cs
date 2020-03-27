using System;
using UnityEngine;


class DummyAttackController : MonoBehaviour
{
    private AttackController attackController;

    private void Start()
    {
        this.attackController = this.GetComponent<AttackController>();
        
        attackController.LightPunch();
    }
}