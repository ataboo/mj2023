using Godot;
using System;

public class PGunControl : Spatial
{
	[Export]
	public NodePath animationTreePath;
	
	[Export]
	public PackedScene pepperoniPrefab;
	
	private AnimationTree _tree;
	

	private AnimationNodeStateMachinePlayback _stateMachine;

	private AnimationPlayer _animationPlayer;

	private float _muzzleVelocity = 50f;
	private int _magSize = 5;
	private int mag = 0;
	// private int ammo = 50;

	[Export]
	NodePath abilityControlPath;
	private AbilityControl _abilityControl;
	
	[Export]
	private NodePath muzzlePath;
	private Spatial _muzzle;

	[Export]
	private NodePath cockpitControlPath;
	private CockpitControl _cockpit;

	private Spatial _entityHolder;

	private Random _rng;

	[Export]
	NodePath shootAudioPath;
	private AudioStreamPlayer3D _shootAudio;


	public override void _Ready()
	{
		_tree = GetNode<AnimationTree>(animationTreePath) ?? throw new NullReferenceException();
		_stateMachine = (AnimationNodeStateMachinePlayback)_tree.Get("parameters/playback") ?? throw new NullReferenceException();
		_animationPlayer = _tree.GetNode<AnimationPlayer>(_tree.AnimPlayer) ?? throw new NullReferenceException();
		_abilityControl = GetNode<AbilityControl>(abilityControlPath) ?? throw new NullReferenceException();
		_muzzle = GetNode<Spatial>(muzzlePath) ?? throw new NullReferenceException();
		_cockpit = GetNode<CockpitControl>(cockpitControlPath) ?? throw new NullReferenceException();
		_entityHolder = LevelManager.MustGetNode(this).EntityHolder ?? throw new NullReferenceException();
		_shootAudio = GetNode<AudioStreamPlayer3D>(shootAudioPath) ?? throw new NullReferenceException();

		_abilityControl.Connect(nameof(AbilityControl.OnAbilityChange), this, nameof(_HandleAbilityChanged));
		_abilityControl.Connect(nameof(AbilityControl.OnClick), this, nameof(_HandleClick));

		_rng = new Random();
		mag = _magSize;
	}
	
	private void _HandleAbilityChanged(MechAbility ability)
	{
		if (ability == MechAbility.Pepperoni)
		{
			_stateMachine.Travel("PGunIdle");
			if(mag == 0)
			{
				Reload();
			}
		}
		else
		{
			_stateMachine.Travel("PGunStowed");
		}
	}
	
	private void _HandleClick(bool isPressed, bool leftClick)
	{
		if (_abilityControl.AbilityState.activeAbility != MechAbility.Pepperoni)
		{
			return;
		}
		
		if (leftClick && isPressed)
		{
			if(_stateMachine.GetCurrentNode() == "PGunIdle")
			{
				if(mag > 0)
				{
					FirePepperoni();
				}
				else
				{
					Reload();
				}
			}
		}
	}

	private void FirePepperoni() {
		_stateMachine.Start("PGunFire");
		mag -= 1;
					
		_shootAudio.Playing = true;
		var roni = pepperoniPrefab.Instance<PepperoniControl>();
		_entityHolder.AddChild(roni);
		roni.Translation = _muzzle.GlobalTranslation;

		var aimPoint = _cockpit.AimPoint();
		var aimDirection = (aimPoint - _muzzle.GlobalTranslation).Normalized();
		roni.GlobalTransform = new Transform(GlobalTransform.basis, roni.Transform.origin);
		roni.Rotate(roni.Transform.basis.z, Mathf.Lerp(-Mathf.Pi, Mathf.Pi, (float)_rng.NextDouble()));
		roni.Rotate(roni.Transform.basis.x, Mathf.Pi / 2f);

		roni.LinearVelocity = aimDirection * _muzzleVelocity;
	}
	
	private void Reload()
	{
		_stateMachine.Travel("PGunReload");
		mag = _magSize;
		// GD.Print("Mag:" + mag);
		// if(ammo > 0)//only reload if player has ammo
		// {
		// 	_stateMachine.Travel("PGunReload");
		// 	if(ammo >= _magSize)
		// 	{
		// 		//reload mag to max
		// 		ammo -= _magSize;
		// 		mag = _magSize;
		// 	}
		// 	else
		// 	{
		// 		//load in whatever is left
		// 		mag = ammo;
		// 		ammo = 0;
		// 	}
		// 	// GD.Print("mag reloaded to " + mag + " rounds");			
		// }
	}
}
