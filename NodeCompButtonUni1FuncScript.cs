using Godot;
using System;

public partial class NodeCompButtonUni1FuncScript : Node2D
{
	[Export] int ButtonIdNum = 1;
	[Export] string ChangedLabel = "qNullq\nqNullq";
	[Export] private NodePath ButtonLabelPath;
	[Export] private NodePath TextButtPath;
	[Export] private NodePath ParentPath;
	Node Menager;
	TextureButton TextButtRef;
	Label ButtonLabelRef;
	public override void _Ready()
	{
		TextButtRef = GetNode<TextureButton>(TextButtPath);
		TextButtRef.Connect("pressed", new Callable(this, nameof(OnACTButtonPressed)));
		TextButtRef.Disabled = false;
		ButtonLabelRef = GetNode<Label>(ButtonLabelPath);
		Menager = GetNode<Node>(ParentPath);
		ButtonLabelRef.Text = ChangedLabel;
	}
	void OnACTButtonPressed(){
		//GD.Print("ACT ON PRESS");
		Menager.Call("Button_ACT"+ButtonIdNum);
		//tutaj mogą być przykładowe błedy w odniesieniu do wejścia przycisków dla gracza
	}
	void OnChangeButtonFunc(int NowCurrentNum){
		ButtonIdNum = NowCurrentNum;
		GD.Print("Przycisk Zmienił funkcję na " + ButtonIdNum);
	}
	void OnChangeButtonLabel(string NewLabel){
		GD.Print("BUTTON PRESS");
		ButtonLabelRef.Text = NewLabel;
	}
	void OnDisablebutton(){
		GD.Print("BUTTON DISABLED");
		TextButtRef.Disabled = true;
	}
}
