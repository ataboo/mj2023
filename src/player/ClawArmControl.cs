using Godot;
using System;

public class ClawArmControl : Spatial
{
    [Export]
    public NodePath animationTreePath;
    private AnimationTree _tree;

    private AnimationNodeStateMachinePlayback _stateMachine;

    private AnimationPlayer _animationPlayer;

    private float _punchForce = 10f;
    private float _punchDamage = 10f;

    [Export]
    NodePath abilityControlPath;
    private AbilityControl _abilityControl;

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
        if (ability == MechAbility.Grab)
        {
            _stateMachine.Travel("ClawIdle");
        }
        else
        {
            _stateMachine.Travel("ClawStowed");
        }
    }

    private void _HandleClick(bool isPressed, bool leftClick)
    {
        if (_abilityControl.AbilityState.activeAbility != MechAbility.Grab)
        {
            return;
        }

        if (leftClick && isPressed)
        {
            _stateMachine.Travel("ClawExtend");
        }
    }

    public void _HandleGrabDetectorBodyEntered(Node body)
    {
        if (_stateMachine.GetCurrentNode() != "ClawExtend")
        {
            return;
        }

        _stateMachine.Travel("ClawIdle");

        if (body is DoughballControl doughBall)
        {
            doughBall.Punch(GlobalTransform.basis.z * _punchForce, _punchDamage);
        }
        else if (body is DoughPileControl doughPile)
        {
            doughPile.Kick(Vector3.Zero, GlobalTransform.basis.z * _punchForce);
        }
    }


}
