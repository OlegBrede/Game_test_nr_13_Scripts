using Godot;
using System;

public partial class NodeCompButtonUni1FuncScript : Node2D
{
	[Export] int ButtonIdNum = 1;
	[Export] int currentsize = 125;
	[Export] string ChangedLabel = "qNullq\nqNullq";
	[Export] private NodePath ParentPath;
	Node Menager;
	TextureButton TextButtRef;
	Label ButtonLabelRef;

	public override void _Ready()
	{
		TextButtRef = GetNode<TextureButton>("TextureButton");
		TextButtRef.Connect("pressed", new Callable(this, nameof(OnACTButtonPressed)));
		TextButtRef.Disabled = false;
		ButtonLabelRef = GetNode<Label>("Label");
		Menager = GetNode<Node>(ParentPath);
		ButtonLabelRef.Text = ChangedLabel;
		currentsize = ButtonLabelRef.GetThemeFontSize("font_size");
		ButtonLabelRef.AddThemeFontSizeOverride("font_size", currentsize);
	}
	void OnACTButtonPressed()
	{
		//GD.Print("ACT ON PRESS");
		Menager.Call("Button_ACT" + ButtonIdNum);
		//tutaj mogą być przykładowe błedy w odniesieniu do wejścia przycisków dla gracza
	}
	void OnChangeButtonFunc(int NowCurrentNum)
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
		GD.Print("BUTTON DISABLED");
		TextButtRef.Disabled = true;
	}
	void ButtonVisibility(bool visible)
	{
		Visible = visible; 
	}
}
