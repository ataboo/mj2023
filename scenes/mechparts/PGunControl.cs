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

	private float _pepperoniSpeed = 10f;
	private int _magSize = 5;
	private int mag = 0;
	private int ammo = 50;

	[Export]
	NodePath abilityControlPath;
	private AbilityControl _abilityControl;
	
	[Export]
	public Spatial PSpawner;
	// Called when the node enters the scene tree for the first time.
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
		if (ability == MechAbility.Pepperoni)
		{
			GD.Print("Pepperoni Equipped, mag: " + mag + ", Ammo: " + ammo);
			_stateMachine.Travel("PGunIdle");
			if(mag == 0)
			{
				GD.Print("Reloading newly equipped gun");
				reload();
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
		
		//on click
		if (leftClick && isPressed)
		{
			//can only fire when in idle
			if(_stateMachine.GetCurrentNode() == "PGunIdle")
			{
				if(ammo > 0 || mag > 0)
				{
					if(mag > 0)//reload if out of ammo
					{
						_stateMachine.Start("PGunFire");
						mag -= 1;
						GD.Print("Mag: " + mag);
						
						//spawn pepperoni
						var pBullet = pepperoniPrefab.Instance<RigidBody>();
						PSpawner.AddChild(pBullet);
						pBullet.initialForce = true;
					}
					else
					{
						reload();
					}
				}
				else
				{
					//Play click sound, no ammo
				}
				
			}
			
		}
	}
	
	private void reload()
	{
		GD.Print("Mag:" + mag);
		if(ammo > 0)//only reload if player has ammo
		{
			_stateMachine.Travel("PGunReload");
			if(ammo >= _magSize)
			{
				//reload mag to max
				ammo -= _magSize;
				mag = _magSize;
			}
			else
			{
				//load in whatever is left
				mag = ammo;
				ammo = 0;
			}
			GD.Print("mag reloaded to " + mag + " rounds");			
		}
	}
}
