using Godot;

// Vector3s follow OGL convention of:
// -z => Forward
// x => Right
public class MechState {
    public Vector3 velocity;

    public Vector3 fwdVelocity;

    public Vector2 walkInput;
    
    public Vector2 lookTarget;

    public float turnInput;

    public float walkCycleT;

    public float walkSpeedSaturation;

    public bool shuffling;

    public float shuffleSaturation;

    public MechAbility activeAbility;

    public bool legWindUp;

    public bool legKick;

    public Vector3? queuedImpulse;
}