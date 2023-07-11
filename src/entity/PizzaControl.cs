using Godot;
using System;

public class PizzaControl : RigidBody
{
    private MeshInstance _sauceMesh;

    private Spatial _stickTarget;

    private float _maxSauceY = 0.15f;

    private float _sauceLevel = 0f;

    private uint _originalCollisionLayer = 1|1<<1|1<<2;

    private ARProgressGroupControl _progress;

    public override void _Ready()
    {
        _sauceMesh = GetNode<MeshInstance>("pizza/PizzaCrust/PizzaSauce") ?? throw new NullReferenceException();
        // _originalCollisionLayer = CollisionLayer;

        CallDeferred("InitProgressBars");
    }

    private void InitProgressBars()
    {
        var level = LevelManager.MustGetNode(this);
        _progress = level.ARHolder.CreateProgressBar(this, new[]{"Toppings", "Cheese", "Sauce"});
        _progress.SetTargetOffset(new Vector3(0, 1, 0));
    }

    public void SetSauceLevel(float t) {
        t = Mathf.Clamp(t, 0, 1);
        _sauceLevel = t;
        var sauceY = Mathf.Lerp(0, _maxSauceY, _sauceLevel);
        _sauceMesh.Translation = new Vector3(0, sauceY, 0);
    }

    public void SetRBActive(bool active, bool? collision = null) {
        if(active) {
            Mode = ModeEnum.Rigid;
        } else {
            Mode = ModeEnum.Kinematic;
        }

        CollisionLayer = collision ?? active ? _originalCollisionLayer : 0;
        CollisionMask = collision ?? active ? _originalCollisionLayer : 0;
    }
}
