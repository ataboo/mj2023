using Godot;
using System;

public class PlateArmControl : Spatial
{
    [Export]
    public NodePath animationTreePath;
    private AnimationTree _tree;

    private AnimationNodeStateMachinePlayback _stateMachine;

    private AnimationPlayer _animationPlayer;

    [Export]
    NodePath abilityControlPath;
    private AbilityControl _abilityControl;

    [Export]
    PackedScene doughPilePrefab;

    [Export]
    PackedScene pizzaPrefab;

    private float _punchForce = 5f;
    private float _punchDamage = 5f;

    private PizzaControl _heldPizza;

    private bool _holdingDough;

    private bool HoldingItem => _heldPizza != null || _holdingDough;

    [Export]
    public NodePath spinDoughPath;
    private SpinDoughControl _spinDough;


    private LevelManager _levelManager;

    private ARHolderControl _arHolder;

    private Spatial _entityHolder;

    private Spatial _plate;

    private bool _spinning = false;
    private float _spinV = 0f;
    private float _spinDoughT = 0f;

    private float _spinAccel = 8f;

    private float _spinDeccel = 8f;

    private float _maxSpinSpeed = 100f;

    private ARProgressGroupControl _spinProgressBar;

    private Vector2 _stretchSpeedRange = new Vector2(10f, 80f);
    private Vector2 _stretchRate = new Vector2(.1f, 0.3f);

    private bool _droppingItem;

    public override void _Ready()
    {
        _tree = GetNode<AnimationTree>(animationTreePath) ?? throw new NullReferenceException();
        _stateMachine = (AnimationNodeStateMachinePlayback)_tree.Get("parameters/playback") ?? throw new NullReferenceException();
        _animationPlayer = _tree.GetNode<AnimationPlayer>(_tree.AnimPlayer) ?? throw new NullReferenceException();
        _abilityControl = GetNode<AbilityControl>(abilityControlPath) ?? throw new NullReferenceException();
        _spinDough = GetNode<SpinDoughControl>(spinDoughPath) ?? throw new NullReferenceException();
        _plate = _spinDough.GetParent<Spatial>() ?? throw new NullReferenceException();
        _levelManager = LevelManager.MustGetNode(this) ?? throw new NullReferenceException();
        _arHolder = _levelManager.ARHolder ?? throw new NullReferenceException();
        _entityHolder = _levelManager.EntityHolder ?? throw new NullReferenceException();

        _abilityControl.Connect(nameof(AbilityControl.OnAbilityChange), this, nameof(_HandleAbilityChanged));
        _abilityControl.Connect(nameof(AbilityControl.OnClick), this, nameof(_HandleClick));

        _spinDough.Visible = false;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (_spinning)
        {
            _spinV += _spinAccel * delta;
        }
        else
        {
            _spinV -= _spinDeccel * delta;
        }
        _spinV = Mathf.Clamp(_spinV, 0, _maxSpinSpeed);

        _plate.Rotate(Vector3.Forward, -_spinV * delta);

        if (_holdingDough && _spinV > _stretchSpeedRange.x)
        {
            var stretchWeight = Mathf.Clamp(_spinV / (_stretchSpeedRange.y - _stretchSpeedRange.x), 0, 1);
            var stretchRate = Mathf.Lerp(_stretchRate.x, _stretchRate.y, stretchWeight);
            _spinDoughT = Mathf.Clamp(_spinDoughT + stretchRate * delta, 0, 1);
            ShowSpinProgress();
        }

        if (_holdingDough && _heldPizza == null && _spinDoughT == 1)
        {
            HideSpinDough();

            _heldPizza = pizzaPrefab.Instance<PizzaControl>();
            AddPizzaToPlate();
            _spinDoughT = 0;
            _stateMachine.Travel("PlateIdle");
        }
    }

    private void _HandleAbilityChanged(MechAbility ability)
    {
        if (ability == MechAbility.Plate)
        {
            _stateMachine.Travel("PlateIdle");
        }
        else
        {
            _stateMachine.Travel("PlateStashed");
        }
    }

