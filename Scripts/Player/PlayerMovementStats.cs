using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Player Movement")]
public class PlayerMovementStats : ScriptableObject
{
    [Header("Walk Stats")]
    [Range(1f, 100f)] public float walkSpeed = 12.5f;
    [Range(0.25f, 50f)] public float GroundAcceleration = 5f;
    [Range(0.25f, 50f)] public float GroundDeceleration = 20f;
    [Range(0.25f, 50f)] public float AirAcceleration = 5f;
    [Range(0.25f, 50f)] public float AirDeceleration = 5f;

    [Header("Run Stat")]
    [Range(1f, 100f)] public float runSpeed = 22f;

    [Header("Grounded/Collision Checks")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.02f;
    public float HeadDetectionLength = 0.02f;
    [Range(0.1f, 1f)] public float HeadWidth = 0.75f;

    [Header("Jump Stats")]
    public float jumpHeight = 5.5f;
    [Range(1f, 1.1f)] public float JumpHeightCompensarionFactor = 1.055f;
    public float timeToJumpApex = 0.4f;
    [Range(0.01f, 5f)] public float GravityOnReleaseMultiplier = 2f;
    public float MaxFallSpeed = 25f;
    [Range(1, 4)] public int NumberOfJumpsAllowed = 2;

    [Header("Jump Cut")]
    [Range(0.02f, 0.3f)] public float TimeUpwardsCancel = 0.027f;

    [Header("Jump Apex")]
    [Range(0.5f, 1f)] public float ApexTrheshold = 0.95f;
    [Range(0.01f, 1f)] public float ApexHangTime = 0.07f;

    [Header("Jump Buffer")]
    [Range(0f, 1f)] public float JumpBufferTime = 0.130f;

    [Header("Jump Coyote Time")]
    [Range(0f, 1f)] public float JumpCoyoteTime = 0.1f;

    [Header("Hit Force")]
    [Range(0f, 10f)] public float HitForce;

    public float Gravity { get; private set; }
    public float InitialJumpVelocity { get; private set; }
    public float AdjustedJumpHeight { get; private set; }

    private void OnValidate()
    {
        CalculateVals();
    }

    private void OnEnable()
    {
        CalculateVals();
    }

    private void CalculateVals()
    {
        AdjustedJumpHeight = jumpHeight * JumpHeightCompensarionFactor;
        Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(timeToJumpApex, 2f);
        InitialJumpVelocity = Mathf.Abs(Gravity) * timeToJumpApex;
    }
}
