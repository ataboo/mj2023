using Godot;
using System;

public class MainMenuControl : Control
{
    [Export]
    NodePath startButtonPath;

    [Export]
    NodePath audioCheckboxPath;
    private ButtonCheckbox _audioCheckbox;


    [Export]
    NodePath musicCheckboxPath;
    private ButtonCheckbox _musicCheckbox;

    private GameManager _gameManager;

    [Export]
    NodePath instructionsPath;
    private PanelContainer _instructions;

    [Export]
    NodePath creditsPath;
    private PanelContainer _credits;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        _audioCheckbox = GetNode<ButtonCheckbox>(audioCheckboxPath) ?? throw new NullReferenceException();
        _musicCheckbox = GetNode<ButtonCheckbox>(musicCheckboxPath) ?? throw new NullReferenceException();
        _instructions = GetNode<PanelContainer>(instructionsPath) ?? throw new NullReferenceException();
        _credits = GetNode<PanelContainer>(creditsPath) ?? throw new NullReferenceException();
        
        _gameManager = GameManager.MustGetNode(this);
        _musicCheckbox.Checked = _gameManager.AudioOn && _gameManager.MusicOn;
        _audioCheckbox.Checked = _gameManager.AudioOn;
    }

    public void _HandleAudioChecked(bool @checked) 
    {
        if(!@checked) {
            _musicCheckbox.Checked = false;
            _gameManager.MusicOn = false;
        }

        _gameManager.AudioOn = @checked;
    }

    public void _HandleMusicChecked(bool @checked) 
    {
        _gameManager.MusicOn = @checked;
    }

    public void _HandleStartClicked() {
        _gameManager.LoadMainLevel();
    }

    public void _HandleInstructionsClicked() {
        _instructions.Visible = true;
    }

    public void _HandleCloseInstructionsClicked() {
        _instructions.Visible = false;
    }

    public void _HandleCreditsClicked() {
        _credits.Visible = true;
    }

    public void _HandleCloseCreditsClicked() {
        _credits.Visible = false;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
