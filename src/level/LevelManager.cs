using System;
using Godot;

public class LevelManager : Node
{
    public static LevelManager MustGetNode(Node self)
    {
        return self.GetNode<LevelManager>("/root/Level") ?? throw new System.NullReferenceException();
    }

    private GameManager _gameManager;
    public GameManager GameManager {
        get {
            if(_gameManager == null) {
                _gameManager = GameManager.MustGetNode(this) ?? throw new NullReferenceException();
            }

            return _gameManager;
        }
    }

    [Export]
    private NodePath arHolderPath;
    private ARHolderControl _arHolder;
    public ARHolderControl ARHolder {
        get {
            if(_arHolder == null) {
                _arHolder = GetNode<ARHolderControl>(arHolderPath) ?? throw new NullReferenceException();
            }

            return _arHolder;
        }
    }

    [Export]
    private NodePath hudControlPath;
    private HudControl _hudControl;
    public HudControl HudControl {
        get {
            if(_hudControl == null) {
                _hudControl = GetNode<HudControl>(hudControlPath) ?? throw new NullReferenceException();
            }

            return _hudControl;
        }
    }

    [Export]
    private NodePath playerControlPath;
    private PlayerControl _playerControl;
    public PlayerControl PlayerControl {
        get {
            if (_playerControl == null) {
                _playerControl = GetNode<PlayerControl>(playerControlPath) ?? throw new NullReferenceException();
            }

            return _playerControl;
        }
    }

    [Export]
    private NodePath hotbarAbilityControlPath;
    private HotbarAbilityControl _hotbarAbilityControl;
    public HotbarAbilityControl HotbarAbilityControl {
        get {
            if(_hotbarAbilityControl == null) {
                _hotbarAbilityControl = GetNode<HotbarAbilityControl>(hotbarAbilityControlPath);
            }

            return _hotbarAbilityControl;
        }
    }

    [Export]
    private NodePath entityHolderPath;
    private Spatial _entityHolder;
    public Spatial EntityHolder {
        get {
            if(_entityHolder == null) {
                _entityHolder = GetNode<Spatial>(entityHolderPath);
            }

            return _entityHolder;
        }
    }

    [Export]
    private NodePath ordersManagerPath;
    private OrdersManager _ordersManager;
    public OrdersManager OrdersManager {
        get {
            if(_ordersManager == null) {
                _ordersManager = GetNode<OrdersManager>(ordersManagerPath);
            }

            return _ordersManager;
        }
    }

    public Camera Camera => PlayerControl.Camera;

    public override void _Ready()
    {
        PlayerControl.AbilityControl.Connect(nameof(AbilityControl.OnAbilityChange), HotbarAbilityControl, nameof(HotbarAbilityControl._HandleAbilityChange));
    }
}
