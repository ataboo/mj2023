using Godot;
using System;

public class CheeseArmControl : Spatial
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
    NodePath cockpitControlPath;
    private CockpitControl _cockpitControl;

    [Export]
    NodePath particlePath;
    private CPUParticles _particles;

    private bool _firing = false;

    private Random _rng = new Random();

    private float _barrelSpeed;

    [Export]
    private float maxBarrelSpeed = 80f;

    [Export]
    private float barrelAccel = 1f;

    [Export]
    private NodePath barrelPath;
    private Spatial _barrel;

    float cheeseTimeoutRate = 0.05f;
	float _cheeseCooldown;

    public override void _Ready()
    {
        _tree = GetNode<AnimationTree>(animationTreePath) ?? throw new NullReferenceException();
        _stateMachine = (AnimationNodeStateMachinePlayback)_tree.Get("parameters/playback") ?? throw new NullReferenceException();
        _animationPlayer = _tree.GetNode<AnimationPlayer>(_tree.AnimPlayer) ?? throw new NullReferenceException();
        _abilityControl = GetNode<AbilityControl>(abilityControlPath) ?? throw new NullReferenceException();
        _cockpitControl = GetNode<CockpitControl>(cockpitControlPath) ?? throw new NullReferenceException();
        _particles = GetNode<CPUParticles>(particlePath) ?? throw new NullReferenceException();
        _barrel = GetNode<Spatial>(barrelPath) ?? throw new NullReferenceException();

        _abilityControl.Connect(nameof(AbilityControl.OnAbilityChange), this, nameof(_HandleAbilityChanged));
        _abilityControl.Connect(nameof(AbilityControl.OnClick), this, nameof(_HandleClick));

    }

    private void _HandleAbilityChanged(MechAbility ability)
    {
        if (ability == MechAbility.Cheese)
        {
            _stateMachine.Travel("GunIdle");
        }
        else
        {
            _firing = false;
            _stateMachine.Travel("GunStowed");
        }
    }

    private void _HandleClick(bool isPressed, bool leftClick)
    {
        if (_abilityControl.AbilityState.activeAbility != MechAbility.Cheese)
        {
            return;
        }

        if(leftClick) {
            _firing = isPressed;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        var shooting = _barrelSpeed > 0.9f * maxBarrelSpeed;
        _barrelSpeed = Mathf.Lerp(_barrelSpeed, _firing ? maxBarrelSpeed : 0, barrelAccel * delta);

        _barrel.RotateY(-_barrelSpeed * delta);

        _particles.Emitting = shooting;

        if(shooting) {
            if(_cheeseCooldown > 0) {
                _cheeseCooldown-=delta;
            } else {
                _cheeseCooldown = cheeseTimeoutRate;
                var pizza = _cockpitControl.AimedPizza();
                if(pizza != null) {
                    pizza.AddCheese();
                }

            }
        }
    }

    public override void _Process(float delta)
    {
        // if(_shotCooldown > 0) {
        //     _shotCooldown-=delta;
        // }

        // if(!_firing){
        //     return;
        // }

        // if(_shotCooldown <= 0) {
        //     var glob = sauceGlobPrefab.Instance<SauceGlobControl>();
        //     _entityHolder.AddChild(glob);

    	// 	glob.Translation = _muzzle.GlobalTranslation;

        //     var aimPoint = _cockpitControl.AimPoint();
        //     var aimDirection = (aimPoint - _muzzle.GlobalTranslation).Normalized();

        //     glob.GlobalTransform = new Transform(GlobalTransform.basis, glob.Transform.origin);
        //     glob.Rotate(glob.Transform.basis.z, Mathf.Lerp(-Mathf.Pi, Mathf.Pi, (float)_rng.NextDouble()));
        //     glob.Rotate(glob.Transform.basis.x, Mathf.Pi / 2f);

        //     glob.LinearVelocity = aimDirection * _muzzleVelocity;

        //     _shotCooldown = _fireDelay;
        // }
    }
}
