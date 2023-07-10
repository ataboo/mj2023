using Godot;
using System;

public class ARProgressControl : VBoxContainer
{
    [Export]
    public NodePath progressFrontPath;
    private Panel _progressFront;
    
    [Export]
    public NodePath labelPath;
    private Label _label;
    
    private float _barStartWidth;

    private float _halfWidth;

    private Camera _camera;

    private Spatial _target;

    private Vector3 _targetOffset = Vector3.Zero;

    public override void _Ready()
    {
        _progressFront = GetNode<Panel>(progressFrontPath) ?? throw new NullReferenceException();
        _label = GetNode<Label>(labelPath) ?? throw new NullReferenceException();

        _barStartWidth = _progressFront.RectSize.x;
        _halfWidth = RectSize.x / 2;
    }

    public override void _Process(float delta)
    {
        if(_camera == null || _target == null) {
            return;
        }

        var centerScreenPos = _camera.UnprojectPosition(_target.GlobalTranslation + _targetOffset);

        RectPosition = new Vector2(centerScreenPos.x - _halfWidth, centerScreenPos.y);
    }

    public void SetLabel(string label) {
        _label.Text = label;
    }

    public void SetProgress(float t) {
        t = Mathf.Clamp(t, 0, 1);
        _progressFront.RectSize = new Vector2(t * _barStartWidth, _progressFront.RectSize.y);
    }

    public void SetTargetOffset(Vector3 offset) {
        _targetOffset = offset;
    }

    public void SetTarget(Spatial target, Camera camera) {
        _camera = camera;
        _target = target;
    }
}
