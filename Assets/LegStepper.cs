using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
class LegStepper: MonoBehaviour
{
    [SerializeField] private float step;

    private InverseKinematics kinematics;
    private LegInterpolator interpolator;
    
    private Vector3 foot;

    public enum LegState
    {
        Stationary,
        Interpolating
    }

    private LegState state;
    
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private void Start()
    {
        Transform lowerLeg = transform.Find("LowerLeg");
        Transform upperLeg = transform.Find("UpperLeg");
        
        this.kinematics = new InverseKinematics(lowerLeg, upperLeg);

        this.foot = kinematics.Foot();
        
        this.state = LegState.Stationary;
    }
    

    private void Update()
    {
        switch (state)
        {
            case LegState.Stationary:
                UpdateStationary();
                break;
            case LegState.Interpolating:
                UpdateInterpolator();
                break;
        }
    }
    
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private void UpdateStationary()
    {
        RaycastHit raycast;
        if (Physics.Raycast(transform.position + step * Vector3.right, Vector3.down, out raycast))
        {
            Debug.DrawLine(transform.position, transform.position + step * Vector3.right, Color.red);
            Debug.DrawLine(transform.position + step * Vector3.right, raycast.point, Color.red);

            if (Vector3.Magnitude(foot - raycast.point) > Mathf.Abs(step))
            {
                this.interpolator = new LegInterpolator(foot, raycast.point);
                this.foot = raycast.point;
                state = LegState.Interpolating;
                return;
            }

            kinematics.Target(foot);
        }
    }
    

    private void UpdateInterpolator()
    {
        Vector3 point = interpolator.CalculateLegPosition(Time.deltaTime);
        kinematics.Target(point);

        if (point == foot) this.state = LegState.Stationary;
    }
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class InverseKinematics
{
    private Transform transform;
    
    private Transform lowerLeg;
    private Transform upperLeg;

    private float legLength;

    public InverseKinematics(Transform lowerLeg, Transform upperLeg)
    {
        this.lowerLeg = lowerLeg;
        this.upperLeg = upperLeg;

        this.transform = upperLeg;

        this.legLength = lowerLeg.localScale.x;
    }
    
    public InverseKinematics(Transform lowerLeg, Transform upperLeg, float legLength)
    {
        this.lowerLeg = lowerLeg;
        this.upperLeg = upperLeg;

        this.legLength = legLength;
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
            yParameter = Mathf.Sqrt(1.0f / legLength - (deltaY * deltaY) / (4.0f * legLength * legLength * legLength * legLength));
        }
        else if (deltaY == 0.0f)
        {
            xParameter = Mathf.Sqrt(1.0f / legLength - (deltaX * deltaX) / (4.0f * legLength * legLength * legLength * legLength));
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

    public void Target(Vector3 target)
    {
        Vector3 delta = target - transform.position;
            
        Debug.DrawLine(transform.position, target, Color.green);

        // If the target position is too far away from the top of the leg, inverse kinematics won't be able to reach it.
        // This line ensures the leg stays below its maximum extension.
        if (Vector3.Magnitude(delta) >= 2 * legLength) delta *= 1.99f * legLength / Vector3.Magnitude(delta);

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


    public Vector3 Foot()
    {
        return lowerLeg.position + legLength * (Mathf.Cos(lowerLeg.eulerAngles.z) * Vector3.right +
                                                Mathf.Sin(lowerLeg.eulerAngles.z) * Vector3.up);
    }
}

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

class LegInterpolator
{
    private float timeElapsed;
    private float totalTime;

    private Vector3 start;
    private Vector3 end;

    public LegInterpolator(Vector3 start, Vector3 end)
    {
        this.timeElapsed = 0.0f;
        this.totalTime = 0.25f;

        this.end = end;
        this.start = start;
    }

    public Vector3 CalculateLegPosition(float delta)
    {
        timeElapsed += delta;
        if (timeElapsed > totalTime) return end;
        
        return Vector3.Lerp(start, end, timeElapsed / totalTime) 
               - 2.0f * (timeElapsed / totalTime) * (timeElapsed / totalTime - 1.0f) * Vector3.up;
    }
}


