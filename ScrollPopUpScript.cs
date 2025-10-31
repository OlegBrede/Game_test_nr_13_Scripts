using Godot;
using System;
using System.Collections.Generic;

public partial class ScrollPopUpScript : Node2D
{
    [Export] VBoxContainer PartExtractor;
    PawnPlayerController Twat;
    public void GeneratePartButtons(List<PawnPart> PartsToShow, PawnPlayerController TwatThatCalled)
    {
        Twat = TwatThatCalled;
        if (PartExtractor.GetChildCount() > 0)// primo Wyczyść listę 
        {
            foreach (var BodyPartButton in PartExtractor.GetChildren())
            {
                BodyPartButton.QueueFree();
            }
        }
        else
        {
            GD.Print("Nie ma po co czyścić listy części ciała");
        }
        if (PartsToShow.Count < 0)
        {
            GD.Print("Nie przesłano części ciała ? teoretycznie niemożliwe");
            return;
        }

        foreach (PawnPart Part in PartsToShow)
        {
            PackedScene PartButtonScene = GD.Load<PackedScene>("res://Prefabs/body_part_instance_button.tscn");
            Button Button = PartButtonScene.Instantiate<Button>();
            PartExtractor.AddChild(Button);
            Button.Call("PrimeButton", PartsToShow.IndexOf(Part), Part.Name, Twat);// WE NEED INDEX HERE, zamiast tej jedynki. Pamiętaj by ten call zsynchronizować z Gamescript na wejściu, potem rozpisz se guziki, pierdol jeśli idiokratycznie, mam to dokończyć do 11-go zanim egzaminy się poważniejsze zaczną
        }
    }
    void Button_ACT1()
    {
        if(Twat != null)
        {
            Twat.ResetActionCommitment(true);
            Visible = false;
        }
    }
}
