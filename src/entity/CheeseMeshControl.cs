using System;
using Godot;

public class CheeseMeshControl : MeshInstance, ICookable
{
    private CookedState _cookedState = CookedState.Raw;
    public CookedState CookedState { get => _cookedState; set => SetCooked(value); }


    [Export]
    private Color cookedColor = Colors.SandyBrown;

    [Export]
    private Color burntColor = new Color("#29251e");

    [Export]
    private Color rawColor = Colors.White;

    private SpatialMaterial _material;

    public override void _Ready()
    {
        _material = GetActiveMaterial(0) as SpatialMaterial ?? throw new NullReferenceException();
        _material.ResourceLocalToScene = true;
    }

    private void SetCooked(CookedState cookedState) {
        _cookedState = cookedState;

        switch(cookedState) {
            case CookedState.Raw:
                _material.AlbedoColor = rawColor;
                break;
            case CookedState.Cooked:
                _material.AlbedoColor = cookedColor;
                break;
            case CookedState.Burned:
                _material.AlbedoColor = burntColor;
                break;
        }

    }
}