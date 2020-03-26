using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyCharacter : MonoBehaviour
{
    [SerializeField] private GameObject moveObject;
    
    private Move move;

    private AttackState state = AttackState.PreMove;
    
    // Start is called before the first frame update
    void Start()
    {
        move = Instantiate(moveObject, this.transform).GetComponent<Move>();

        this.state = move.Initialize().Item1;
    }

    // Update is called once per frame
    void Update()
    {
        this.state = move.Tick(this.state).Item1;
    }
}