    private void _HandleClick(bool isPressed, bool leftClick)
    {
        if (_abilityControl.AbilityState.activeAbility != MechAbility.Plate)
        {
            return;
        }

        if (HoldingItem)
        {
            if (leftClick)
            {
                _spinning = isPressed;
            }
            else if (isPressed)
            {
                _droppingItem = true;
                _stateMachine.Travel("PlateExtend");
            }
            return;

        }

        if (leftClick && isPressed)
        {
            _stateMachine.Travel("PlateExtend");
            return;
        }
    }

    public void _HandlePlateDetectorAreaEntered(Area area) {
        if (_stateMachine.GetCurrentNode() != "PlateExtend")
        {
            return;
        }
        
        if(area is StickTargetControl stickTarget) {
            if(_heldPizza != null && _droppingItem) {
                _plate.RemoveChild(_heldPizza);
                stickTarget.AddChild(_heldPizza);
                _heldPizza.GlobalTranslation = _plate.GlobalTranslation;
                _heldPizza.Translation = new Vector3(0, _heldPizza.Translation.y, _heldPizza.Translation.z);
                _heldPizza.Rotation = new Vector3(0, 0, -Mathf.Pi/2);
                _heldPizza.SetRBActive(false, true);
                _heldPizza = null;
            }
        }
    }

    public void _HandlePlateDetectorBodyEntered(Node body)
    { 
        if(body is PlayerControl) {
            return;
        }

        if(_stateMachine.GetCurrentNode() == "PlateIdle" && _droppingItem) {
            _droppingItem = false;
        }

        if (_stateMachine.GetCurrentNode() != "PlateExtend")
        {
            return;
        }

        _stateMachine.Travel("PlateIdle");

        if (_droppingItem)
        {
            _abilityControl.SetChangeLock(false);
            _stateMachine.Travel("PlateIdle");
            _droppingItem = false;

            if (_holdingDough)
            {
                HideSpinDough();
                var instance = doughPilePrefab.Instance<Spatial>();
                _entityHolder.AddChild(instance);
                instance.Translation = _plate.GlobalTranslation;

            }
            else if (_heldPizza != null)
            {
                _plate.RemoveChild(_heldPizza);
                _entityHolder.AddChild(_heldPizza);
                _heldPizza.Translation = _plate.GlobalTranslation;
                _heldPizza.SetRBActive(true);
                _heldPizza = null;
            }

            return;
        }

        if (body is DoughballControl doughBall)
        {
            doughBall.Punch(GlobalTransform.basis.z * _punchForce, _punchDamage);
        }
        else if (body is DoughPileControl doughPile)
        {
            doughPile.QueueFree();
            _stateMachine.Travel("PlateLook");

            _holdingDough = true;
            _spinDough.Visible = true;
            _spinV = 0;
            _spinProgressBar = _arHolder.CreateProgressBar(_spinDough, new[] { "Stretch" });
            _spinDoughT = 0f;
            ShowSpinProgress();

            _abilityControl.SetChangeLock(true);
        }
        else if (body is PizzaControl pizza)
        {
            if(_heldPizza != null) {
                return;
            }

            _heldPizza = pizza;
            _stateMachine.Travel("PlateLook");
            _abilityControl.SetChangeLock(true);

            AddPizzaToPlate();
        }
    }

    private void HideSpinDough()
    {
        _spinDough.Visible = false;
        _holdingDough = false;

        if (_spinProgressBar != null)
        {
            _spinProgressBar.QueueFree();
            _spinProgressBar = null;
        }
    }

    private void AddPizzaToPlate()
    {
        var parent = _heldPizza.GetParent();
        if (parent != null)
        {
            parent.RemoveChild(_heldPizza);
        }
        _plate.AddChild(_heldPizza);
        _heldPizza.Translation = Vector3.Zero;
        _heldPizza.Rotation = Vector3.Zero;
        _heldPizza.SetRBActive(false);
    }

    private void ShowSpinProgress()
    {
        _spinDough.SetStretchProgress(_spinDoughT);
        if (_spinProgressBar != null)
        {
            _spinProgressBar.SetProgresses(new[] { _spinDoughT });
        }
    }
}
