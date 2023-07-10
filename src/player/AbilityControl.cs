using System;
using Godot;

public class AbilityControl : Spatial
{
    [Signal]
    public delegate void OnAbilityChange(int ability);

    [Signal]
    public delegate void OnLegStateChange();

    private GameManager _gameManager;

    [Export]
    public NodePath legControlPath;
    private MechLegControl _legControl;

    [Export]
    public NodePath playerControlPath;
    private PlayerControl _playerControl;

    private AbilityState _abilityState;
    public AbilityState AbilityState => _abilityState;

    public override void _Ready()
    {
        _legControl = GetNode<MechLegControl>(legControlPath) ?? throw new NullReferenceException();
        _gameManager = GameManager.MustGetNode(this);
        _playerControl = GetNode<PlayerControl>(playerControlPath) ?? throw new NullReferenceException();
        _abilityState = new AbilityState();
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
                    HandleLeftClick(eventMouseButton.IsPressed());
                    break;
                case ButtonList.Right:
                    HandleRightClick(eventMouseButton.IsPressed());
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
                        SwitchAbility(MechAbility.Sauce);
                        break;
                    case KeyList.Key4:
                        SwitchAbility(MechAbility.Pepperoni);
                        break;
                    case KeyList.Key5:
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

    private void SwitchAbility(MechAbility ability)
    {
        if (ability == _abilityState.activeAbility)
        {
            return;
        }

        //TODO there might be a delay in pending and active with animations.

        var lastAbility = _abilityState.activeAbility;

        _abilityState.activeAbility = ability;
        this.EmitSignal(nameof(OnAbilityChange), (int)ability);

        if (lastAbility == MechAbility.Kick || ability == MechAbility.Kick)
        {
            this.EmitSignal(nameof(OnLegStateChange));
        }
    }

    private void HandleLeftClick(bool isPressed)
    {
        switch (_abilityState.activeAbility)
        {
            case MechAbility.Kick:
                if (_abilityState.legKick != isPressed)
                {
                    _abilityState.legKick = isPressed;
                    EmitSignal(nameof(OnLegStateChange));
                }
                break;
        }
    }

    private void HandleRightClick(bool isPressed)
    {
        switch (_abilityState.activeAbility)
        {
            case MechAbility.Kick:
                if (_abilityState.legWindUp != isPressed)
                {
                    _abilityState.legWindUp = isPressed;
                    EmitSignal(nameof(OnLegStateChange));
                }
                break;
        }

    }
}
