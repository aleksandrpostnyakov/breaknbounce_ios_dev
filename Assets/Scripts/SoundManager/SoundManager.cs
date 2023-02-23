using System;
using System.Linq;
using Config;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Sound
{
    public interface ISoundManager
    {
        void ToggleSounds();
        void ToggleMusic();
        void ToggleVibro();
        SoundManagerSettings Settings { get; }
        event Action<bool> Changed;
        void Init(SoundManagerSettings settings);
        void PlayUISound(UISoundId clipId, bool loop = false);
        void PlayMetaGameSound(MetaGameSoundId clipId, bool loop = false);
        void PlayAmbientMusic(AmbientSoundId clipId, bool loop = true);
        void PlayBackgroundMusic(BackgroundSoundId clipId, bool loop = true);
        void SetSilent(bool isSilent, bool adsSilent = false, string source = "unknown");
    }
    public class SoundManager : MonoBehaviour, ISoundManager
    {
        public SoundManagerSettings Settings => _settings;

        public event Action<bool> Changed;

        [Inject] private IAudioConfig _audiocConfig;
        [Inject] private WebGLProviderService _webGLProviderService;

        [SerializeField] private AudioSource AmbientSource;
        [SerializeField] private AudioSource BackroundSource;
        [SerializeField] private AudioSource MetaGameSource;
        [SerializeField] private AudioSource UISource;

        private SoundManagerSettings _settings;
        private string _currentBackgroundClipId = string.Empty;
        private string _currentAmbientClipId = string.Empty;

        private void SetMusicMode()
        {
            if (BackroundSource != null)
            {
                BackroundSource.volume = _settings.MusicOn ? 1 : .0f;
            }
        }
        
        private void SetSoundsMode()
        {
            if (AmbientSource != null)
            {
                AmbientSource.volume = _settings.SoundsOn ? 1 : 0;
            }

            if (UISource != null)
            {
                UISource.volume = _settings.SoundsOn ? 1 : 0;
            }

            if (MetaGameSource != null)
            {
                MetaGameSource.volume = _settings.SoundsOn ? 1 : 0;
            }
        }

        private bool _adsSilent;
        public void SetSilent(bool isSilent, bool adsSilent = false, string source = "unknown")
        {
            //Debug.Log("SET SILENT " + isSilent + " " + adsSilent + " " + source);
            if (_adsSilent)
            {
                if (!adsSilent)
                {
                    return;
                }

                if (!isSilent)
                {
                    _adsSilent = false;
                }
            }
            else
            {
                if (adsSilent)
                {
                    _adsSilent = true;
                }
            }
            
            if (!isSilent)
            {
                SetMusicMode();
                SetSoundsMode();
            }
            else
            {
                BackroundSource.volume = 0f;
                AmbientSource.volume = 0f;
                UISource.volume = 0f;
                MetaGameSource.volume = 0f;
            }
        }

        public void Init(SoundManagerSettings settings)
        {
            _settings = settings ?? new SoundManagerSettings(){SoundsOn = _audiocConfig.StartSoundsOn, MusicOn = _audiocConfig.StartMusicOn};
            SetMusicMode();
            SetSoundsMode();
            
            //var random = (BackgroundSoundId)Random.Range(0, Enum.GetValues(typeof(BackgroundSoundId)).Length);
            PlayBackgroundMusic(BackgroundSoundId.MusicMainMenu);
            //var arandom = (AmbientSoundId)Random.Range(0, Enum.GetValues(typeof(AmbientSoundId)).Length);
            PlayAmbientMusic(AmbientSoundId.AmbientMainMenu);
            
            Changed?.Invoke(false);
            
            Application.focusChanged += ApplicationOnfocusChanged;
            _webGLProviderService.FrameOnVisibilityChange += WebGLProviderServiceOnFrameOnVisibilityChange;
        }

        private void WebGLProviderServiceOnFrameOnVisibilityChange(bool isVisible)
        {
            SetSilent(!isVisible, false, "sm_web");
        }

        private void ApplicationOnfocusChanged(bool isFocus)
        {
            SetSilent(!isFocus, false, "sm_af");
        }

        public void ToggleMusic()
        {
            _settings.MusicOn = !_settings.MusicOn;
            
            if (!_settings.MusicOn)
            {
                PlayUISound(UISoundId.Button_options_off);
                SetMusicMode();
            }
            else
            {
                SetMusicMode();
                PlayUISound(UISoundId.Button_options_on);
            }
            Changed?.Invoke(true);
        }

        public async void ToggleSounds()
        {
            _settings.SoundsOn = !_settings.SoundsOn;
            if (!_settings.SoundsOn)
            {
                PlayUISound(UISoundId.Button_options_off);
                await new WaitForSeconds(.2f);
                SetSoundsMode();
            }
            else 
            {
                SetSoundsMode();
                PlayUISound(UISoundId.Button_options_on);
            }
            
            Changed?.Invoke(true);
        }

        public void ToggleVibro()
        {
            _settings.VibroOn = !_settings.VibroOn;
            PlayUISound(_settings.VibroOn ? UISoundId.Button_options_on : UISoundId.Button_options_off);
            Changed?.Invoke(true);
        }
        
        public void PlayBackgroundMusic(BackgroundSoundId clipId, bool loop = true) 
        {
            var config = _audiocConfig.BackroundClips.FirstOrDefault(c => c.Id == clipId);
            if (config != null && _currentBackgroundClipId != config.Clip.name)
            {
                _currentBackgroundClipId = config.Clip.name;
                PlaySound(BackroundSource, config.Clip, loop, true);
            }
        }
        
        public void PlayAmbientMusic(AmbientSoundId clipId, bool loop = true)
        {
            AmbientSource.clip = null;
            var config = _audiocConfig.AmbientClips.FirstOrDefault(c => c.Id == clipId);
            if (config != null && _currentAmbientClipId != config.Clip.name)
            {
                _currentAmbientClipId = config.Clip.name;
                PlaySound(AmbientSource, config.Clip, loop, false);
            }
        }

        public void PlayUISound(UISoundId clipId, bool loop = false) 
        {
            Debug.Log("PUIS " + clipId.ToString());
            var config = _audiocConfig.UIClips.FirstOrDefault(c => c.Id == clipId);
            if (config != null)
            {
                UISource.PlayOneShot(config.Clip);
            }
        }
        
        public void PlayMetaGameSound(MetaGameSoundId clipId, bool loop = false) 
        {
            Debug.Log("PMS " + clipId.ToString());
            var config = _audiocConfig.MetaGameClips.FirstOrDefault(c => c.Id == clipId);
            if (config != null)
            {
                MetaGameSource.PlayOneShot(config.Clip);
            }
        }


        private void PlaySound(AudioSource source, AudioClip clip, bool loop, bool isMusic)
        {
            source.clip = clip;
            source.loop = loop;
            source.Play();
        }

        private void OnDestroy()
        {
            Application.focusChanged -= ApplicationOnfocusChanged;
            _webGLProviderService.FrameOnVisibilityChange -= WebGLProviderServiceOnFrameOnVisibilityChange;
        }
    }

    [Serializable]
    public class SoundManagerSettings
    {
        public bool MusicOn = true;
        public bool SoundsOn = true;
        public bool VibroOn = true;
    }
}