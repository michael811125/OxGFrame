using OxGFrame.MediaFrame;
using UnityEngine;
using UnityEngine.Audio;

public static class AudioPrefs
{
    // If use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";

    // Paths
    private static readonly string _bgmPath = $"{_prefix}Audio/a_BGM/";
    private static readonly string _generalPath = $"{_prefix}Audio/b_General/";
    private static readonly string _interactPath = $"{_prefix}Audio/c_Interact/";
    private static readonly string _voicePath = $"{_prefix}Audio/d_Voice/";
    private static readonly string _atmospherePath = $"{_prefix}Audio/e_Atmosphere/";

    // Assets
    public static readonly string a101 = $"{_bgmPath}101";
    public static readonly string a201 = $"{_generalPath}201";
    public static readonly string a301 = $"{_interactPath}301";
    public static readonly string a401 = $"{_voicePath}401";
    public static readonly string a501 = $"{_atmospherePath}501";
}

public class AudioFrameDemo : MonoBehaviour
{
    public AudioClip[] clips;

    private void Awake()
    {
        // If Init instance can more efficiency
        MediaFrames.AudioFrame.InitInstance();
    }

    #region Audio 【BGM】
    public async void PlayBGM()
    {
        await MediaFrames.AudioFrame.Play(AudioPrefs.a101);
    }

    public void StopBGM()
    {
        MediaFrames.AudioFrame.Stop(AudioPrefs.a101);
    }

    public void StopBGMWithDestroy()
    {
        MediaFrames.AudioFrame.Stop(AudioPrefs.a101, false, true);
    }

    public void PauseBGM()
    {
        MediaFrames.AudioFrame.Pause(AudioPrefs.a101);
    }
    #endregion

    #region Audio 【General SFX】 
    public async void PlayGeneralFX()
    {
        // You can assign a clip to prefab and play it, or load a clip from prefab and play it
        await MediaFrames.AudioFrame.Play(AudioPrefs.a201, this.clips[0]);
    }

    public void StopGeneralFX()
    {
        MediaFrames.AudioFrame.Stop(AudioPrefs.a201);
    }

    public void StopGeneralFXWithDestroy()
    {
        /*
         * [if Audio is not checked OnDestroyAndUnload, can use ForceUnload to stop and unload]
         * 
         * MediaFrames.AudioFrame.ForceUnload(Audio.GeneralSoundExample);
         */

        MediaFrames.AudioFrame.Stop(AudioPrefs.a201, false, true);
    }

    public void PauseGeneralFX()
    {
        MediaFrames.AudioFrame.Pause(AudioPrefs.a201);
    }
    #endregion

    #region Audio 【Interact SFX】
    public async void PlayFightFX()
    {
        // You can assign a clip to prefab and play it, or load a clip from prefab and play it
        await MediaFrames.AudioFrame.Play(AudioPrefs.a301, this.clips[1]);
    }

    public void StopFightFX()
    {
        MediaFrames.AudioFrame.Stop(AudioPrefs.a301);
    }

    public void StopFightFXWithDestroy()
    {
        MediaFrames.AudioFrame.Stop(AudioPrefs.a301, false, true);
    }

    public void PauseFightFX()
    {
        MediaFrames.AudioFrame.Pause(AudioPrefs.a301);
    }
    #endregion

    #region Audio 【Voice】
    public async void PlayVoice()
    {
        await MediaFrames.AudioFrame.Play(AudioPrefs.a401);
    }

    public void StopVoice()
    {
        MediaFrames.AudioFrame.Stop(AudioPrefs.a401);
    }

    public void StopVoiceWithDestroy()
    {
        MediaFrames.AudioFrame.Stop(AudioPrefs.a401, false, true);
    }

    public void PauseVoice()
    {
        MediaFrames.AudioFrame.Pause(AudioPrefs.a401);
    }
    #endregion

    #region Audio 【Atmosphere】
    public async void PlayAtmosphere()
    {
        await MediaFrames.AudioFrame.Play(AudioPrefs.a501);
    }

    public void StopAtmosphere()
    {
        MediaFrames.AudioFrame.Stop(AudioPrefs.a501);
    }

