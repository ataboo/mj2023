using System;
using Godot;

public class HudControl: Control {
    [Export]
    public NodePath bottomCrosshairPath;
    private TextureRect _bottomCrosshair;
    
    [Export]
    public NodePath topCrosshairPath;
    private TextureRect _topCrosshair;

    [Export]
    public NodePath cameraPath;
    private Camera _camera;

    [Export]
    public NodePath playerControlPath;
    private PlayerControl _playerControl;

    [Export]
    public Texture bottomCrosshairsText;

    [Export]
    public Texture bottomCrosshairsHashedText;
    
    public override void _Ready()
    {
        _bottomCrosshair = GetNode<TextureRect>(bottomCrosshairPath) ?? throw new NullReferenceException();
        _camera = GetNode<Camera>(cameraPath) ?? throw new NullReferenceException();
        _playerControl = GetNode<PlayerControl>(playerControlPath) ?? throw new NullReferenceException();

        _playerControl.Connect(nameof(PlayerControl.OnPhysicsDone), this, nameof(_HandleChassisPhysicsComplete));
    }

    public void _HandleChassisPhysicsComplete(float delta) {
        var halfCrosshairWidth = _bottomCrosshair.RectSize.x / 2;

        var legsForwardPos = _camera.GlobalTranslation - _playerControl.Transform.basis.z;

        var screenPos = _camera.UnprojectPosition(legsForwardPos);
        screenPos.y += 200;
        var clampedPos = new Vector2(screenPos.x, screenPos.y);
        if(_camera.IsPositionBehind(legsForwardPos)) {
            if(screenPos.x > GetViewportRect().Size.x / 2) {
                clampedPos.x = halfCrosshairWidth;
            } else {
                clampedPos.x = GetViewportRect().Size.x - halfCrosshairWidth;
            }
        } else {
            clampedPos.x = Mathf.Clamp(clampedPos.x, halfCrosshairWidth, GetViewportRect().Size.x - halfCrosshairWidth);
        }
        clampedPos.y = Mathf.Clamp(clampedPos.y, halfCrosshairWidth, GetViewportRect().Size.y - halfCrosshairWidth);

        _bottomCrosshair.RectPosition = clampedPos - new Vector2(halfCrosshairWidth, halfCrosshairWidth);

        _bottomCrosshair.Texture = screenPos == clampedPos ? bottomCrosshairsText : bottomCrosshairsHashedText;
    }
}