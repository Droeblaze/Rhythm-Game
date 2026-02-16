using UnityEngine;

public class SongClock : MonoBehaviour
{
    public AudioSource music;
    public float bpm = 120f;

    private double dspStartTime;

    public double SongTime =>
        AudioSettings.dspTime - dspStartTime;

    public double SongBeat =>
        SongTime / (60f / bpm);

    public void StartSong()
    {
        dspStartTime = AudioSettings.dspTime;
        music.Play();
    }
}

