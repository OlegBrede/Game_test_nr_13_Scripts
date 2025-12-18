using Godot;
using System;
using System.ComponentModel;
using System.Linq;

public partial class UNI_AudioStreamPlayer2d : AudioStreamPlayer2D
{
    public SoundControlScript SCS;
    RandomNumberGenerator RNGGEN = new RandomNumberGenerator();
    [Export] int SoundGroup = 1;// grupy dźwiękowe to 0 = Music, 1 = SFX, 2 = Voice
    [Export] AudioStream[] AFSB; // audio from sound bank
    public override void _Ready()
    {
        RNGGEN.Randomize();
        // tu powinien być sound bank load
    }
    float PercentToDb(float percent)
    {
        percent = Mathf.Clamp(percent, 0, 100);
        return Mathf.LinearToDb(percent / 100.0f);
    }
    public void PlaySound(int SoundBankndex)
    {
        if (SoundBankndex < 0 || SoundBankndex >= AFSB.Length)
        {
            GD.PrintErr("Sound index poza zakresem banku");
            return;
        }
        PitchScale = RNGGEN.RandfRange(0.85f,1.15f);
        Stream = AFSB[SoundBankndex];
        if (SCS != null)
        {
            switch (SoundGroup)
            {
                case 0:
                    VolumeDb = PercentToDb(SCS.Music);
                break;
                case 1:
                    VolumeDb = PercentToDb(SCS.SFX);
                break;
                case 2:
                    VolumeDb = PercentToDb(SCS.Voices);
                break;
                default:
                    GD.Print("UNI_AudioStreamPlayer2d nie ma ustawień głośności więc wartość dźwięku będzie ustawiona do tej w edytorze");
                break;
            }
        }
        Play();
    }
}
