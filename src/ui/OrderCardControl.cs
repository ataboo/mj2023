using Godot;
using System;
using System.Threading.Tasks;

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

    public override void _Ready()
    {
        _sauceIcon = GetNode<TextureRect>(sauceIconPath) ?? throw new NullReferenceException();
        _cheeseIcon = GetNode<TextureRect>(cheeseIconPath) ?? throw new NullReferenceException();
        _roniIcon = GetNode<TextureRect>(roniIconPath) ?? throw new NullReferenceException();

        _progressFront = GetNode<Panel>(progressFrontPath) ?? throw new NullReferenceException();

        _panelStyleBox = GetStylebox("panel") as StyleBoxFlat ?? throw new NullReferenceException();
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

    public async Task EndOrder(bool failed) {
        var startingBGColor = _panelStyleBox.BgColor;

        var finalColor = failed ? Colors.PaleVioletRed : Colors.LightGreen;

        await FadeToColor(Colors.White, .05f, .01f);
        await FadeToColor(finalColor, .05f, .01f);

        await Task.Delay(TimeSpan.FromSeconds(2));

        QueueFree();
    }

    private async Task FadeToColor(Color color, float duration, float tickDelta) {
        for(var i=0f; i<duration; i+=tickDelta) {
            var t = i / duration;
            _panelStyleBox.BgColor = _panelStyleBox.BgColor.LinearInterpolate(color, t);
            await Task.Delay(TimeSpan.FromSeconds(tickDelta));
        }
    }
}
