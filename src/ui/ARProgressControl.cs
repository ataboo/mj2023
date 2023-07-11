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

    public override void _Ready()
    {
        _progressFront = GetNode<Panel>(progressFrontPath) ?? throw new NullReferenceException();
        _label = GetNode<Label>(labelPath) ?? throw new NullReferenceException();

        _barStartWidth = _progressFront.RectSize.x;
    }

    public void SetLabel(string label) {
        _label.Text = label;
    }

    public void SetProgress(float t) {
        t = Mathf.Clamp(t, 0, 1);
        _progressFront.RectSize = new Vector2(t * _barStartWidth, _progressFront.RectSize.y);
    }
}
