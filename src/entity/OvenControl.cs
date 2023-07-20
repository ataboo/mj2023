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

    [Export]
    private NodePath ovenEndAudioPath;
    private AudioStreamPlayer3D _ovenEndAudio;

    [Export]
    private NodePath ovenLoopAudioPath;
    private AudioStreamPlayer3D _ovenLoopAudio;

    private int _pizzaCount;

    private bool _active;

    public override void _Ready()
    {
        _cookParticles = cookParticlePaths.Select(p => GetNode<CPUParticles>(p) ?? throw new NullReferenceException()).ToArray();
        _cookLight = GetNode<OmniLight>(cookLightPath)?? throw new NullReferenceException();
        _ovenEndAudio = GetNode<AudioStreamPlayer3D>(ovenEndAudioPath) ?? throw new NullReferenceException();
        _ovenLoopAudio = GetNode<AudioStreamPlayer3D>(ovenLoopAudioPath) ?? throw new NullReferenceException();
    }

    private void SetCookingActive(bool active) {
        foreach(var p in _cookParticles) {
            p.Emitting = active;
            _cookLight.Visible = active;
        }

        _ovenLoopAudio.Playing = active;
        if(!active && _active) {
            _ovenEndAudio.Playing = true;
        }

        _active = active;
    }

    public void AddPizza() {
        _pizzaCount++;
        SetCookingActive(_pizzaCount > 0);
    }


    public void RemovePizza() {
        _pizzaCount--;
        SetCookingActive(_pizzaCount > 0);
    }
}
