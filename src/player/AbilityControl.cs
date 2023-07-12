using System;
using Godot;

public class AbilityControl : Spatial
{
	[Signal]
	public delegate void OnAbilityChange(int ability);

	[Signal]
	public delegate void OnClick(bool isPressed, bool isLeft);

	private GameManager _gameManager;

	[Export]
	public NodePath legControlPath;
	private MechLegControl _legControl;

	[Export]
	public NodePath playerControlPath;
	private PlayerControl _playerControl;

	private AbilityState _abilityState;
	public AbilityState AbilityState => _abilityState;
	
	private bool _changeLock = false;

	public override void _Ready()
	{
		_legControl = GetNode<MechLegControl>(legControlPath) ?? throw new NullReferenceException();
		_gameManager = GameManager.MustGetNode(this);
		_playerControl = GetNode<PlayerControl>(playerControlPath) ?? throw new NullReferenceException();
		_abilityState = new AbilityState();

		CallDeferred(nameof(SwitchAbility), 0, true);
	}

	public void SetChangeLock(bool changeLock) {
		_changeLock = changeLock;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton eventMouseButton)
		{

			switch ((ButtonList)eventMouseButton.ButtonIndex)
			{
				case ButtonList.WheelUp:
					if (eventMouseButton.IsPressed())
					{
						UpOneAbility();
					}
					break;
				case ButtonList.WheelDown:
					if (eventMouseButton.IsPressed())
					{
						DownOneAbility();
					}
					break;
				case ButtonList.Left:
					EmitSignal(nameof(OnClick), eventMouseButton.IsPressed(), true);
					break;
				case ButtonList.Right:
					EmitSignal(nameof(OnClick), eventMouseButton.IsPressed(), false);
					break;
			}
		}
		else if (@event is InputEventKey eventKey)
		{
			if (eventKey.IsPressed())
			{
				switch ((KeyList)eventKey.Scancode)
				{
					case KeyList.Key1:
						SwitchAbility(MechAbility.Grab);
						break;
					case KeyList.Key2:
						SwitchAbility(MechAbility.Kick);
						break;
					case KeyList.Key3:
						SwitchAbility(MechAbility.Plate);
						break;
					case KeyList.Key4:
						SwitchAbility(MechAbility.Sauce);
						break;
					case KeyList.Key5:
						SwitchAbility(MechAbility.Pepperoni);
						break;
					case KeyList.Key6:
						SwitchAbility(MechAbility.Cheese);
						break;
					default:
						break;
				}
			}
		}
	}

	private void UpOneAbility()
	{
		var maxAbility = MechGameHelpers.MaxEnumValue<MechAbility>();

		SwitchAbility((MechAbility)(((int)_abilityState.activeAbility + 1) % (maxAbility + 1)));
	}

	private void DownOneAbility()
	{
		var maxAbility = MechGameHelpers.MaxEnumValue<MechAbility>();

		SwitchAbility((MechAbility)(((int)_abilityState.activeAbility + maxAbility) % (maxAbility + 1)));
	}

	private void SwitchAbility(MechAbility ability, bool force = false)
	{
		if (!force && (ability == _abilityState.activeAbility || _changeLock))
		{
			return;
		}

		_abilityState.activeAbility = ability;
		this.EmitSignal(nameof(OnAbilityChange), (int)ability);
	}
}
