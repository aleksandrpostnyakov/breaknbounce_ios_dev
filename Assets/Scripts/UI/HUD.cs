using System;
using System.Collections;
using Config;
using DG.Tweening;
using I2.Loc;
using PlayerInfo;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class HUD : MonoBehaviour
    {
        [Inject] private GameConfig _gameConfig;
        
        [SerializeField] private Canvas _canvas;
        [SerializeField] private TMP_Text _infoTxt;
        [SerializeField] private TMP_Text _levelTxt;
        [SerializeField] private TMP_Text _coinsCountTxt;
        [SerializeField] private Button _killBallsBtn;
        [SerializeField] private Button _changeProjection3DBtn;
        [SerializeField] private Button _changeProjection2DBtn;
        [SerializeField] private Button _pauseBtn;
        [SerializeField] private Button _skipBtn;
        [SerializeField] private GameObject _tutorialHand;
        [SerializeField] private GameObject _tutorialHandParent;
        [SerializeField] private Transform _tutorialHandRightPoint;
        [SerializeField] private Transform _tutorialHandLeftPoint;
        [SerializeField] private Transform _bustersTransform;

        public event Action OnKillBalls;
        public event Action OnPause;
        public event Action OnSkip;
        public event Action<bool> OnChangeCameraProjection;

        private Coroutine _showKillButtonRoutine;
        private Sequence _tutorialhandSequence;

        private void Start()
        {
            Hide();
            BossIncoming(false);
            _changeProjection3DBtn.gameObject.SetActive(false);
            _killBallsBtn.onClick.AddListener(OnKIllBallsClick);
            _changeProjection3DBtn.onClick.AddListener(OnChangeCameraProjection3D);
            _changeProjection2DBtn.onClick.AddListener(OnChangeCameraProjection2D);
            _pauseBtn.onClick.AddListener(OnPauseClick);
            _skipBtn.onClick.AddListener(OnSkipClick);
            
            #if !UNITY_EDITOR
            _skipBtn.gameObject.SetActive(false);
            #endif
        }

        private void OnSkipClick()
        {
            OnSkip?.Invoke();
        }

        private void OnKIllBallsClick()
        {
            OnKillBalls?.Invoke();
        }
        
        private void OnPauseClick()
        {
            OnPause?.Invoke();
        }
        
        private void OnChangeCameraProjection3D()
        {
            _changeProjection3DBtn.gameObject.SetActive(false);
            _changeProjection2DBtn.gameObject.SetActive(true);
            OnChangeCameraProjection?.Invoke(false);
        }
        
        private void OnChangeCameraProjection2D()
        {
            _changeProjection3DBtn.gameObject.SetActive(true);
            _changeProjection2DBtn.gameObject.SetActive(false);
            OnChangeCameraProjection?.Invoke(true);
        }
        

        public void Show(bool isTutorial, bool in2D, float bustersY)
        {
            if (isTutorial)
            {
                _changeProjection2DBtn.gameObject.SetActive(false);
                _changeProjection3DBtn.gameObject.SetActive(false);
            }
            else
            {
                _changeProjection2DBtn.gameObject.SetActive(!in2D);
                _changeProjection3DBtn.gameObject.SetActive(in2D);
            }
            
            _pauseBtn.gameObject.SetActive(!isTutorial);
            _levelTxt.gameObject.SetActive(!isTutorial);
            
            _killBallsBtn.gameObject.SetActive(false);
            
            ShowTutorialHand(isTutorial);

            SetBustersPosition(bustersY);
            
            _canvas.enabled = true;
        }
        
        public void Hide()
        {
            _canvas.enabled = false;
        }

        public void ShowInfo(string text, bool showBoss)
        {
            if (showBoss)
            {
                if(LocalizationManager.TryGetTranslation("BnB UI/Hud_BossIncoming", out var txt))
                {
                    _infoTxt.text = txt;
                    _infoTxt.color = Color.red;
                }
            }
            else
            {
                if(LocalizationManager.TryGetTranslation("BnB UI/Hud_Wave", out var txt))
                {
                    _infoTxt.text = string.Format(txt, text);
                    _infoTxt.color = Color.white;
                }
            }

            _infoTxt.enabled = true;
        }
        
        public void BossIncoming(bool show)
        {
            _infoTxt.enabled = show;
        }

        public void ShowLevel(string text)
        {
            if (text == string.Empty)
            {
                _levelTxt.text = "";
            }
            else if(LocalizationManager.TryGetTranslation("BnB UI/Hud_Level", out var txt))
            {
                _levelTxt.text = string.Format(txt, text);
            }
        }

        public void StartFly()
        {
            _killBallsBtn.gameObject.SetActive(false);
            _showKillButtonRoutine = StartCoroutine(ShowKill());
        }

        private IEnumerator ShowKill()
        {
            yield return new WaitForSeconds(_gameConfig.GetHudConfig.KillBallsButtonShowDelay);
            _killBallsBtn.gameObject.SetActive(true);
        }

        public void EndFly()
        {
            _killBallsBtn.gameObject.SetActive(false);
            if (_showKillButtonRoutine != null)
            {
                StopCoroutine(_showKillButtonRoutine);
                _showKillButtonRoutine = null;
            }
        }
        
        public void UpdateCoins(int oldValue, int currentValue)
        {
            _coinsCountTxt.text = currentValue.ToString();
        }

        public void ShowTutorialHand(bool show)
        {
            _tutorialHand.SetActive(show);
            _tutorialHandParent.SetActive(show);
            if (show)
            {
                StartSequence();
            }
            else
            {
                StopSequence();
            }
        }

        public void SetBustersPosition(float bustersY)
        {
            var pos = _bustersTransform.position;
            _bustersTransform.position = new Vector3(pos.x, bustersY, pos.z);
        }

        public Vector3 GetTutorialHandPosition()
        {
            return _tutorialHand.transform.position;
        }

        private void StartSequence()
        {
            _tutorialhandSequence = DOTween.Sequence();
            _tutorialhandSequence.Append(_tutorialHand.transform.DOMoveX(_tutorialHandRightPoint.transform.position.x, 1));
            _tutorialhandSequence.Append(_tutorialHand.transform.DOMoveX(_tutorialHandLeftPoint.transform.position.x, 1));
            _tutorialhandSequence.SetLoops(-1);
            _tutorialhandSequence.Play();
        }
        
        private void StopSequence()
        {
            if (_tutorialhandSequence != null)
            {
                _tutorialhandSequence.Kill();
                _tutorialhandSequence = null;
            }
        }

        private void OnDestroy()
        {
            _killBallsBtn.onClick.RemoveListener(OnKIllBallsClick);
            _changeProjection3DBtn.onClick.RemoveListener(OnChangeCameraProjection3D);
            _changeProjection2DBtn.onClick.RemoveListener(OnChangeCameraProjection2D);
            _pauseBtn.onClick.RemoveListener(OnPauseClick);
            _skipBtn.onClick.RemoveListener(OnSkipClick);
        }
    }
}