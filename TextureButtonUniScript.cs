using Godot;
using System;

public partial class TextureButtonUniScript : TextureButton
{
    [Export] int ButtonIdNum = 1;
    [Export] Node Menager;
    public override void _Ready()
    {
        Connect("pressed", new Callable(this, nameof(OnACTButtonPressed)));
    }
    void OnACTButtonPressed()
    {
        Menager.Call("TextButton_ACT" + ButtonIdNum);
    }
}
