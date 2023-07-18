using Godot;
using System;

[Tool]
public class ButtonCheckbox : PanelContainer
{
    [Signal]
    public delegate void OnChecked(bool @checked);

    private bool _checked;

    [Export]
    public bool Checked {
        get => _checked;
        set {
            _checked = value;
            if(CheckBox != null) {
                CheckBox.Pressed = _checked;
            }
        }
    }

    private string _labelText = "Label";
    [Export]
    private string labelText {
        get => _labelText;
        set {
            _labelText = value;
            if(Label != null) {
                Label.Text = value;
            }
        }
    }

    [Export]
    private NodePath checkboxPath;
    private CheckBox _checkBox;
    public CheckBox CheckBox {
        get {
            if(_checkBox == null && checkboxPath != null) {
                _checkBox = GetNode<CheckBox>(checkboxPath);
            }

            return _checkBox;
        }
    }

    [Export]
    private NodePath labelPath;
    private Label _label;
    public Label Label {
        get {
            if(_label == null && labelPath != null) {
                _label = GetNode<Label>(labelPath);
            }

            return _label;
        }
    }

    public override void _Ready()
    {
        CheckBox.Pressed = _checked;
        Label.Text = labelText;

        Connect("gui_input", this, nameof(_HandleGUIInput));
    }

    private void _HandleGUIInput(InputEvent @event) {
        if(@event is InputEventMouseButton button && button.IsPressed()) {
            Checked = !Checked;
            EmitSignal(nameof(OnChecked), _checked);
        }
    }

    private void _HandleCheckboxPress() {
        Checked = !Checked;
        EmitSignal(nameof(OnChecked), _checked);
    }
}
