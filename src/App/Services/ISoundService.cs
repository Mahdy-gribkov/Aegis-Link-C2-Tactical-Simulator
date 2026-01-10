namespace AegisLink.App.Services;

public interface ISoundService
{
    void PlayBeep();
    void PlayClick();
    void PlayAlert();
    bool IsMuted { get; set; }
}
