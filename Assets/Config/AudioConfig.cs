using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Config
{
    public interface IAudioConfig
    {
        bool StartMusicOn{ get; }
        bool StartSoundsOn{ get; }
        AudioMixer MasterMixer { get; }
        List<AudioAmbient> AmbientClips { get; }
        List<AudioBackground> BackroundClips { get; }
        List<AudioMetaGame> MetaGameClips { get; }
        List<AudioUI> UIClips { get; }
    }

    [CreateAssetMenu(fileName = "Audio", menuName = "Config/Audio", order = 1)]
    public class AudioConfig : ScriptableObject, IAudioConfig
    {
        [SerializeField]
        private bool _startMusicOn;
        [SerializeField]
        private bool _startSoundsOn;
        
        [SerializeField]
        private AudioMixer _masterMixer;

        [Header("Clips:")]
        [SerializeField]
        private List<AudioAmbient> _ambientClips = new List<AudioAmbient>();
        [SerializeField]
        private List<AudioBackground> _backroundClips = new List<AudioBackground>();
        [SerializeField]
        private List<AudioMetaGame> _metaGameClips = new List<AudioMetaGame>();
        [SerializeField]
        private List<AudioUI> _uiClips = new List<AudioUI>();

        public bool StartMusicOn => _startMusicOn;
        public bool StartSoundsOn => _startSoundsOn;
   

        public AudioMixer MasterMixer => _masterMixer;

        public List<AudioAmbient> AmbientClips => _ambientClips;
        public List<AudioBackground> BackroundClips => _backroundClips;
        public List<AudioMetaGame> MetaGameClips => _metaGameClips;
        public List<AudioUI> UIClips => _uiClips;
    }

    [Serializable]
    public class AudioBackground 
    {
        public BackgroundSoundId Id;
        public AudioClip Clip;
    }
    
    [Serializable]
    public class AudioAmbient 
    {
        public AmbientSoundId Id;
        public AudioClip Clip;
    }
    
    [Serializable]
    public class AudioMetaGame
    {
        public MetaGameSoundId Id;
        public AudioClip Clip;
    }
    
    [Serializable]
    public class AudioUI
    {
        public UISoundId Id;
        public AudioClip Clip;
    }

    public enum BackgroundSoundId
    {
        MusicMainMenu,
        MusicInGame
    }

    public enum AmbientSoundId
    {
        AmbientMainMenu,
        AmbientMusicInGame,
    }
    
    public enum UISoundId
    {
        ButtonClick,
        SoundOn,
        SoundOff,
        MusicOn,
        MusicOff,
        Button_options_on,
        Button_options_off,
        Play,
        UgradeSkills,
        Close,
        CoinAdd,
        XPAdd,
        Win,
        Revive,
        Lose
    }
    
    public enum MetaGameSoundId
    {
        BallKickUnit,
        BallKickBoss,
        UnitKickCanon,
        UnitDeath,
        BossDeath,
        CannonDeath,
        GetBonusBall,
        GetBooster,
        BossShot,
        BomberUnitDeath,
        BomberBossDeath,
        HealerHealing
    }
}