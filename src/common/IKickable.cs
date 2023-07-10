using Godot;

public interface IKickable {
    bool Sleeping {get; set;}

    void Kick(Vector3 position, Vector3 direction);
    
}