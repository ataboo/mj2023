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

    private HudControl _hudControl;

    private GameManager _gameManager;

    private LevelManager _levelManager;

    [Export]
    float walkBobScale = 0.4f;

    private Vector3 _startPos;

    [Export]
    private NodePath raycastPath;
    private RayCast _camRaycast;

    public override void _Ready()
    {
        _gameManager = GameManager.MustGetNode(this);
        this._startPos = this.Translation;
        this._playerControl = GetNode<PlayerControl>(playerControlPath) ?? throw new NullReferenceException();
        this._camera = GetNode<Camera>(cameraPath) ?? throw new NullReferenceException();
        _camRaycast = GetNode<RayCast>(raycastPath) ?? throw new NullReferenceException();
        
        _levelManager = LevelManager.MustGetNode(this) ?? throw new NullReferenceException();
        this._hudControl = _levelManager.HudControl ?? throw new NullReferenceException();

        _playerControl.Connect(nameof(PlayerControl.OnPhysicsDone), this, nameof(_HandleChassisPhysicsComplete));
        _camRaycast.CastTo = new Vector3(0, 0, -300);
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

    public Vector3 AimPoint() {
        if(!_camRaycast.IsColliding()) {
            return ToGlobal(Translation + _camRaycast.CastTo);
        }

        return _camRaycast.GetCollisionPoint();
    }

    public PizzaControl AimedPizza() {
        if(!_camRaycast.IsColliding()) {
            return null;
        }
        
        if(_camRaycast.GetCollider() is PizzaControl pizza) {
            return pizza;
        }

        return null;
    }
}
