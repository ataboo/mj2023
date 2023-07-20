using Godot;

public class DoughPileControl : RigidBody, IKickable
{
    ARProgressGroupControl _progress;

    public void Kick(Vector3 position, Vector3 direction)
    {
        ApplyImpulse(position, direction);
    }

    public override void _Ready()
    {
        CallDeferred(nameof(RemoveEmptyParent));
        CallDeferred(nameof(InitProgressBar));
    }

    public override void _Process(float delta)
    {
        if(Translation.y < -100) {
            if(_progress != null) {
                _progress.QueueFree();
                _progress = null;
            }
            QueueFree();
        }
    }

    private void InitProgressBar()
    {
        var level = LevelManager.MustGetNode(this);
        _progress = level.ARHolder.CreateProgressBar(this, new[]{"Kneaded"});
        _progress.SetProgresses(new []{1f});
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
