using Godot;
using System;

public class SauceGlobControl : RigidBody
{

    private bool _hit = false;

    public override void _Ready()
    {
        
    }

    public void Fire(Vector3 velocity) {
        LinearVelocity = velocity;
    }

    public override void _PhysicsProcess(float delta)
    {
        if(_hit) {
            return;
        }

        // _velocity += new Vector3(0, -_gravityAccel * delta, 0);
        // Translate(_velocity * delta);

        if(Translation.y < -100) {
            QueueFree();
        }
    }

    public void _HandleBodyEntered(Node node) {
        LinearVelocity = Vector3.Zero;
        AngularVelocity = Vector3.Zero;
        // Mode = ModeEnum.Static;
        _hit = true;
        Scale = new Vector3(3, 0.3f, 3f);
    }
}
