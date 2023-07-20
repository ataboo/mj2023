using Godot;
using System;
using System.Collections.Generic;

public class cockpitText : Label
{
    private List<String> ToolTips = new List<String>();

    // Here's another option that's a bit safer since you keep the ability typing
    // private Dictionary<MechAbility, string> _toolTips = new Dictionary<MechAbility, string>(){
    //     {MechAbility.Grab, "LMB: Punch"},
    //     //...
    // };

    public override void _Ready()
    {
        ToolTips.Add("LMB: Punch");
        ToolTips.Add("RMB [HOLD]:" + System.Environment.NewLine +"Raise Leg" + System.Environment.NewLine + System.Environment.NewLine + "LMB: Kick");
        ToolTips.Add("LMB: Pickup\n[HOLD] to spin\ndough\nRMB: Drop item");
        ToolTips.Add("LMB [HOLD]:" + System.Environment.NewLine +"Shoot sauce");
        ToolTips.Add("LMB: Shoot" + System.Environment.NewLine +"pepperoni");
        ToolTips.Add("LMB [HOLD]:" + System.Environment.NewLine +"Shoot cheese");
    }

    public void _HandleAbilityChange(int ability) 
    {
        Text = ToolTips[ability];
    }
}
