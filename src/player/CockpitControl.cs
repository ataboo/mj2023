using Godot;
using System;

public class CockpitControl : Spatial
{
    [Export]
    public NodePath playerControlPath;
    private PlayerControl _playerControl;

    [Export]
    float walkBobScale = 0.2f;

    private Vector3 _startPos;

    public override void _Ready()
    {
        this._startPos = this.Translation;
        this._playerControl = GetNode<PlayerControl>(playerControlPath) ?? throw new NullReferenceException();

        _playerControl.Connect(nameof(PlayerControl.OnPhysicsDone), this, nameof(_HandleChassisPhysicsComplete));
    }

    public void _HandleChassisPhysicsComplete(float delta) {
        var walkCycleT = _playerControl.MechState.walkCycleT;
        var bobScale = _playerControl.MechState.shuffling ? walkBobScale / 3 : walkBobScale;
        var targetPos = new Vector3(0, Mathf.Cos(walkCycleT * Mathf.Pi * 2) * bobScale, 0);

        Translation = Translation.LinearInterpolate(targetPos, delta * 10f);
    }

}
