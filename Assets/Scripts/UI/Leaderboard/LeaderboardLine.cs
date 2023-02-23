using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.Leaderboard
{
    public class LeaderboardLine : MonoBehaviour
    {
        [SerializeField] private TMP_Text _placeTxt;
        [SerializeField] private TMP_Text _nameTxt;
        [SerializeField] private TMP_Text _scoreTxt;
        [SerializeField] private Image _backImage;
        [SerializeField] private Image _flagImage;
        [SerializeField] private Sprite _backDefault;
        [SerializeField] private Sprite _backPlayer;
        [SerializeField] private Sprite _backBronze;
        [SerializeField] private Sprite _backSilver;
        [SerializeField] private Sprite _backGold;
        [SerializeField] private Canvas _canvas;

        private string _oldNameText;
        private string _oldScoreText;

        private void Awake()
        {
            _oldNameText = _nameTxt.text;
            _oldScoreText = _scoreTxt.text;
        }

        public void Create(int score, int placeId, string userName, bool isPlayer, Sprite flag, bool hideCanvas = false)
        {
            var backSprite = placeId switch
            {
                1 => _backGold,
                2 => _backSilver,
                3 => _backBronze,
                _ => isPlayer ? _backPlayer : _backDefault
            };

            _backImage.sprite = backSprite;

            _placeTxt.text = "#" + placeId;
            _scoreTxt.text = score.ToString();
            _nameTxt.text = userName;

            var cl = _flagImage.color;
            if (isPlayer)
            {
                _flagImage.color = new Color(cl.r, cl.g, cl.b, 0);
            }
            else
            {
                _flagImage.color = new Color(cl.r, cl.g, cl.b, 1);
                _flagImage.sprite = flag;
            }

            transform.localScale = Vector3.one;
            
            Show(!hideCanvas);
        }

        public void RestoreToOld()
        {
            _nameTxt.text = _oldNameText;
            _scoreTxt.text = _oldScoreText;
            var cl = _flagImage.color;
            _flagImage.color = new Color(cl.r, cl.g, cl.b, 1);
        }

        public int GetFakeScore()
        {
            return int.Parse(_oldScoreText);
        }

        public void Show(bool needShow)
        {
            _canvas.enabled = needShow;
        }
        
        public class Pool : MonoMemoryPool<LeaderboardLine> { }
    }
}