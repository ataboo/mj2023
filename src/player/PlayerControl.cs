using Godot;
using System;

public class PlayerControl : KinematicBody
{
    [Export]
    public float walkSpeed = 3f;

    [Export]
    public float walkAccel = 0.25f;

    [Export]
    public float rotationSpeed = 0.85f;

    private Vector3 gravity = Vector3.Down * 10;

    private MechState _state;

    public override void _Ready()
    {
        InitState();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }

    public override void _PhysicsProcess(float delta)
    {
        ProcessMove(delta);
    }

    private void ProcessInput(float delta) {
        var originalVY = _state.velocity.y;

        var targetV = Vector3.Zero;

        _state.walkInput = Vector2.Zero;
        if(Input.IsActionPressed("turn_right")) {
            RotateY(-rotationSpeed * delta);
        }
        if(Input.IsActionPressed("turn_left")) {
            RotateY(rotationSpeed * delta);
        }
        if(Input.IsActionPressed("move_fwd")) {
            _state.walkInput += Vector2.Up;
            targetV = -Transform.basis.z * walkSpeed;
        }
        if(Input.IsActionPressed("move_back")) {
            _state.walkInput += Vector2.Down;
            targetV = Transform.basis.z * walkSpeed;
        }
        if(Input.IsActionPressed("move_left")) {
            _state.walkInput += Vector2.Left;
            targetV = -Transform.basis.x * walkSpeed;
        }
        if(Input.IsActionPressed("move_right")) {
            _state.walkInput += Vector2.Right;
            targetV = Transform.basis.x * walkSpeed;
        }

        _state.velocity = _state.velocity.LinearInterpolate(targetV, walkAccel);

        _state.velocity.y = originalVY;
    }

    private void ProcessMove(float delta) {
        _state.velocity += gravity * delta;
        ProcessInput(delta);

        _state.velocity = MoveAndSlide(_state.velocity, Vector3.Up, true, 4, 0.785f, true);

    }

    private void InitState() {
        this._state = new MechState() {
            velocity = Vector3.Zero,
            walkInput = Vector2.Zero,
        };
    }
}
