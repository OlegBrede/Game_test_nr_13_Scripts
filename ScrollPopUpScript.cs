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
            if(Part.HP > 0)
            {
                PackedScene PartButtonScene = GD.Load<PackedScene>("res://Prefabs/body_part_instance_button.tscn");
                Button Button = PartButtonScene.Instantiate<Button>();
                PartExtractor.AddChild(Button);
                Button.Call("PrimeButton", PartsToShow.IndexOf(Part), Part.Name,LocationRollCalc(PartsToShow,Part.Name), Twat);
            }
        }
    }
    float LocationRollCalc(List<PawnPart> PartsToShow,string WANTED_P_NAME)
    {
        //GD.Print($"Szukanie prawdopodobieństwa dla {WANTED_P_NAME}");
        List<string> LocationHitProbabilitytable = new List<string>();
        foreach (var Part in PartsToShow)
        {
            for (int i = 0; i < Part.ChanceToHit; i++) // im więcej razy dany element pojawi się na liście tym łatwiej go wylosować 
            {
                LocationHitProbabilitytable.Add(Part.Name);
                //GD.Print($"dodano do listy {Part.Name}");
            }
        }
        int prob = 0;
        foreach (string NAME in LocationHitProbabilitytable)
        {
            if (NAME == WANTED_P_NAME)
            {
                prob++;
            }
        }
        float probf = prob;
        float LHPTF = LocationHitProbabilitytable.Count;
        //GD.Print($"prawdopodobieństwo dla {WANTED_P_NAME} to {prob}");
        float HitChance = probf / LHPTF;
        //GD.Print($"HitChance wynosi {HitChance}");
        LocationHitProbabilitytable.Clear(); // na wszelki wypadek
        return HitChance;
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
