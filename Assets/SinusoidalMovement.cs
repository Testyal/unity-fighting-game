using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinusoidalMovement : MonoBehaviour
{
    [SerializeField] private float amplitude;
    [SerializeField] private float speed;
    
    private float timeElapsed = 0.0f;
    
    void Update()
    {
        timeElapsed += Time.deltaTime;
        // to avoid overflow.
        if (timeElapsed > 2.0f * Mathf.PI) timeElapsed = 0.0f;
        
        // d/dt A sin(ωt) = Aω cos(ωt)
        transform.Translate((amplitude * speed * Mathf.Cos(speed * timeElapsed) * Time.deltaTime) * Vector3.right);
    }
}
