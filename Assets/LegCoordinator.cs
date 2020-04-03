using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coordinates the motion of two pairs of legs so that one pair of legs steps only when the other pair has.
/// </summary>
///
/// <remarks>Any LegStepper components referenced by this component should have their Update methods disabled.</remarks>
[ExecuteInEditMode]
public class LegCoordinator : MonoBehaviour
{
    [SerializeField] private LegStepper leftLeg;
    [SerializeField] private LegStepper rightLeg;
    [SerializeField] private SteppingLegID initialSteppingLeg;
    
    enum SteppingLegID
    {
        Left,
        Right
    }
    
    private SteppingLegID steppingLegID;

    private LegStepper SteppingLeg
    {
        get
        {
            switch (this.steppingLegID)
            {
                case SteppingLegID.Left: return this.leftLeg;
                case SteppingLegID.Right: return this.rightLeg;
                
                default: return null;
            }
        }
    }

    private LegStepper OtherLeg
    {
        get
        {
            switch (this.steppingLegID)
            {
                case SteppingLegID.Left: return this.rightLeg;
                case SteppingLegID.Right: return this.leftLeg;
                
                default: return null;
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private void Start()
    {
        this.steppingLegID = initialSteppingLeg;
    }

    private void Update()
    {
        OtherLeg.PlantFoot();
        if (SteppingLeg.Step()) FlipSteppingLeg();
    }
    
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private void FlipSteppingLeg()
    {
        switch (steppingLegID)
        {
            case SteppingLegID.Left: this.steppingLegID = SteppingLegID.Right;
                break;
            case SteppingLegID.Right: this.steppingLegID = SteppingLegID.Left;
                break;
        }
    }
}
