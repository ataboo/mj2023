using Godot;

public class DoughPileControl : RigidBody, IKickable
{
    ARProgressControl _progress;

    public void Kick(Vector3 position, Vector3 direction)
    {
        ApplyImpulse(position, direction);
    }

    public override void _Ready()
    {
        CallDeferred(nameof(RemoveEmptyParent));
        CallDeferred(nameof(InitProgressBar));
    }

    private void InitProgressBar()
    {
        var level = LevelManager.MustGetNode(this);
        _progress = level.ARHolder.CreateProgressBar(this);

        _progress.SetLabel("Kneaded");
        _progress.SetProgress(1);
        _progress.SetTargetOffset(new Vector3(0, 1, 0));
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
