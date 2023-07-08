using Godot;
using System;

[Tool]
public class HotbarAbilityItemControl : Container
{
    Color activeColor = new Color("#FFFFFF");
    Color innactiveColor = new Color("#777777");

    [Export]
    Texture abilityTexture;

    [Export]
    public string labelText;

    [Export]
    public MechAbility ability;

    [Export]
    NodePath abilityRectPath;
    TextureRect _abilityRect;

    [Export]
    NodePath backgroundPanelPath;
    Panel _backgroundPanel;

    [Export]
    NodePath keyNumberLabelPath;
    Label _keyNumberLabel;

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
