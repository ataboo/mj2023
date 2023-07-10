using Godot;

// Vector3s follow OGL convention of:
// -z => Forward
// x => Right
public class AbilityState {
    public MechAbility activeAbility;

    public bool legWindUp;

    public bool legKick;
}