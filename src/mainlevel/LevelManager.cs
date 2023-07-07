using Godot;

public class LevelManager : Node
{
    private GameManager _gameManager;

    public override void _Ready()
    {
        _gameManager = GetNode<GameManager>("/root/GameManager") ?? throw new System.NullReferenceException();
    }
}
