using Godot;
using System;

public class ChinookControl : RigidBody, IKickable
{
    public void Kick(Vector3 position, Vector3 direction)
    {
        ApplyImpulse(position, direction);
    }

    public override void _Process(float delta)
    {
        if(Translation.y < -100) {
            QueueFree();
        }
    }
}
