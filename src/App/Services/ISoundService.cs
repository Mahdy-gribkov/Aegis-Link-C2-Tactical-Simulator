namespace AegisLink.App.Services;

public interface ISoundService
{
    void PlayBeep();
    void PlayClick();
    void PlayAlert();
    void PlayChirp();
    bool IsMuted { get; set; }
}
