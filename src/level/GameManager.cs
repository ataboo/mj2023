using Godot;

public class GameManager : Node
{
    [Export]
    private PackedScene mainLevelScene;
    
    [Export]
    private PackedScene mainMenuScene;

    public UserConfig UserConfig { get; private set; }

    public static GameManager MustGetNode(Node self)
    {
        return self.GetNode<GameManager>("/root/GameManager") ?? throw new System.NullReferenceException();
    }

    public override void _Ready()
    {
        UserConfig = new UserConfig();

    }

    public bool AudioOn {
        get => !AudioServer.IsBusMute(0);
        set {
            AudioServer.SetBusMute(0, !value);
        }
    }

    public bool MusicOn {
        get => !AudioServer.IsBusMute(1);
        set {
            AudioServer.SetBusMute(1, !value);
        }
    }

    public void LoadMainLevel() {
        GetTree().ChangeSceneTo(mainLevelScene);
    }

    public void LoadMainMenu() {
        GetTree().ChangeSceneTo(mainMenuScene);
    }
}
