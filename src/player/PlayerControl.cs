using System;
using Godot;

public class PlayerControl : KinematicBody
{
    [Export]
    public float mass = 20f;

    [Export]
    public float walkSpeed = 3f;

    [Export]
    public float strafeSpeed = 1f;

    [Export]
    public float walkAccel = 0.25f;

    [Export]
    public float rotationSpeed = 0.65f;

    [Export]
    public float walkCyclePeriod = 2f;

    [Export]
    public float shuffleCyclePeriod = 1.4f;

    [Export]
    public float mouseSensitivity = .001f;

    [Export]
    private float lookLimitX = Mathf.Pi * 7 / 8;

    [Export]
    private float lookLimitY = Mathf.Pi / 4;

    private Vector3 _startingPos;

    [Export]
    private NodePath abilityControlPath;
    private AbilityControl _abilityControl;
    public AbilityControl AbilityControl {
        get {
            if(_abilityControl == null) {
                _abilityControl = GetNode<AbilityControl>(abilityControlPath) ?? throw new NullReferenceException();
            }

            return _abilityControl;
        }
    }

    private Vector3 gravity = Vector3.Down * 10;

    public MechState MechState => _state;
    private MechState _state;

    [Signal]
    public delegate void OnPhysicsDone(float delta);

    private GameManager _gameManager;

    private LevelManager _levelManager;

    [Export]
    private NodePath cameraPath;
    private Camera _camera;
    public Camera Camera {
        get {
            if(_camera == null) {
                _camera = GetNode<Camera>(cameraPath) ?? throw new NullReferenceException();
            }

            return _camera;
        }
    }

    public override void _Ready()
    {
        _gameManager = GameManager.MustGetNode(this);
        _levelManager = LevelManager.MustGetNode(this);

        Input.MouseMode = Input.MouseModeEnum.Captured;
        InitState();

        _startingPos = Translation;
    }

    public override void _PhysicsProcess(float delta)
    {
        ProcessMove(delta);

        if(Translation.y < -100) {
            Translation = _startingPos;
        }

        EmitSignal(nameof(OnPhysicsDone), delta);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            _state.lookTarget.x = Mathf.Clamp(_state.lookTarget.x - eventMouseMotion.Relative.x * mouseSensitivity, -lookLimitX, lookLimitX);
            _state.lookTarget.y = Mathf.Clamp(_state.lookTarget.y - eventMouseMotion.Relative.y * mouseSensitivity, -lookLimitY, lookLimitY);
        }
    }

    public void QueueImpulse(Vector3 worldImpulse) {
        MechState.queuedImpulse = worldImpulse;
    }

    public void SetMoveLocked(bool moveLocked) {
        MechState.moveLocked = moveLocked;
    }

    private void ProcessInput(float delta)
    {
        _state.walkInput = Vector2.Zero;
        _state.turnInput = 0;
        if (Input.IsActionPressed("turn_right"))
        {
            _state.turnInput += 1f;
        }
        if (Input.IsActionPressed("turn_left"))
        {
            _state.turnInput -= 1f;
        }
        if (Input.IsActionPressed("move_fwd"))
        {
            _state.walkInput += Vector2.Up;
        }
        if (Input.IsActionPressed("move_back"))
        {
            _state.walkInput += Vector2.Down;
        }
        if (Input.IsActionPressed("move_left"))
        {
            _state.walkInput += Vector2.Left;
        }
        if (Input.IsActionPressed("move_right"))
        {
            _state.walkInput += Vector2.Right;
        }

        _state.fwdVelocity = new Vector3(_state.walkInput.x * strafeSpeed, 0, _state.walkInput.y * walkSpeed).LimitLength(walkSpeed);

        var targetV = Transform.basis.Xform(_state.fwdVelocity);

        if(MechState.moveLocked) {
            targetV = Vector3.Zero;
        }

        var originalVY = _state.velocity.y;
        _state.velocity = _state.velocity.LinearInterpolate(targetV, walkAccel);
        _state.velocity.y = originalVY;

        RotateY(-_state.turnInput * rotationSpeed * delta);
    }

    private void ProcessMove(float delta)
    {
        _state.velocity += gravity * delta;

        if(IsOnFloor()) {
            ProcessInput(delta);
        }

        if(MechState.queuedImpulse != null) {
            _state.velocity += MechState.queuedImpulse.Value/mass;
            MechState.queuedImpulse = null;
        }

        _state.velocity = MoveAndSlide(_state.velocity, Vector3.Up, true, 4, 0.785f, true);

        _state.fwdVelocity = Transform.basis.XformInv(_state.velocity);

        _state.walkSpeedSaturation = Mathf.Abs(_state.fwdVelocity.Length()) / walkSpeed;
        _state.shuffleSaturation = Mathf.Max(Mathf.Abs(_state.fwdVelocity.x) / strafeSpeed, Mathf.Abs(_state.turnInput));

        var stopped = _state.walkInput.Length() < 0.01f && Mathf.Abs(_state.turnInput) < 0.01f;
        if (stopped)
        {
            _state.shuffling = true;
            _state.walkCycleT = Mathf.Lerp(_state.walkCycleT, 1f, 2 * delta);
        }
        else
        {
            _state.shuffling = Mathf.Abs(_state.walkInput.y) < 0.01f;

            if (_state.shuffling)
            {
                _state.walkCycleT = (_state.walkCycleT + _state.shuffleSaturation * delta / shuffleCyclePeriod) % 1f;
            }
            else
            {
                _state.walkCycleT = (_state.walkCycleT + _state.walkSpeedSaturation * delta / walkCyclePeriod) % 1f;
            }
        }
    }

    private void InitState()
    {
        this._state = new MechState()
        {

        };
    }
}
