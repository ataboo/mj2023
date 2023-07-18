using Godot;
using System;

public class OrderCardControl : PanelContainer
{
    [Export]
    private NodePath sauceIconPath;
    private TextureRect _sauceIcon;

    [Export]
    private NodePath cheeseIconPath;
    private TextureRect _cheeseIcon;

    [Export]
    private NodePath roniIconPath;
    private TextureRect _roniIcon;

    [Export] 
    NodePath progressFrontPath;
    private Panel _progressFront;

    OrderState _orderState;

    private float _progressWidth = 100;

    private StyleBoxFlat _panelStyleBox;

    private FadeMode _fadeMode = FadeMode.None;

    private float _fadeProgress;
    private float _fadeTarget;

    private Color _startColor;

    private bool _orderFailed;

    public override void _Ready()
    {
        _sauceIcon = GetNode<TextureRect>(sauceIconPath) ?? throw new NullReferenceException();
        _cheeseIcon = GetNode<TextureRect>(cheeseIconPath) ?? throw new NullReferenceException();
        _roniIcon = GetNode<TextureRect>(roniIconPath) ?? throw new NullReferenceException();

        _progressFront = GetNode<Panel>(progressFrontPath) ?? throw new NullReferenceException();

        _panelStyleBox = GetStylebox("panel") as StyleBoxFlat ?? throw new NullReferenceException();
        _startColor = _panelStyleBox.BgColor; 
    }

    public void SetOrder(OrderState order) {
        _sauceIcon.Visible = order.sauce;
        _cheeseIcon.Visible = order.cheese;
        _roniIcon.Visible = order.roni;
    }

    public void SetProgress(float t) {
        t = Mathf.Clamp(t, 0f, 1f);
        var width = Mathf.Lerp(0, _progressWidth, t);
        _progressFront.RectSize = new Vector2(width, _progressFront.RectSize.y);
    }

    public override void _Process(float delta)
    {
        if(_fadeMode == FadeMode.None) {
            return;
        }
        
        _fadeProgress += delta;
        var t = Mathf.Clamp(_fadeProgress / _fadeTarget, 0, 1);
        
        switch(_fadeMode) {
            case FadeMode.Flash:
                _panelStyleBox.BgColor = _startColor.LinearInterpolate(Colors.White, t);
                if(t == 1) {
                    _fadeMode = FadeMode.Color;
                    _fadeProgress = 0f;
                    _fadeTarget = .2f;
                }
                break;
            case FadeMode.Color:
                _panelStyleBox.BgColor = Colors.White.LinearInterpolate(_orderFailed ? Colors.PaleVioletRed : Colors.LightGreen, t);
                if(t == 1) {
                    _fadeMode = FadeMode.None;
                    GetTree().CreateTimer(2f).Connect("timeout", this, nameof(_HandleTimeoutTimer));
                }
                break;
        }
    }

    private void _HandleTimeoutTimer() {
        QueueFree();
    }

    public void EndOrder(bool failed) {
        _orderFailed = failed;
        _fadeProgress = 0;
        _fadeTarget = 0.1f;
        _fadeMode = FadeMode.Flash;
    }

    private enum FadeMode  {
        None,
        Flash,
        Color,
    }    
}

