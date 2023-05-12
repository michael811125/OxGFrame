using OxGFrame.MediaFrame;
using UnityEngine;
using UnityEngine.Audio;

public static class Audio
{
    // If use prefix "res#" will load from resource else will from bundle
    private const string _prefix = "res#";

    // Paths
    private static readonly string _bgmPath = $"{_prefix}Audio/BGM/";
    private static readonly string _generalPah = $"{_prefix}Audio/General/";
    private static readonly string _fightPath = $"{_prefix}Audio/Fight/";
    private static readonly string _voicePath = $"{_prefix}Audio/Voice/";
    private static readonly string _atmospherePath = $"{_prefix}Audio/Atmosphere/";

    // Assets
    public static readonly string BgmExample = $"{_bgmPath}bgm_Example";
    public static readonly string GeneralSoundExample = $"{_generalPah}general_sound_Example";
    public static readonly string FightSoundExample = $"{_fightPath}fight_sound_Example";
    public static readonly string VoiceExample = $"{_voicePath}voice_Example";
    public static readonly string AtmosphereExample = $"{_atmospherePath}atmosphere_Example";
}

public class AudioFrameDemo : MonoBehaviour
{
    private void Awake()
    {
        // If Init instance can more efficiency
        MediaFrames.AudioFrame.InitInstance();
    }

    #region Audio 【BGM】
    public async void PlayBGM()
    {
        await MediaFrames.AudioFrame.Play(Audio.BgmExample);
    }

    public void StopBGM()
    {
        MediaFrames.AudioFrame.Stop(Audio.BgmExample);
    }

    public void StopBGMWithDestroy()
    {
        MediaFrames.AudioFrame.Stop(Audio.BgmExample, false, true);
    }

    public void PauseBGM()
    {
        MediaFrames.AudioFrame.Pause(Audio.BgmExample);
    }
    #endregion

    #region Audio 【GeneralFX】 
    public async void PlayGeneralFX()
    {
        await MediaFrames.AudioFrame.Play(Audio.GeneralSoundExample);
    }

    public void StopGeneralFX()
    {
        MediaFrames.AudioFrame.Stop(Audio.GeneralSoundExample);
    }

    public void StopGeneralFXWithDestroy()
    {
        /*
         * [if Audio is not checked OnDestroyAndUnload, can use ForceUnload to stop and unload]
         * 
         * MediaFrames.AudioFrame.ForceUnload(Audio.GeneralSoundExample);
         */

        MediaFrames.AudioFrame.Stop(Audio.GeneralSoundExample, false, true);
    }

    public void PauseGeneralFX()
    {
        MediaFrames.AudioFrame.Pause(Audio.GeneralSoundExample);
    }
    #endregion

    #region Audio 【FightFX】
    public async void PlayFightFX()
    {
        await MediaFrames.AudioFrame.Play(Audio.FightSoundExample);
    }

    public void StopFightFX()
    {
        MediaFrames.AudioFrame.Stop(Audio.FightSoundExample);
    }

    public void StopFightFXWithDestroy()
    {
        MediaFrames.AudioFrame.Stop(Audio.FightSoundExample, false, true);
    }

    public void PauseFightFX()
    {
        MediaFrames.AudioFrame.Pause(Audio.FightSoundExample);
    }
    #endregion

    #region Audio 【Voice】
    public async void PlayVoice()
    {
        await MediaFrames.AudioFrame.Play(Audio.VoiceExample);
    }

    public void StopVoice()
    {
        MediaFrames.AudioFrame.Stop(Audio.VoiceExample);
    }

    public void StopVoiceWithDestroy()
    {
        MediaFrames.AudioFrame.Stop(Audio.VoiceExample, false, true);
    }

    public void PauseVoice()
    {
        MediaFrames.AudioFrame.Pause(Audio.VoiceExample);
    }
    #endregion

    #region Audio 【Atmosphere】
    public async void PlayAtmosphere()
    {
        await MediaFrames.AudioFrame.Play(Audio.AtmosphereExample);
    }

    public void StopAtmosphere()
    {
        MediaFrames.AudioFrame.Stop(Audio.AtmosphereExample);
    }

    public void StopAtmosphereWithDestroy()
    {
        MediaFrames.AudioFrame.Stop(Audio.AtmosphereExample, false, true);
    }

    public void PauseAtmosphere()
    {
        MediaFrames.AudioFrame.Pause(Audio.AtmosphereExample);
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

    public void SetFightSoundVol(float vol)
    {
        var mixer = MediaFrames.AudioFrame.GetMixerByName("MasterMixer");
        MediaFrames.AudioFrame.SetMixerExposedParam(mixer, "Fight_Vol", vol);
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