    public void StopAtmosphereWithDestroy()
    {
        MediaFrames.AudioFrame.Stop(AudioPrefs.a501, false, true);
    }

    public void PauseAtmosphere()
    {
        MediaFrames.AudioFrame.Pause(AudioPrefs.a501);
    }
    #endregion

    #region Control All Audio
    public void ResumeAll()
    {
        MediaFrames.AudioFrame.ResumeAll();
    }

    public void StopAll()
    {
        MediaFrames.AudioFrame.StopAll();
    }

    public void StopAllWithDestroy()
    {
        MediaFrames.AudioFrame.StopAll(false, true);
    }

    public void PauseAll()
    {
        MediaFrames.AudioFrame.PauseAll();
    }
    #endregion

    #region Mixer Snapshot
    public void SnapshotNormal()
    {
        var mixer = MediaFrames.AudioFrame.GetMixerByName("MasterMixer");
        MediaFrames.AudioFrame.AutoRestoreMixerExposedParams(mixer);
        MediaFrames.AudioFrame.SetMixerSnapshot(mixer, "Normal");
    }

    public void SnapshotPaused()
    {
        var mixer = MediaFrames.AudioFrame.GetMixerByName("MasterMixer");
        MediaFrames.AudioFrame.AutoClearMixerExposedParams(mixer);
        MediaFrames.AudioFrame.SetMixerSnapshot(mixer, "Paused");
    }

    public void SnapshotReverb()
    {
        var mixer = MediaFrames.AudioFrame.GetMixerByName("MasterMixer");
        MediaFrames.AudioFrame.AutoClearMixerExposedParams(mixer);
        MediaFrames.AudioFrame.SetMixerSnapshot(mixer, "Reverb_Heavy");
    }
    #endregion

    #region Mixer Volume
    public void SetMasterVol(float vol)
    {
        var mixer = MediaFrames.AudioFrame.GetMixerByName("MasterMixer");
        MediaFrames.AudioFrame.SetMixerExposedParam(mixer, "Master_Vol", vol);
    }

    public void SetBGMVol(float vol)
    {
        var mixer = MediaFrames.AudioFrame.GetMixerByName("MasterMixer");
        MediaFrames.AudioFrame.SetMixerExposedParam(mixer, "BGM_Vol", vol);
    }

    public void SetGeneralSoundVol(float vol)
    {
        var mixer = MediaFrames.AudioFrame.GetMixerByName("MasterMixer");
        MediaFrames.AudioFrame.SetMixerExposedParam(mixer, "General_Vol", vol);
    }

    public void SetInteractSoundVol(float vol)
    {
        var mixer = MediaFrames.AudioFrame.GetMixerByName("MasterMixer");
        MediaFrames.AudioFrame.SetMixerExposedParam(mixer, "Interact_Vol", vol);
    }

    public void SetVoiceVol(float vol)
    {
        var mixer = MediaFrames.AudioFrame.GetMixerByName("MasterMixer");
        MediaFrames.AudioFrame.SetMixerExposedParam(mixer, "Voice_Vol", vol);
    }

    public void SetAtmosphereVol(float vol)
    {
        var mixer = MediaFrames.AudioFrame.GetMixerByName("MasterMixer");
        MediaFrames.AudioFrame.SetMixerExposedParam(mixer, "Atmosphere_Vol", vol);
    }
    #endregion

    #region Mixer Snapshot Weight
    public void SetReverbWeight(float val)
    {
        var mixer = MediaFrames.AudioFrame.GetMixerByName("MasterMixer");

        AudioMixerSnapshot[] snapshots = new AudioMixerSnapshot[]
        {
            MediaFrames.AudioFrame.GetMixerSnapshot(mixer, "Reverb_Light"),
            MediaFrames.AudioFrame.GetMixerSnapshot(mixer, "Reverb_Heavy")
        };

        float min = (1 - val);
        float max = val;
        float[] weights = new float[]
        {
            min,
            max
        };

        MediaFrames.AudioFrame.SetMixerTransitionToSnapshot(mixer, snapshots, weights);
    }
    #endregion
}
