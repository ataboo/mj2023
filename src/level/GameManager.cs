using Godot;

public class GameManager : Node
{
    public UserConfig UserConfig {get; private set;}

    public static GameManager MustGetNode(Node self) {
        return self.GetNode<GameManager>("/root/GameManager") ?? throw new System.NullReferenceException();
    }

    public override void _Ready()
    {
        UserConfig = new UserConfig();
        
    }
}
