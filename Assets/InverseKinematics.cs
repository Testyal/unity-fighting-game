using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class InverseKinematics : MonoBehaviour
{
    private Transform upperLeg;
    private Transform lowerLeg;

    [SerializeField] private float legLength;

    [SerializeField] private Vector3 rayDirection;

    private void Start()
    {
        this.upperLeg = this.transform.Find("UpperLeg");
        this.lowerLeg = this.transform.Find("LowerLeg");
    }

    private void Update()
    {
        if (!transform.hasChanged) return;

        RaycastHit raycast;
        if (Physics.Raycast(transform.position, rayDirection, out raycast))
        {
            Debug.DrawRay(transform.position, 10.0f * rayDirection, Color.red);

            Vector3 delta = raycast.point - transform.position;

            var (xParameter, yParameter) = CalculateParameters(delta.x, delta.y);

            // obviously none of this is particularly efficient, I'm just copying the maths on the paper in front of me.
            Vector2 upperLegDirection = (delta.x / (2.0f * legLength) + xParameter * legLength) * Vector2.right
                                  + (delta.y / (2.0f * legLength) + yParameter * legLength) * Vector2.up;

            Vector2 lowerLegDirection = (delta.x / (2.0f * legLength) - xParameter * legLength) * Vector2.right
                                  + (delta.y / (2.0f * legLength) - yParameter * legLength) * Vector2.up;

            float upperLegAngle = Mathf.Rad2Deg * Mathf.Atan2(upperLegDirection.y, upperLegDirection.x);
            float lowerLegAngle = Mathf.Rad2Deg * Mathf.Atan2(lowerLegDirection.y, lowerLegDirection.x);
            
            upperLeg.rotation = Quaternion.Euler(0.0f, 0.0f, upperLegAngle);
            lowerLeg.localPosition = legLength * upperLegDirection;
            lowerLeg.rotation = Quaternion.Euler(0.0f, 0.0f, lowerLegAngle);
        }
    }
    
    private (float, float) CalculateParameters(float deltaX, float deltaY)
    {
        float xParameter;
        float yParameter;
        
        // Current implementation is just algebra to the extreme. An equation solving algorithm might be faster and prettier.
        if (deltaX == 0.0f)
        {
            xParameter = 0.0f;
            // Why use Mathf.Pow when I can multiply a thing by itself multiple times B)
            yParameter = Mathf.Sqrt(1 / legLength - (deltaY * deltaY) / (4.0f * legLength * legLength * legLength * legLength));
        }
        else if (deltaY == 0.0f)
        {
            xParameter = Mathf.Sqrt(1 / legLength - (deltaX * deltaX) / (4.0f * legLength * legLength * legLength * legLength));
            yParameter = 0.0f;
        }
        else
        {
            float numerator = 1.0f / (legLength * legLength) - (deltaX * deltaX + deltaY * deltaY) /
                              (4.0f * legLength * legLength * legLength * legLength);
            float denominator = 1.0f + (deltaY * deltaY) / (deltaX * deltaX);

            xParameter = Mathf.Sqrt(numerator / denominator);
            yParameter = -(deltaY / deltaX) * xParameter;
        }
        
        // uh so this should say (xParameter, yParameter) but I mixed up the parameters on my page somewhere.
        return (yParameter, xParameter);
    }
}
