using System;
using Godot;

public class DoughballControl : RigidBody, IKickable
{
    [Export]
    public PackedScene doughPilePrefab;

    public Vector3 lastVelocity;
    public Vector3 currentVelocity;

    [Export]
    public float kickMultiplier = 1f;

    [Export]
    public float impactMultiplier = 5f;

    [Export]
    public float startHealth = 100f;

    private float _health;

    private ARProgressControl _progress;

    public void Kick(Vector3 position, Vector3 impulse)
    {
        this.ApplyImpulse(position, impulse);
        this.ApplyDamage(impulse.Length() / Mass, kickMultiplier, impulse);
    }

    public override void _Ready()
    {
        _health = startHealth;
        CallDeferred(nameof(RemoveEmptyParent));
        CallDeferred(nameof(InitProgressBar));
    }

    private void InitProgressBar()
    {
        var level = LevelManager.MustGetNode(this);
        _progress = level.ARHolder.CreateProgressBar(this);

        _progress.SetLabel("Gluten");
        _progress.SetProgress(0);
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

    public void _HandleBodyEntered(Node body)
    {
        this.ApplyDamage((currentVelocity - lastVelocity).Length(), impactMultiplier, null);
    }

    private void ApplyDamage(float damage, float multiplier, Vector3? pendingImpulse)
    {
        _health -= damage * multiplier;

        if (_health <= 0)
        {
            SpawnDoughPile(pendingImpulse);
            return;
        }

        if (_progress != null)
        {
            _progress.SetProgress((startHealth - _health) / startHealth);
        }
    }

    private void SpawnDoughPile(Vector3? pendingImpulse)
    {
        this.CollisionLayer = 0;
        this.CollisionMask = 0;

        var doughPileParent = doughPilePrefab.Instance<Spatial>();
        var doughPile = doughPileParent.GetChild<DoughPileControl>(0);

        doughPileParent.Translation = Translation + Vector3.Up * 1f;
        doughPile.LinearVelocity = LinearVelocity;

        GetParent().AddChild(doughPileParent);

        if (pendingImpulse != null)
        {
            doughPile.ApplyImpulse(Vector3.Zero, pendingImpulse.Value);
        }

        _progress.QueueFree();
        QueueFree();
    }

    public override void _IntegrateForces(PhysicsDirectBodyState state)
    {
        lastVelocity = currentVelocity;
        currentVelocity = state.LinearVelocity;
    }

    public void Punch(Vector3 impulse, float damage)
    {
        ApplyImpulse(Vector3.Zero, impulse);
        ApplyDamage(damage, kickMultiplier, impulse);
    }
}
