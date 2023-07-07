using Godot;
using System;

public class PlayerControl : KinematicBody
{
    [Export]
    public float walkSpeed = 3f;

    [Export]
    public float strafeSpeed = 1f;

    [Export]
    public float walkAccel = 0.25f;

    [Export]
    public float rotationSpeed = 0.65f;

    [Export]
    public float walkCyclePeriod = 1f;

    [Export]
    public float shuffleCyclePeriod = 0.7f;

    private Vector3 gravity = Vector3.Down * 10;

    public MechState MechState => _state;
    private MechState _state;

    [Signal]
    public delegate void OnPhysicsDone();

    int debugCount = 0;

    public override void _Ready()
    {
        InitState();
    }

    public override void _PhysicsProcess(float delta)
    {
        ProcessMove(delta);

        // if(debugCount++ > 100) {
        //     debugCount = 0;
        //     GD.Print($"Fwd z: {_state.fwdVelocity.z}, Fwd x: {_state.fwdVelocity.x}, Speed: {_state.fwdSpeedSaturation}");
        // }

        EmitSignal(nameof(OnPhysicsDone), delta);
    }

    private void ProcessInput(float delta) {

        _state.walkInput = Vector2.Zero;
        _state.turnInput = 0;
        if(Input.IsActionPressed("turn_right")) {
            _state.turnInput += 1f;
        }
        if(Input.IsActionPressed("turn_left")) {
            _state.turnInput -= 1f;
        }
        if(Input.IsActionPressed("move_fwd")) {
            _state.walkInput += Vector2.Up;
        }
        if(Input.IsActionPressed("move_back")) {
            _state.walkInput += Vector2.Down;
        }
        if(Input.IsActionPressed("move_left")) {
            _state.walkInput += Vector2.Left;
        }
        if(Input.IsActionPressed("move_right")) {
            _state.walkInput += Vector2.Right;
        }

        _state.fwdVelocity = new Vector3(_state.walkInput.x * strafeSpeed, 0, _state.walkInput.y * walkSpeed).LimitLength(walkSpeed);

        var targetV = Transform.basis.Xform(_state.fwdVelocity);

        var originalVY = _state.velocity.y;
        _state.velocity = _state.velocity.LinearInterpolate(targetV, walkAccel);
        _state.velocity.y = originalVY;

        RotateY(-_state.turnInput * rotationSpeed * delta);
    }

    private void ProcessMove(float delta) {
        _state.velocity += gravity * delta;
        ProcessInput(delta);

        _state.velocity = MoveAndSlide(_state.velocity, Vector3.Up, true, 4, 0.785f, true);

        _state.fwdVelocity = Transform.basis.XformInv(_state.velocity);

        _state.walkSpeedSaturation = Mathf.Abs(_state.fwdVelocity.Length()) / walkSpeed;
        _state.shuffleSaturation = Mathf.Max(Mathf.Abs(_state.fwdVelocity.x) / strafeSpeed, Mathf.Abs(_state.turnInput));

        var stopped = _state.walkInput.Length() < 0.01f && Mathf.Abs(_state.turnInput) < 0.01f;
        if(stopped) {
            _state.shuffling = true;
            _state.walkCycleT = Mathf.Lerp(_state.walkCycleT, 1f, 2 * delta);
        } else {
            _state.shuffling = Mathf.Abs(_state.walkInput.y) < 0.01f;
        
            if(_state.shuffling) {
                _state.walkCycleT = (_state.walkCycleT + _state.shuffleSaturation * delta / shuffleCyclePeriod) % 1f;
            } else {
                _state.walkCycleT = (_state.walkCycleT + _state.walkSpeedSaturation * delta / walkCyclePeriod) % 1f;
            }
        }
    }

    private void InitState() {
        this._state = new MechState() {
            
        };
    }
}
