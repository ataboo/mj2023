using Godot;
using System;
using System.Collections.Generic;
using static MechGameConstants;

public class PizzaControl : RigidBody, ICookable
{
    private MeshInstance _sauceMesh;

    private Spatial _stickTarget;

    private float _maxSauceY = 0.15f;

    private float _roniHeight = 0.25f;

    private float _cheeseHeight = 0.23f;

    private uint _originalCollisionLayer = 7; //(1 << 0  | 1 << 1 | 1 << 2)

    private ARProgressGroupControl _progress;

    private int _roniCount = 0;

    private int _cheeseCount = 0;

    private List<ICookable> _cookables = new List<ICookable>();

    private float _cookProgress = 0f;

    [Export]
    private float saucePerGlob = 0.1f;

    [Export]
    public PackedScene cheesePrefab;

    private MeshInstance _crustMesh;

    [Export]
    private Color rawCrustColor = Colors.White;

    [Export]
    private Color cookedCrustColor = Colors.SandyBrown;

    [Export]
    private Color burntCrustColor = new Color("#29251e");

    [Export]
    private Color rawSauceColor = Colors.Red;

    [Export]
    private Color cookedSauceColor = Colors.DarkRed;

    [Export]
    private Color burntSauceColor = new Color("#29251e");

    private float _cookRate = 0f;

    private SpatialMaterial _crustMaterial;

    private SpatialMaterial _sauceMaterial;

    private PizzaState _pizzaState;
    public PizzaState PizzaState => _pizzaState;

    public CookedState CookedState
    {
        get => _pizzaState.cookedState; set
        {
            SetCooked(value);
        }
    }

    private Random _rng = new Random();

    public override void _Ready()
    {
        _pizzaState = new PizzaState(){cookedState = CookedState.Raw};
        _sauceMesh = GetNode<MeshInstance>("pizza/PizzaCrust/PizzaSauce") ?? throw new NullReferenceException();
        _crustMesh = GetNode<MeshInstance>("pizza/PizzaCrust") ?? throw new NullReferenceException();

        _sauceMaterial = _sauceMesh.GetActiveMaterial(0) as SpatialMaterial ?? throw new NullReferenceException();
        _crustMaterial = _crustMesh.GetActiveMaterial(0) as SpatialMaterial ?? throw new NullReferenceException();

        CallDeferred("InitProgressBars");
    }

    public override void _Process(float delta)
    {
        if(_cookRate > 0) {
            if(CookedState == CookedState.Raw) {
                SetCooked(CookedState.Cooking);
                _progress.SetBarCount(1);
                _progress.SetTargetOffset(Vector3.Zero);
                _progress.SetLabels(new[]{"Cooking"});
            }

            _cookProgress += delta * _cookRate;

            switch(CookedState) {
                case CookedState.Raw:
                    SetCooked(CookedState.Cooking);
                    _progress.SetBarCount(1);
                    _progress.SetTargetOffset(Vector3.Zero);
                    _progress.SetLabels(new[]{"Cooking"});
                    break;
                case CookedState.Cooking:
                    if(_cookProgress >= 1f) {
                        _progress.SetLabels(new[]{"Cooked"});
                        SetCooked(CookedState.Cooked);
                    }
                    break;
                case CookedState.Cooked:
                    if(_cookProgress > 1.5f) {
                        _progress.SetLabels(new[]{"Burnt"});
                        SetCooked(CookedState.Burned);
                    }
                    break;
            }

            UpdateProgress();
        }
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
        _pizzaState.sauceLevel = t;
        var sauceY = Mathf.Lerp(0, _maxSauceY, _pizzaState.sauceLevel);
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
        _pizzaState.cookedState = cooked;

        switch(cooked) {
            case CookedState.Raw:
                _crustMaterial.AlbedoColor = rawCrustColor;
                _sauceMaterial.AlbedoColor = rawSauceColor;
                break;
            case CookedState.Cooked:
                _crustMaterial.AlbedoColor = cookedCrustColor;
                _sauceMaterial.AlbedoColor = cookedSauceColor;
                break;
            case CookedState.Burned:
                _crustMaterial.AlbedoColor = burntCrustColor;
                _sauceMaterial.AlbedoColor = burntSauceColor;
                break;
        }

        foreach(var cookable in _cookables) {
            cookable.CookedState = cooked;
        }
    }

    public void _HandleBodyEnteredToppingDetector(Node body)
    {
        if(CookedState != CookedState.Raw) {
            return;
        }

        if (body is PepperoniControl pc)
        {
            AddRoniChild(pc);
            return;
        }

        if(body is SauceGlobControl sauceGlob) {
            AddSauce(sauceGlob);
            return;
        }
    }

    public void _HandleAreaEnteredToppingDetector(Area area) {
        if(area is OvenControl oven) {
            _cookRate = oven.CookRate;
            oven.CookingActive(true);
        }
    } 

    public void _HandleAreaExitedToppingDetector(Area area) {
        if(area is OvenControl oven) {
            _cookRate = 0;
            oven.CookingActive(false);
        }
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
        _pizzaState.roniLevel = (float)_roniCount / RoniQuota;
        
        _cookables.Add(pc);

        UpdateProgress();
    }

    private void AddSauce(SauceGlobControl sauce) {
        this.SetSauceLevel(_pizzaState.sauceLevel + saucePerGlob);
        sauce.CollisionLayer = 0;
        sauce.CollisionMask = 0;
        sauce.QueueFree();

        UpdateProgress();
    }

    public void AddCheese() {
        if(CookedState != CookedState.Raw) {
            return;
        }

        if(_cheeseCount > MechGameConstants.CheeseQuota) {
            return;
        }

        var newCheese = cheesePrefab.Instance<CheeseMeshControl>();
        _sauceMesh.AddChild(newCheese);
        var randPos = MechGameHelpers.RandomPointInCircle(_rng, Vector2.Zero, 1.8f);
        newCheese.Translation = new Vector3(randPos.x, _cheeseHeight, randPos.y);
        var scale = Mathf.Lerp(0.2f, 1.2f, (float)_rng.NextDouble());
        newCheese.Scale = new Vector3(scale, scale, scale);
        newCheese.Rotation = new Vector3(-Mathf.Pi/2, 0, 0);
        newCheese.RotateY((float)_rng.NextDouble() * Mathf.Pi * 2);

        _cookables.Add(newCheese);

        _cheeseCount++;
        _pizzaState.cheeseLevel = (float)_cheeseCount / CheeseQuota;

        UpdateProgress();
    }

    private void UpdateProgress()
    {
        if(CookedState != CookedState.Raw) {
            _progress.SetProgresses(new []{_cookProgress});
            return;
        }

        _progress.SetProgresses(new[] { _pizzaState.roniLevel, _pizzaState.cheeseLevel, _pizzaState.sauceLevel });
    }
}
