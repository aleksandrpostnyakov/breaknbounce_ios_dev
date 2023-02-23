using Config;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Sound
{
    [RequireComponent(typeof(Button))]
    public class SoundUIButtonReactor: MonoBehaviour
    {
        [Inject] private ISoundManager _soundManager;
        
        public UISoundId SoundId;

        private Button _button;

        private void Awake()
        {
            _button = gameObject.GetComponent<Button>();
        }

        private void Start()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            _soundManager.PlayUISound(SoundId);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
        }
    }
}