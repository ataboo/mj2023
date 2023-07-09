using Godot;

public class DoughballControl : RigidBody, IKickable
{
    [Export]
    public PackedScene doughPilePrefab;

    public void Kick(Vector3 position, Vector3 impulse)
    {
        this.ApplyImpulse(position, impulse);
    }

    public override void _Ready()
    {
        CallDeferred(nameof(RemoveEmptyParent));   
    }

    private void RemoveEmptyParent() {
        var emptyParent = GetParent<Spatial>();
        emptyParent.RemoveChild(this);
        Transform = emptyParent.Transform;
        emptyParent.GetParent().AddChild(this);
        emptyParent.QueueFree();
    }

    public void _HandleBodyEntered(Node body) {
        if(body is CollisionObject physicsBody) {
            GD.Print(physicsBody.GetColliderVelocity());
        }
    }

    // public override void _PhysicsProcess(float delta)
    // {
    //     foreach(var body in GetCollidingBodies()) {
    //         GD.Print(body);
    //     }
    // }

}
