using Godot;
using System;
using System.Collections.Generic;

public class cockpitText : Label
{
    private List<String> ToolTips = new List<String>();

    public override void _Ready()
    {
        ToolTips.Add("LMB: Punch");
        ToolTips.Add("RMB [HOLD]:" + System.Environment.NewLine +"Raise Leg" + System.Environment.NewLine + System.Environment.NewLine + "LMB: Kick");
        ToolTips.Add("LMB: Extend &" + System.Environment.NewLine +"grab dough" + System.Environment.NewLine + System.Environment.NewLine + "RMB: Spin plate");
        ToolTips.Add("LMB [HOLD]:" + System.Environment.NewLine +"Shoot sauce");
        ToolTips.Add("LMB: Shoot" + System.Environment.NewLine +"pepperoni");
        ToolTips.Add("LMB [HOLD]:" + System.Environment.NewLine +"Shoot cheese");
    }

    public void _HandleAbilityChange(int ability) 
    {
        SetText(ToolTips[ability]);
    }
}
