using System;
using Config;
using PlayerInfo;
using UnityEngine;
using Zenject;

namespace UI
{
    public class UpgradesPanel : MonoBehaviour
    {
        [Inject] private PlayerGameInfo _playerGameInfo;
        [Inject] private GameConfig _gameConfig;
        
        [SerializeField] private UpgradeCard _shieldCard;
        [SerializeField] private UpgradeCard _pointerCard;
        [SerializeField] private UpgradeCard _ballCard;

        public Action<UpgradeType> OnUpgradeCard;

        private void Start()
        {
            _shieldCard.OnUpgrade += OnUpgrade;
            _pointerCard.OnUpgrade += OnUpgrade;
            _ballCard.OnUpgrade += OnUpgrade;
        }

        public void Init()
        {
            var shieldActive = _shieldCard.Init( false);
            var pointerActive = _pointerCard.Init( false);
            var ballActive = _ballCard.Init( false);

            if (!shieldActive && !pointerActive && !ballActive && _playerGameInfo.SettingsData.UpgradeAdsTime.AddSeconds(_gameConfig.AdsDelay) < DateTime.Now)
            {
                
                // временно оставил такой фолиант перегруженный лишним алгоритмом, но вдруг опять придется возвращаться к условиям, когда кнопка рекламы возможна и при доступных кнопках
                var adsIds = _playerGameInfo.SettingsData.UpgradeIds;
                var first = adsIds / 100;
                var freeId = first switch
                {
                    1 when !shieldActive => 1,
                    2 when !pointerActive => 2,
                    3 when !ballActive => 3,
                    _ => 0
                };

                if (freeId != 0)
                {
                    switch (freeId)
                    {
                        case 1:
                            _shieldCard.Init( true);
                            break;
                        case 2:
                            _pointerCard.Init( true);
                            break;
                        case 3:
                            _ballCard.Init( true);
                            break;
                    }
                }
                else
                {
                    var second = (adsIds - first * 100) / 10;
                    freeId = second switch
                    {
                        1 when !shieldActive => 1,
                        2 when !pointerActive => 2,
                        3 when !ballActive => 3,
                        _ => freeId
                    };

                    if (freeId != 0)
                    {
                        switch (freeId)
                        {
                            case 1:
                                _shieldCard.Init( true);
                                break;
                            case 2:
                                _pointerCard.Init( true);
                                break;
                            case 3:
                                _ballCard.Init( true);
                                break;
                        }
                    }
                    else
                    {
                        var third = (adsIds - second * 10);
                        freeId = third switch
                        {
                            1 when !shieldActive => 1,
                            2 when !pointerActive => 2,
                            3 when !ballActive => 3,
                            _ => freeId
                        };

                        if (freeId != 0)
                        {
                            switch (freeId)
                            {
                                case 1:
                                    _shieldCard.Init( true);
                                    break;
                                case 2:
                                    _pointerCard.Init( true);
                                    break;
                                case 3:
                                    _ballCard.Init( true);
                                    break;
                            }
                        }
                    }
                }
            }
        }
        

        public Vector3 GetCardBuyBtnPosition(int cardId)
        {
            switch (cardId)
            {
                case 1:
                    return _shieldCard.GetBuyBtnPosition();
                case 2:
                    return _pointerCard.GetBuyBtnPosition();
                case 3:
                    return _ballCard.GetBuyBtnPosition();
            }
            
            return Vector3.zero;
        }

        private void OnUpgrade(UpgradeType upgradeType)
        {
            OnUpgradeCard?.Invoke(upgradeType);
            Init();
        }

        private void OnDestroy()
        {
            _shieldCard.OnUpgrade -= OnUpgrade;
            _pointerCard.OnUpgrade -= OnUpgrade;
            _ballCard.OnUpgrade -= OnUpgrade;
        }
    }

    public enum UpgradeType
    {
        Shield,
        Pointer,
        Ball
    }
}