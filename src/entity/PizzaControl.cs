using Godot;
using System;
using System.Collections.Generic;

public class PizzaControl : RigidBody, ICookable
{
    private MeshInstance _sauceMesh;

    private Spatial _stickTarget;

    private float _maxSauceY = 0.15f;

    private float _roniHeight = 0.2f;

    private float _sauceLevel = 0f;

    private uint _originalCollisionLayer = 7; //(1 << 0  | 1 << 1 | 1 << 2)

    private ARProgressGroupControl _progress;


    private int _roniCount = 0;

    private int _cheeseCount = 0;

    [Export]
    private int roniQuota = 10;

    [Export]
    private int cheeseQuota = 10;

    private List<ICookable> _cookables = new List<ICookable>();

    [Export]
    private float saucePerGlob = 0.1f;

    private CookedState _cookedState;
    public CookedState CookedState
    {
        get => _cookedState; set
        {
            SetCooked(value);
        }
    }

    public override void _Ready()
    {
        _sauceMesh = GetNode<MeshInstance>("pizza/PizzaCrust/PizzaSauce") ?? throw new NullReferenceException();
        CallDeferred("InitProgressBars");
    }

    private void InitProgressBars()
    {
        var level = LevelManager.MustGetNode(this);
        _progress = level.ARHolder.CreateProgressBar(this, new[] { "Toppings", "Cheese", "Sauce" });
        _progress.SetTargetOffset(new Vector3(0, 1, 0));
    }

    public void SetSauceLevel(float t)
    {
        t = Mathf.Clamp(t, 0, 1);
        _sauceLevel = t;
        var sauceY = Mathf.Lerp(0, _maxSauceY, _sauceLevel);
        _sauceMesh.Translation = new Vector3(0, sauceY, 0);
    }

    public void SetRBActive(bool active, bool? collision = null)
    {
        if (active)
        {
            Mode = ModeEnum.Rigid;
        }
        else
        {
            Mode = ModeEnum.Static;
        }

        CollisionLayer = collision ?? active ? _originalCollisionLayer : 0;
        CollisionMask = collision ?? active ? _originalCollisionLayer : 0;
    }

    private void SetCooked(CookedState cooked)
    {
        _cookedState = cooked;

        foreach(var cookable in _cookables) {
            cookable.CookedState = cooked;
        }
    }

    public void _HandleBodyEnteredToppingDetector(Node body)
    {
        if (body is PepperoniControl pc)
        {
            AddRoniChild(pc);
            return;
        }

        if(body is SauceGlobControl sauceGlob) {
            AddSauce(sauceGlob);
            return;
        }

        // if(body is CheeseBallControl cheeseBall) {
        //     //TODO add cheese ball

        //     return;
        // }
    }

    private void AddRoniChild(PepperoniControl pc) {
        if (pc.GetParent() == _sauceMesh)
        {
            // Sometimes this will double-fire when it's in the middle of handling.
            return;
        }

        var oldTrans = pc.Translation;

        pc.Mode = ModeEnum.Static;
        pc.LinearVelocity = Vector3.Zero;
        pc.CollisionLayer = 0;
        pc.CollisionMask = 0;

        pc.AxisLockAngularX = true;
        pc.AxisLockAngularY = true;
        pc.AxisLockAngularZ = true;
        pc.AxisLockLinearX = true;
        pc.AxisLockLinearY = true;
        pc.AxisLockLinearZ = true;

        pc.CancelDeath();

        pc.GetParent().RemoveChild(pc);
        _sauceMesh.AddChild(pc);
        pc.GlobalTranslation = oldTrans;
        pc.Rotation = new Vector3(0, pc.Rotation.y, 0);
        pc.Translation = new Vector3(pc.Translation.x, _roniHeight, pc.Translation.z);

        _roniCount++;
        
        _cookables.Add(pc);

        UpdateProgress();
    }

    private void AddSauce(SauceGlobControl sauce) {
        this.SetSauceLevel(this._sauceLevel + saucePerGlob);
        sauce.CollisionLayer = 0;
        sauce.CollisionMask = 0;
        sauce.QueueFree();

        UpdateProgress();
    }

    // private void AddCheese(CheeseBallControl cheese) {

    // }

    private void UpdateProgress()
    {
        var roniProgress = Mathf.Clamp((float)_roniCount / roniQuota, 0f, 1f);
        var cheeseProgress = Mathf.Clamp((float)_cheeseCount / cheeseQuota, 0f, 1f);

        _progress.SetProgresses(new[] { roniProgress, cheeseProgress, _sauceLevel });
    }
}
