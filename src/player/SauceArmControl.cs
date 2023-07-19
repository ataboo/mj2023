using Godot;
using System;

public class SauceArmControl : Spatial
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
    PackedScene sauceGlobPrefab;

    [Export]
    NodePath cockpitControlPath;
    private CockpitControl _cockpitControl;

    [Export]
    NodePath muzzleNodePath;
    private Spatial _muzzle;

    private Spatial _entityHolder;

    private float _shotCooldown;

    private float _fireDelay = 0.1f;

    private bool _firing = false;

    private float _muzzleVelocity = 50;

    private Random _rng = new Random();

    [Export]
    private NodePath shootAudioPath;
    private AudioStreamPlayer3D _shootAudioPlayer;

    public override void _Ready()
    {
        _tree = GetNode<AnimationTree>(animationTreePath) ?? throw new NullReferenceException();
        _stateMachine = (AnimationNodeStateMachinePlayback)_tree.Get("parameters/playback") ?? throw new NullReferenceException();
        _animationPlayer = _tree.GetNode<AnimationPlayer>(_tree.AnimPlayer) ?? throw new NullReferenceException();
        _abilityControl = GetNode<AbilityControl>(abilityControlPath) ?? throw new NullReferenceException();
        _entityHolder = LevelManager.MustGetNode(this).EntityHolder ?? throw new NullReferenceException();
        _cockpitControl = GetNode<CockpitControl>(cockpitControlPath) ?? throw new NullReferenceException();
        _muzzle = GetNode<Spatial>(muzzleNodePath) ?? throw new NullReferenceException();
        _shootAudioPlayer = GetNode<AudioStreamPlayer3D>(shootAudioPath) ?? throw new NullReferenceException();

        _abilityControl.Connect(nameof(AbilityControl.OnAbilityChange), this, nameof(_HandleAbilityChanged));
        _abilityControl.Connect(nameof(AbilityControl.OnClick), this, nameof(_HandleClick));

    }

    private void _HandleAbilityChanged(MechAbility ability)
    {
        if (ability == MechAbility.Sauce)
        {
            _stateMachine.Travel("GunIdle");
        }
        else
        {
            _firing = false;
            _shootAudioPlayer.Playing = false;
            _stateMachine.Travel("GunStowed");
        }
    }

    private void _HandleClick(bool isPressed, bool leftClick)
    {
        if (_abilityControl.AbilityState.activeAbility != MechAbility.Sauce)
        {
            return;
        }

        if(leftClick) {
            _firing = isPressed;
            _shootAudioPlayer.Playing = _firing;
        }

    }

    public override void _Process(float delta)
    {
        if(_shotCooldown > 0) {
            _shotCooldown-=delta;
        }

        if(!_firing){
            return;
        }

        if(_shotCooldown <= 0) {
            var glob = sauceGlobPrefab.Instance<SauceGlobControl>();
            _entityHolder.AddChild(glob);

    		glob.Translation = _muzzle.GlobalTranslation;

            var aimPoint = _cockpitControl.AimPoint();
            var aimDirection = (aimPoint - _muzzle.GlobalTranslation).Normalized();

            glob.GlobalTransform = new Transform(GlobalTransform.basis, glob.Transform.origin);
            glob.Rotate(glob.Transform.basis.z, Mathf.Lerp(-Mathf.Pi, Mathf.Pi, (float)_rng.NextDouble()));
            glob.Rotate(glob.Transform.basis.x, Mathf.Pi / 2f);

            glob.LinearVelocity = aimDirection * _muzzleVelocity;

            _shotCooldown = _fireDelay;
        }
    }
}
