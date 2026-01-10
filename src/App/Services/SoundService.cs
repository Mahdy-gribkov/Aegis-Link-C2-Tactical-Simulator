using System.Media;

namespace AegisLink.App.Services;

public class SoundService : ISoundService
{
    public bool IsMuted { get; set; } = false;

    public void PlayBeep()
    {
        if (IsMuted) return;
        SystemSounds.Beep.Play();
    }

    public void PlayClick()
    {
        if (IsMuted) return;
        SystemSounds.Asterisk.Play();
    }

    public void PlayAlert()
    {
        if (IsMuted) return;
        SystemSounds.Exclamation.Play();
    }
}
