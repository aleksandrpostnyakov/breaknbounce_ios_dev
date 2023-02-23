using System;
using System.Linq;
using Config;
using Core;
using I2.Loc;
using PlayerInfo;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class UpgradeCard : MonoBehaviour
    {
        [Inject] private PlayerGameInfo _playerGameInfo;
        [Inject] private GameConfig _gameConfig;
        [Inject] private IAdsSystem _adsSystem;
        [Inject(Id = "PlatformType")] PlatformType _platformType;
        
        [SerializeField] private TMP_Text _countTxt;
        [SerializeField] private TMP_Text _descriptionTxt;
        [SerializeField] private TMP_Text _buyBtnTxt;
        [SerializeField] private Button _buyBtn;
        [SerializeField] private Button _adsBtn;
        [SerializeField] private Image _cardImage;
        [SerializeField] private Image _btnImage;
        [SerializeField] private Sprite _activeImage;
        [SerializeField] private Sprite _passiveImage;
        [SerializeField] private Sprite _activeBtnImage;
        [SerializeField] private Sprite _passiveBtnImage;
        [SerializeField] private UpgradeType _upgradeType;

        private int _cost;

        public Action<UpgradeType> OnUpgrade;

        private void Start()
        {
            _buyBtn.onClick.AddListener(OnBuyClick);
            _adsBtn.onClick.AddListener(OnAdsClick);
        }

        public bool Init(bool ads)
        {
            SkillConfig config;
            int count;

            switch (_upgradeType)
            {
                case UpgradeType.Shield:
                    _countTxt.text = _playerGameInfo.GetHealth().ToString();
                    config = _gameConfig.GetShieldConfig;
                    if(LocalizationManager.TryGetTranslation("BnB UI/Upgrades_HP", out var txt))
                    {
                        _descriptionTxt.text = string.Format(txt, config.IncrementStep);
                    }
                    count = _playerGameInfo.GetHealthLevel;
                    break;
                case UpgradeType.Pointer:
                    _countTxt.text = _playerGameInfo.GetDots().ToString();
                    config = _gameConfig.GetDotsConfig;
                    if(LocalizationManager.TryGetTranslation("BnB UI/Upgrades_POINT", out var txt1))
                    {
                        _descriptionTxt.text = string.Format(txt1, config.IncrementStep);
                    }
                    count = _playerGameInfo.GetDotsLevel;
                    break;
                case UpgradeType.Ball:
                    _countTxt.text = _playerGameInfo.GetBalls().ToString();
                    config = _gameConfig.GetBallsConfig;
                    if(LocalizationManager.TryGetTranslation("BnB UI/Upgrades_BALL", out var txt2))
                    {
                        _descriptionTxt.text = string.Format(txt2, config.IncrementStep);
                    }
                    count = _playerGameInfo.GetBallsLevel;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_upgradeType), _upgradeType, null);
            }

            var coef = config.CostIncreaseCoefficients.Last().Cost;
            for (var i = 0; i <= config.CostIncreaseCoefficients.Length; i++)
            {
                if (count < config.CostIncreaseCoefficients[i].Level)
                {
                    coef = config.CostIncreaseCoefficients[i].Cost;
                    break;
                }
            }

            _cost = config.StartCostOfUpgrade + coef * count;
            var active = _cost <= _playerGameInfo.GetCoins || ads;
            _buyBtnTxt.text = _cost.ToString();
            _cardImage.sprite = active ? _activeImage : _passiveImage;
            _btnImage.sprite = active ? _activeBtnImage : _passiveBtnImage;
            
            if (ads)
            {
                _adsBtn.gameObject.SetActive(true);
                _buyBtn.gameObject.SetActive(false);
            }
            else
            {
                _adsBtn.gameObject.SetActive(false);
                _buyBtn.gameObject.SetActive(true);
                _buyBtn.enabled = active;
            }

            return active;
        }

        public Vector3 GetBuyBtnPosition()
        {
            return _buyBtn.transform.position;
        }
        
        private void OnAdsClick()
        {
            ShowAds();
        }
        
        private async void ShowAds()
        {
            var upgradeId = _upgradeType switch
            {
                UpgradeType.Shield => 1,
                UpgradeType.Pointer => 2,
                UpgradeType.Ball => 3,
                _ => throw new ArgumentOutOfRangeException()
            };
            if (_platformType == PlatformType.AndroidAds)
            {
                _adsSystem.OnShowRewardedAds -= AdsSystemOnOnShowRewardedAds;
                _adsSystem.OnShowRewardedAds += AdsSystemOnOnShowRewardedAds;
                await _adsSystem.ShowAdsForUpgrade(upgradeId);
            }
            else
            {
                var result = await _adsSystem.ShowAdsForUpgrade(upgradeId);
                if (result)
                {
                    OnAdsReward();
                }
            }
        }

        private void AdsSystemOnOnShowRewardedAds(BaseAdsSystem.AdsType adsType, bool result)
        {
            if (adsType == BaseAdsSystem.AdsType.UpgradeCard)
            {
                _adsSystem.OnShowRewardedAds -= AdsSystemOnOnShowRewardedAds;
                if (result)
                {
                    
                    OnAdsReward();
                }
            }
        }

        private void OnAdsReward()
        {
            _playerGameInfo.ChangeUpgradeAds(_upgradeType == UpgradeType.Shield
                ? 1
                : (_upgradeType == UpgradeType.Pointer ? 2 : 3));
            Increase();
            OnUpgrade?.Invoke(_upgradeType);
        }

        private void Increase()
        {
            switch (_upgradeType)
            {
                case UpgradeType.Shield:
                    _playerGameInfo.IncreaseHealthLevel();
                    break;
                case UpgradeType.Pointer:
                    _playerGameInfo.IncreaseDotsLevel();
                    break;
                case UpgradeType.Ball:
                    _playerGameInfo.IncreaseBallsLevel();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnBuyClick()
        {
            _playerGameInfo.TakeCoins(_cost);
            Increase();
            OnUpgrade?.Invoke(_upgradeType);
        }

        private void OnDestroy()
        {
            _buyBtn.onClick.RemoveListener(OnBuyClick);
            _adsBtn.onClick.RemoveListener(OnAdsClick);
        }
    }
}