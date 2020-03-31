using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinusoidalMovement : MonoBehaviour
{
    [SerializeField] private float amplitude;
    
    private float timeElapsed = 0.0f;
    
    void Update()
    {
        timeElapsed += Time.deltaTime;
        
        transform.Translate((amplitude * Mathf.Cos(timeElapsed) * Time.deltaTime) * Vector3.right);
    }
}
