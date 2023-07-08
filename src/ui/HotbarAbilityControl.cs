using Godot;
using System;
using System.Collections.Generic;

public class HotbarAbilityControl : CenterContainer
{
    [Export]
    public NodePath hotbarItemHolderPath;
    private Control _hotbarItemHolder;

    [Export]
    public NodePath playerControlPath;
    private PlayerControl _playerControl;

    private List<HotbarAbilityItemControl> _hotbarItems;

    public override void _Ready()
    {
        _hotbarItemHolder = GetNode<Control>(hotbarItemHolderPath) ?? throw new NullReferenceException();
        _playerControl = GetNode<PlayerControl>(playerControlPath) ?? throw new NullReferenceException();


        _hotbarItems = new List<HotbarAbilityItemControl>();
        for(int i=0; i<_hotbarItemHolder.GetChildCount(); i++) {
            if(_hotbarItemHolder.GetChild(i) is HotbarAbilityItemControl item) {
                _hotbarItems.Add(item);
            }
        }
        
        _playerControl.Connect(nameof(PlayerControl.OnAbilityChange), this, nameof(_HandleAbilityChange));

        _HandleAbilityChange((int)MechAbility.Grab);
    }

    public void _HandleAbilityChange(int ability) {
        foreach(var item in _hotbarItems) {
            item.SetActive((int)item.ability == ability);
        }
    }
}
