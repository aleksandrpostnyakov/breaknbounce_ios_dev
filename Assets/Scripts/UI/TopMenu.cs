using System;
using DG.Tweening;
using I2.Loc;
using PlayerInfo;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI
{
    public class TopMenu : MonoBehaviour
    {
        [Inject] private PlayerGameInfo _playerInfo;
        [Inject] private CameraHandler.CameraHandler _cameraHandler;
        
        [SerializeField] private Transform _coinsTransform;
        [SerializeField] private Transform _xpTransform;
        [SerializeField] private Transform _levelTransform;
        [SerializeField] private TMP_Text _coinsTxt;
        [SerializeField] private TMP_Text _xpTxt;
        [SerializeField] private TMP_Text _levelTxt;
        [SerializeField] private Color _upColor = Color.green;
        [SerializeField] private Color _downColor = Color.red;

        private int _coinsCount = 0;
        private int _xpCount = 0;
        private Tweener _coinsTweener;
        private Tweener _xpTweener;

        private void Awake()
        {
            _playerInfo.XPChanged += UpdateXP;
            _playerInfo.CoinsChanged += UpdateCoins;

            var coef = _cameraHandler.GetAspect() / .5625f;
            if (coef > 1)
            {
                coef = 1 / coef;
            }

            _coinsTransform.localScale = new Vector3(coef, coef, 0);
            _xpTransform.localScale = new Vector3(coef, coef, 0);
            if (_levelTransform != null)
            {
                _levelTransform.localScale = new Vector3(coef, coef, 0);
            }
        }

        public void Init(bool showPreviousLevel = false)
        {
            _coinsTxt.text = _playerInfo.GetCoins.ToString();
            _xpTxt.text = _playerInfo.GetXP.ToString();
            if (_levelTxt != null)
            {
                if(LocalizationManager.TryGetTranslation("BnB UI/MainScreen_Level", out var txt))
                {
                    _levelTxt.text = string.Format(txt, _playerInfo.CurrentLevel - (showPreviousLevel ? 1 : 0));
                }
            }
        }

        private void UpdateCoins()
        {
            var currCoins = _playerInfo.GetCoins;
            if (_coinsCount == currCoins)
            {
                return;
            }
            
            if (_coinsTweener != null)
            {
                _coinsTweener.Kill();
                _coinsTweener = null;
            }

            _coinsTxt.color = _coinsCount < currCoins ? _upColor : _downColor;
                
                _coinsTweener = DOTween.To((value) =>
                    {
                        _coinsTxt.text = ((int)value).ToString();
                    }, _coinsCount, currCoins, 2f)
                    .OnComplete(() =>
                    {
                        _coinsTxt.text = currCoins.ToString();
                        _coinsTxt.color = Color.white;
                        _coinsCount = currCoins;
                    });
        }
        
        private void UpdateXP()
        {
            var currXP = _playerInfo.GetXP;
            if (_xpCount == currXP)
            {
                return;
            }
            
            _xpTxt.text = currXP.ToString();
            
            // if (_xpTweener != null)
            // {
            //     _xpTweener.Kill();
            //     _xpTweener = null;
            //
            //     _xpTxt.color = _xpCount < currXP ? Color.green : Color.red;
            //     
            //     _xpTweener = DOTween.To((value) =>
            //         {
            //             _xpTxt.text = ((int)value).ToString();
            //         }, _xpCount, currXP, 2f)
            //         .OnComplete(() =>
            //         {
            //             _xpTxt.color = Color.white;
            //             _xpCount = currXP;
            //         });
            // }
        }

        private void OnDestroy()
        {
            _playerInfo.XPChanged -= UpdateXP;
            _playerInfo.CoinsChanged -= UpdateCoins;
        }
    }
}