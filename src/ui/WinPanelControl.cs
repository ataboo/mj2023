using Godot;
using System;

public class WinPanelControl : PanelContainer
{
    [Export]
    NodePath starHolderPath;
    private Control _starHolder;

    [Export]
    NodePath summaryPath;
    private Label _summary;

    [Export]
    Texture goldStar;

    [Export]
    Texture blackStar;

    private GameManager _gameManager;

    public override void _Ready()
    {
        _starHolder = GetNode<Control>(starHolderPath) ?? throw new NullReferenceException();
        _summary = GetNode<Label>(summaryPath) ?? throw new NullReferenceException();
        _gameManager = GameManager.MustGetNode(this) ?? throw new NullReferenceException();
    }

    public void Show(int complete, int failed) {
        Visible = true;
        GetTree().Paused = true;

        var ratio = (float)complete / failed;

        _summary.Text = $"{complete} / {complete + failed}";
        
        _starHolder.GetChild<TextureRect>(0).Texture = ratio >= .5f ? goldStar : blackStar;
        _starHolder.GetChild<TextureRect>(1).Texture = ratio >= .9f ? goldStar : blackStar;
        _starHolder.GetChild<TextureRect>(2).Texture = ratio == 1f ? goldStar : blackStar;

        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    private void _HandleClickQuit() {
        GetTree().Paused = false;
        _gameManager.LoadMainMenu();
    }
}
