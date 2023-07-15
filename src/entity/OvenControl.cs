using System;
using System.Linq;
using Godot;

public class OvenControl : Area
{
    private float _cookRate = 0.05f;
    public float CookRate => _cookRate;

    [Export]
    private NodePath[] cookParticlePaths;
    private CPUParticles[] _cookParticles;

    [Export]
    private NodePath cookLightPath;
    private OmniLight _cookLight;

    public override void _Ready()
    {
        _cookParticles = cookParticlePaths.Select(p => GetNode<CPUParticles>(p) ?? throw new NullReferenceException()).ToArray();
        _cookLight = GetNode<OmniLight>(cookLightPath)?? throw new NullReferenceException();
    }

    public void CookingActive(bool active) {
        foreach(var p in _cookParticles) {
            p.Emitting = active;
            _cookLight.Visible = active;
        }
    }
}
