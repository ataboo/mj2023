using System;
using Godot;

public class DragonMouthControl: Area {
    
    [Export]
    NodePath dragonControlPath;
    
    private DragonControl _dragonControl;
    public DragonControl DragonControl {
        get {
            if(_dragonControl == null) {
                _dragonControl = GetNode<DragonControl>(dragonControlPath) ?? throw new NullReferenceException();
            }

            return _dragonControl;
        }   
    }

}