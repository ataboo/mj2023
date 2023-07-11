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

    private float _punchForce = 5f;
    private float _punchDamage = 5f;

    private bool _holdingDough = false;

    [Export]
    public NodePath spinDoughPath;
    private SpinDoughControl _spinDough;

    private LevelManager _levelManager;

    private ARHolderControl _arHolder;

    private Spatial _plate;

    private bool _spinning = false;
    private float _spinV = 0f;
    private float _spinDoughT = 0f;

    private float _spinAccel = 8f;

    private float _spinDeccel = 8f;

    private float _maxSpinSpeed = 100f;

    private ARProgressControl _spinProgressBar;

    private Vector2 _stretchSpeedRange = new Vector2(10f, 80f);
    private Vector2 _stretchRate = new Vector2(.1f, 0.3f);

    public override void _Ready()
    {
        _tree = GetNode<AnimationTree>(animationTreePath) ?? throw new NullReferenceException();
        _stateMachine = (AnimationNodeStateMachinePlayback)_tree.Get("parameters/playback") ?? throw new NullReferenceException();
        _animationPlayer = _tree.GetNode<AnimationPlayer>(_tree.AnimPlayer) ?? throw new NullReferenceException();
        _abilityControl = GetNode<AbilityControl>(abilityControlPath) ?? throw new NullReferenceException();
        _spinDough = GetNode<SpinDoughControl>(spinDoughPath) ?? throw new NullReferenceException();
        _plate = _spinDough.GetParent<Spatial>() ?? throw new NullReferenceException();
        _levelManager = LevelManager.MustGetNode(this) ?? throw new NullReferenceException();
        _arHolder = _levelManager.ARHolder;

        _abilityControl.Connect(nameof(AbilityControl.OnAbilityChange), this, nameof(_HandleAbilityChanged));
        _abilityControl.Connect(nameof(AbilityControl.OnClick), this, nameof(_HandleClick));

        _spinDough.Visible = false;
    }

    public override void _PhysicsProcess(float delta)
    {
        if(_spinning) {
            _spinV += _spinAccel * delta;
        } else {
            _spinV -= _spinDeccel * delta;
        }
        _spinV = Mathf.Clamp(_spinV, 0, _maxSpinSpeed);

        _plate.Rotate(Vector3.Forward, -_spinV * delta);

        if(_spinV > _stretchSpeedRange.x) {
            var stretchWeight = Mathf.Clamp(_spinV / (_stretchSpeedRange.y - _stretchSpeedRange.x), 0, 1);
            var stretchRate = Mathf.Lerp(_stretchRate.x, _stretchRate.y, stretchWeight);
            _spinDoughT = Mathf.Clamp(_spinDoughT + stretchRate * delta, 0, 1);
            ShowSpinProgress();
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

        if(_holdingDough) {
            if(leftClick) {
                _spinning = isPressed;
            } else if(isPressed) {
                _holdingDough = false;
                _abilityControl.SetChangeLock(false);
                _stateMachine.Travel("PlateIdle");

                _spinDough.Visible = false;
                _spinProgressBar.QueueFree();
                _spinProgressBar = null;
                
                var instance = doughPilePrefab.Instance<Spatial>();
                LevelManager.MustGetNode(this).AddChild(instance);
                instance.Translation = _abilityControl.GlobalTranslation + -_abilityControl.GlobalTransform.basis.z * 5;
            }
            return;

        }

        if (leftClick && isPressed)
        {
            _stateMachine.Travel("PlateExtend");
            return;
        }
    }

    public void _HandlePlateDetectorBodyEntered(Node body)
    {
        if (_stateMachine.GetCurrentNode() != "PlateExtend")
        {
            return;
        }

        _stateMachine.Travel("PlateIdle");

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
            _spinProgressBar = _arHolder.CreateProgressBar(_spinDough);
            _spinProgressBar.SetLabel("Stretch");
            _spinDoughT = 0f;
            ShowSpinProgress();
            
            _abilityControl.SetChangeLock(true);
        }
    }

    private void ShowSpinProgress() {
        _spinDough.SetStretchProgress(_spinDoughT);
        if(_spinProgressBar != null) {
            _spinProgressBar.SetProgress(_spinDoughT);
            if(_spinDoughT == 1) {
                _spinProgressBar.SetLabel("Crust");
            }
        }
    }
}
