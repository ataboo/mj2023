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
    private float barrelAccel = 1.2f;

    [Export]
    private NodePath barrelPath;
    private Spatial _barrel;

    [Export]
    private NodePath revAudioPath;
    private AudioStreamPlayer3D _revPlayer;

    [Export]
    private AudioStream revUpStream;

    [Export]
    private AudioStream revDownStream;

    [Export]
    private NodePath shootAudioPath;
    private AudioStreamPlayer3D _shootPlayer;

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
        _revPlayer = GetNode<AudioStreamPlayer3D>(revAudioPath) ?? throw new NullReferenceException();
        _shootPlayer = GetNode<AudioStreamPlayer3D>(shootAudioPath) ?? throw new NullReferenceException();

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
            _shootPlayer.Playing = false;
        }
    }

    private void _HandleClick(bool isPressed, bool leftClick)
    {
        if (_abilityControl.AbilityState.activeAbility != MechAbility.Cheese)
        {
            return;
        }

        if(leftClick) {

            if(_firing != isPressed) {
                _revPlayer.Stream = isPressed ? revUpStream : revDownStream;
                _revPlayer.Seek(0);
                _revPlayer.Playing = true;
                
                _firing = isPressed;
            }
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        var shooting = _barrelSpeed > 0.9f * maxBarrelSpeed;
        _barrelSpeed = Mathf.Lerp(_barrelSpeed, _firing ? maxBarrelSpeed : 0, barrelAccel * delta);

        _barrel.RotateY(-_barrelSpeed * delta);

        _particles.Emitting = shooting;

        _shootPlayer.Playing = shooting;

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
}
