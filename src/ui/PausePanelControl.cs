using Godot;
using System;

public class PausePanelControl : PanelContainer
{
    private LevelManager _level;
    private GameManager _gameManager;

    [Export]
    private NodePath instructionsPath;
    private InstructionsControl _instructions;

    public override void _Ready()
    {
        _instructions = GetNode<InstructionsControl>(instructionsPath) ?? throw new NullReferenceException();
        _level = LevelManager.MustGetNode(this);
        _gameManager = GameManager.MustGetNode(this);
    }

    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventKey key && key.IsPressed() && key.Scancode == (uint)KeyList.Escape) {
            TogglePaused();
        }
    }

    private void TogglePaused() {
        SetPaused(!Visible);
    }

    private void SetPaused(bool paused) {
        if(_level.IsOver) {
            return;
        }

        Visible = paused;
        GetTree().Paused = paused;

        Input.MouseMode = paused ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
    }

    private void _HandleClickResume() {
        SetPaused(false);
    }

    private void _HandleClickQuit() {
        SetPaused(false);
        _gameManager.LoadMainMenu();
    }

    private void _HandleClickInstructions() {
        _instructions.Visible = true;
    }

    private void _HandleClickRestart() {
        SetPaused(false);
        GetTree().ReloadCurrentScene();
    }

    private void _HandlePauseButtonPush() {
        TogglePaused();
    }
}
