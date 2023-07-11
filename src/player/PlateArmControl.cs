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

    public override void _Ready()
    {
        _tree = GetNode<AnimationTree>(animationTreePath) ?? throw new NullReferenceException();
        _stateMachine = (AnimationNodeStateMachinePlayback)_tree.Get("parameters/playback") ?? throw new NullReferenceException();
        _animationPlayer = _tree.GetNode<AnimationPlayer>(_tree.AnimPlayer) ?? throw new NullReferenceException();
        _abilityControl = GetNode<AbilityControl>(abilityControlPath) ?? throw new NullReferenceException();

        _abilityControl.Connect(nameof(AbilityControl.OnAbilityChange), this, nameof(_HandleAbilityChanged));
        _abilityControl.Connect(nameof(AbilityControl.OnClick), this, nameof(_HandleClick));
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
        if (_abilityControl.AbilityState.activeAbility != MechAbility.Plate || !isPressed)
        {
            return;
        }

        if (leftClick)
        {
            if (_holdingDough)
            {
                //TODO spin dough
            }
            else
            {
                _stateMachine.Travel("PlateExtend");
            }
        }
        else
        {
            if (_holdingDough)
            {
                //TODO drop dough/pizza
                _holdingDough = false;
                _abilityControl.SetChangeLock(false);
                _stateMachine.Travel("PlateIdle");

                var instance = doughPilePrefab.Instance<Spatial>();
                instance.GlobalTranslation = GlobalTranslation + -GlobalTransform.basis.z * 3;
                LevelManager.MustGetNode(this).AddChild(instance);
            }
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
            _abilityControl.SetChangeLock(true);
        }
    }
}
