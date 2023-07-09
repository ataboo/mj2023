using Godot;
using System;

public class CockpitControl : Spatial
{
    [Export]
    public NodePath playerControlPath;
    private PlayerControl _playerControl;

    [Export]
    public NodePath cameraPath;
    private Camera _camera;

    [Export]
    public NodePath hudPath;
    private HudControl _hudControl;

    private GameManager _gameManager;

    [Export]
    float walkBobScale = 0.2f;

    private Vector3 _startPos;

    public override void _Ready()
    {
        _gameManager = GameManager.MustGetNode(this);
        this._startPos = this.Translation;
        this._playerControl = GetNode<PlayerControl>(playerControlPath) ?? throw new NullReferenceException();
        this._camera = GetNode<Camera>(cameraPath) ?? throw new NullReferenceException();
        this._hudControl = GetNode<HudControl>(hudPath) ?? throw new NullReferenceException();

        _playerControl.Connect(nameof(PlayerControl.OnPhysicsDone), this, nameof(_HandleChassisPhysicsComplete));
    }

    public void _HandleChassisPhysicsComplete(float delta) {
        if(!_gameManager.UserConfig.headBobActive) {
            Translation = Vector3.Zero;
            return;
        }
        var mechState = _playerControl.MechState;
        var bobScale = mechState.shuffling ? walkBobScale / 3 : walkBobScale;
        var targetPos = new Vector3(Mathf.Cos(mechState.walkCycleT * Mathf.Pi*2) * bobScale, Mathf.Cos(mechState.walkCycleT * Mathf.Pi * 4) * bobScale, 0) + _startPos;

        Translation = Translation.LinearInterpolate(targetPos, delta * 10f);
                  
        Rotation = new Vector3(0, mechState.lookTarget.x, 0);
        RotateObjectLocal(Vector3.Right, mechState.lookTarget.y);
    }

}
