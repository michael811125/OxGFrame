using OxGFrame.MediaFrame;
using UnityEngine;
using UnityEngine.Audio;

public class AudioFrameDemo : MonoBehaviour
{
    // if use prefix "res#" will load from resource else will from bundle
    public static string prefix = "res#";
    public static string BGM_PATH = $"{prefix}Audio/BGM/";
    public static string GERNERAL_SOUNDS_PATH = $"{prefix}Audio/General/";
    public static string FIGHT_SOUNDS_PATH = $"{prefix}Audio/Fight/";
    public static string VOICES_PATH = $"{prefix}Audio/Voice/";
    public static string ATMOSPHERE_PATH = $"{prefix}Audio/Atmosphere/";

    #region Audio 【BGM】
    public async void PlayBGM()
    {
        await MediaFrames.AudioFrame.Play(BGM_PATH + "bgm_Example");
    }

    public void StopBGM()
    {
        MediaFrames.AudioFrame.Stop(BGM_PATH + "bgm_Example");
    }

    public void StopBGMWithDestroy()
    {
        MediaFrames.AudioFrame.Stop(BGM_PATH + "bgm_Example", false, true);
    }

    public void PauseBGM()
    {
        MediaFrames.AudioFrame.Pause(BGM_PATH + "bgm_Example");
    }
    #endregion

    #region Audio 【GeneralFX】 
    public async void PlayGeneralFX()
    {
        await MediaFrames.AudioFrame.Play(GERNERAL_SOUNDS_PATH + "general_sound_Example");
    }

    public void StopGeneralFX()
    {
        MediaFrames.AudioFrame.Stop(GERNERAL_SOUNDS_PATH + "general_sound_Example");
    }

    public void StopGeneralFXWithDestroy()
    {
        // if Audio is not checked OnDestroyAndUnload, can use ForceUnload to stop and unload
        //MediaFrames.AudioFrame.ForceUnload(GERNERAL_SOUNDS_PATH + "general_sound_Example");

        MediaFrames.AudioFrame.Stop(GERNERAL_SOUNDS_PATH + "general_sound_Example", false, true);
    }

    public void PauseGeneralFX()
    {
        MediaFrames.AudioFrame.Pause(GERNERAL_SOUNDS_PATH + "general_sound_Example");
    }
    #endregion

    #region Audio 【FightFX】
    public async void PlayFightFX()
    {
        await MediaFrames.AudioFrame.Play(FIGHT_SOUNDS_PATH + "fight_sound_Example");
    }

    public void StopFightFX()
    {
        MediaFrames.AudioFrame.Stop(FIGHT_SOUNDS_PATH + "fight_sound_Example");
    }

    public void StopFightFXWithDestroy()
    {
        MediaFrames.AudioFrame.Stop(FIGHT_SOUNDS_PATH + "fight_sound_Example", false, true);
    }

    public void PauseFightFX()
    {
        MediaFrames.AudioFrame.Pause(FIGHT_SOUNDS_PATH + "fight_sound_Example");
    }
    #endregion

    #region Audio 【Voice】
    public async void PlayVoice()
    {
        await MediaFrames.AudioFrame.Play(VOICES_PATH + "voice_Example");
    }

    public void StopVoice()
    {
        MediaFrames.AudioFrame.Stop(VOICES_PATH + "voice_Example");
    }

    public void StopVoiceWithDestroy()
    {
        MediaFrames.AudioFrame.Stop(VOICES_PATH + "voice_Example", false, true);
    }

    public void PauseVoice()
    {
        MediaFrames.AudioFrame.Pause(VOICES_PATH + "voice_Example");
    }
    #endregion

    #region Audio 【Atmosphere】
    public async void PlayAtmosphere()
    {
        await MediaFrames.AudioFrame.Play(ATMOSPHERE_PATH + "atmosphere_Example");
    }

    public void StopAtmosphere()
    {
        MediaFrames.AudioFrame.Stop(ATMOSPHERE_PATH + "atmosphere_Example");
    }

    public void StopAtmosphereWithDestroy()
    {
        MediaFrames.AudioFrame.Stop(ATMOSPHERE_PATH + "atmosphere_Example", false, true);
    }

    public void PauseAtmosphere()
    {
        MediaFrames.AudioFrame.Pause(ATMOSPHERE_PATH + "atmosphere_Example");
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
