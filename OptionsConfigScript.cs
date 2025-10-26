using Godot;
using System;

public partial class OptionsConfigScript : Node2D
{
    [Export] GameMNGR_Script gameMNGR_Script;
    [Export] Node2D ScreenResButton;
    [Export] Node2D ScreenAspectsOptions;
    // ############################## ROZDZIELCZOŚĆ EKRANU #################################
    void Button_ACT1()
    {
        ScreenResButton.Call("OnChangeButtonLabel","Apply",125);
        ScreenResButton.Call("OnChangeButtonFunc",2);
    }
    void Button_ACT2()
    {
        ScreenResButton.Call("OnChangeButtonLabel", "Change", 125);
        ScreenResButton.Call("OnChangeButtonFunc", 1);
    }
    void Button_ACT3()
    {
        gameMNGR_Script.SetResolution(0);
    }
    void Button_ACT4()
    {
        gameMNGR_Script.SetResolution(1);
    }
    void Button_ACT5()
    {
        gameMNGR_Script.SetResolution(2);
    }
    void Button_ACT6()
    {
        gameMNGR_Script.SetResolution(3);
    }
    void Button_ACT7()
    {
        gameMNGR_Script.SetResolution(4);
    }
    void Button_ACT8()
    {
        gameMNGR_Script.SetResolution(5);
    }
    // ############################## ROZDZIELCZOŚĆ EKRANU #################################
}
