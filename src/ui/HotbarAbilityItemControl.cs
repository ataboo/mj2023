using Godot;
using System;

[Tool]
public class HotbarAbilityItemControl : Container
{
    Color activeColor = new Color("#FFFFFF");
    Color innactiveColor = new Color("#777777");

    [Export]
    private Texture abilityTexture;

    [Export]
    private  string labelText;

    [Export]
    public MechAbility ability;

    [Export]
    private NodePath abilityRectPath;
    private TextureRect _abilityRect;

    [Export]
    private NodePath backgroundPanelPath;
    private Panel _backgroundPanel;

    [Export]
    private NodePath keyNumberLabelPath;
    private Label _keyNumberLabel;

    public override void _Ready()
    {
        _abilityRect = GetNode<TextureRect>(abilityRectPath) ?? throw new NullReferenceException();
        _backgroundPanel = GetNode<Panel>(backgroundPanelPath) ?? throw new NullReferenceException();
        _keyNumberLabel = GetNode<Label>(keyNumberLabelPath) ?? throw new NullReferenceException();


        _abilityRect.Texture = abilityTexture;
        _keyNumberLabel.Text = labelText;
    }

    public void SetActive(bool active) {
        this._backgroundPanel.Modulate = active ? activeColor : innactiveColor;
    }
}
