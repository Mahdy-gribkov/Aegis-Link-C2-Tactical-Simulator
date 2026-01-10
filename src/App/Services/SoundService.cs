using System.Media;

namespace AegisLink.App.Services;

public class SoundService : ISoundService
{
    private DateTime _lastBeepTime = DateTime.MinValue;
    private DateTime _lastClickTime = DateTime.MinValue;
    private DateTime _lastAlertTime = DateTime.MinValue;
    private const int ThrottleMs = 2000;

    public bool IsMuted { get; set; } = false;

    public void PlayBeep()
    {
        if (IsMuted) return;
        if ((DateTime.Now - _lastBeepTime).TotalMilliseconds < ThrottleMs) return;
        _lastBeepTime = DateTime.Now;
        SystemSounds.Beep.Play();
    }

    public void PlayClick()
    {
        if (IsMuted) return;
        if ((DateTime.Now - _lastClickTime).TotalMilliseconds < 200) return;
        _lastClickTime = DateTime.Now;
        SystemSounds.Asterisk.Play();
    }

    public void PlayAlert()
    {
        if (IsMuted) return;
        if ((DateTime.Now - _lastAlertTime).TotalMilliseconds < ThrottleMs) return;
        _lastAlertTime = DateTime.Now;
        SystemSounds.Exclamation.Play();
    }

    public void PlayChirp()
    {
        if (IsMuted) return;
        if ((DateTime.Now - _lastBeepTime).TotalMilliseconds < ThrottleMs) return;
        _lastBeepTime = DateTime.Now;
        SystemSounds.Hand.Play();
    }
}
