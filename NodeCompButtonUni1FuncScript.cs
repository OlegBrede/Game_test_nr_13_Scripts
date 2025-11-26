using Godot;
using System;

public partial class NodeCompButtonUni1FuncScript : Node2D
{
	[Export] int ButtonIdNum = 1;
	[Export] int currentsize = 125;
	[Export] string ChangedLabel = "qNullq\nqNullq";
	[Export] private NodePath ParentPath;
	[Export] private string IconPath = " ";
	Node Menager;
	TextureButton TextButtRef;
	Label ButtonLabelRef;
	Sprite2D ButtonIcon;
	public override void _Ready()
	{
		ButtonIcon = GetNode<Sprite2D>("ButtonSprite");
		ButtonIcon.Visible = false;
        if (IconPath != " ")
        {
			GD.Print("BUTTON ICON TEXTURE SET");
			Texture2D ButTex = GD.Load<Texture2D>(IconPath);
            if (ButTex != null)
            {
				ButtonIcon.Texture = ButTex;
				ButtonIcon.Visible = true;
            }
            else
            {
                GD.PrintErr($"Nie udało się wczytać tekstury z: {IconPath}");
            }
        }
		TextButtRef = GetNode<TextureButton>("TextureButton");
		TextButtRef.Connect("pressed", new Callable(this, nameof(OnACTButtonPressed)));
		TextButtRef.Disabled = false;
		ButtonLabelRef = GetNode<Label>("Label");
		Menager = GetNode<Node>(ParentPath);
		ButtonLabelRef.Text = ChangedLabel;
		ButtonLabelRef.AddThemeFontSizeOverride("font_size", currentsize);
	}
	void OnACTButtonPressed()
	{
		GD.Print($"ACT{ButtonIdNum} ON PRESS");
		Menager.Call("Button_ACT" + ButtonIdNum);
		//tutaj mogą być przykładowe błedy w odniesieniu do wejścia przycisków dla gracza
	}
	public void OnChangeButtonFunc(int NowCurrentNum)
	{
		ButtonIdNum = NowCurrentNum;
		GD.Print("Przycisk Zmienił funkcję na " + ButtonIdNum);
	}
	void OnChangeButtonLabel(string NewLabel, int Size)
	{
		//GD.Print("BUTTON PRESS");
		ButtonLabelRef.Text = NewLabel;
		currentsize = Size;
		ButtonLabelRef.AddThemeFontSizeOverride("font_size", currentsize);

	}
	void OnDisablebutton()
	{
		//GD.Print("BUTTON DISABLED");
		TextButtRef.Disabled = true;
	}
	void OnEnablebutton()
    {
        //GD.Print("BUTTON ENABLED");
		TextButtRef.Disabled = false;
    }
	void ButtonVisibility(bool visible)
	{
		Visible = visible; 
	}
}
