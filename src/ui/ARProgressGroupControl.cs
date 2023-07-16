using Godot;
using System.Collections.Generic;

public class ARProgressGroupControl : VBoxContainer
{    
    private Camera _camera;

    private Spatial _target;

    private Vector3 _targetOffset = Vector3.Zero;

    [Export]
    private PackedScene progressControlPrefab;

    private List<ARProgressControl> _progressBars = new List<ARProgressControl>();

    public override void _Ready()
    {
        Visible = false;
        _camera = LevelManager.MustGetNode(this).Camera;
    }

    public override void _Process(float delta)
    {
        if(_camera == null || _target == null) {
            return;
        }

        if(!IsInstanceValid(_target)) {
            QueueFree();
            return;
        }

        if(_camera.IsPositionBehind(_target.GlobalTranslation)) {
            Visible = false;
        } else {
            var halfWidth =  RectSize.x / 2;
            var halfHeight = RectSize.y / 2;
            Visible = true;
            var centerScreenPos = _camera.UnprojectPosition(_target.GlobalTranslation + _targetOffset);

            RectPosition = new Vector2(centerScreenPos.x - halfWidth, centerScreenPos.y - halfHeight);
        }
    }

    public void SetBarCount(int count) {
        if(count > _progressBars.Count) {
            var addCount = count - _progressBars.Count;
            for(var i=0; i<addCount; i++) {
                var newBar = progressControlPrefab.Instance<ARProgressControl>();
                _progressBars.Add(newBar);
                AddChild(newBar);
            }

            return;
        }

        for(var i=_progressBars.Count-1; i>=count; i--) {
            _progressBars[i].QueueFree();
            _progressBars.RemoveAt(i);
        }

    }

    public void SetLabels(string[] labels) {
        if(labels.Length != _progressBars.Count) {
            GD.PushError($"Progress bar count mismatch: {labels.Length}, {_progressBars.Count}");
            return;
        }

        for(int i=0; i<labels.Length; i++) {
            _progressBars[i].SetLabel(labels[i]);
        }
    }

    public void SetProgresses(float[] progresses) {
        if(progresses.Length != _progressBars.Count) {
            GD.PushError($"Progress bar count mismatch: {progresses.Length}, {_progressBars.Count}");
            return;
        }


        for(int i=0; i<progresses.Length; i++) {
            _progressBars[i].SetProgress(progresses[i]);
        }
    }

    public void SetTargetOffset(Vector3 offset) {
        _targetOffset = offset;
    }

    public void SetTarget(Spatial target) {
        _target = target;
    }
}
