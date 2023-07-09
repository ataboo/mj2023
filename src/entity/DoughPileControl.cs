using Godot;
using System;

public class DoughPileControl : RigidBody, IKickable
{
    public void Kick(Vector3 position, Vector3 direction)
    {
        ApplyImpulse(position, direction);
    }

    public override void _Ready()
    {
        CallDeferred(nameof(RemoveEmptyParent));
    }

    private void RemoveEmptyParent()
    {
        var emptyParent = GetParent<Spatial>();
        emptyParent.RemoveChild(this);
        Transform = emptyParent.Transform;
        emptyParent.GetParent().AddChild(this);
        emptyParent.QueueFree();
    }
}
