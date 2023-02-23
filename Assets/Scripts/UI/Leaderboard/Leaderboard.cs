using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Random = UnityEngine.Random;

namespace UI.Leaderboard
{
    public class Leaderboard : MonoBehaviour
    {
        [Inject] private LeaderboardLine.Pool _pool;
        
        [SerializeField] private Transform _contentTransform;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Sprite[] _flags;
        
        [Header("FAKES")]
        [SerializeField] private LeaderboardLine _fakeLine1;
        [SerializeField] private LeaderboardLine _fakeLine2;
        [SerializeField] private LeaderboardLine _fakeLine3;
        
        [SerializeField] private LeaderboardLine _winLine;

        private List<LeaderboardLine> _lines;
        private Sprite[] _flagsShuffled;
        
        private const int _maxScore = 5000000;
        private const int _step = 500;

        private void Awake()
        {
            _lines = new List<LeaderboardLine>();
            var rnd = new System.Random(42);
            _flagsShuffled = _flags.OrderBy(x => rnd.Next()).ToArray();
        }

        public void Init(int score, int oldscore, int lines, bool isTopLines = false)
        {
            Clear();

            var cutStep = _step - 1;
            var invScore = _maxScore - score;
            var placeId  = Mathf.Max(1, Mathf.FloorToInt((float)invScore / _step));
            var oldPlaceId  = Mathf.Max(1, Mathf.FloorToInt((float) (_maxScore - oldscore) / _step));
            
            if (isTopLines && placeId < 3)
            {
               PrepareWithTopLines(placeId, score, lines, cutStep);
            }
            else if(placeId == oldPlaceId)
            {
                PrepareWithoutTopLines(placeId, score, lines, cutStep);
            }
            else
            {
                PrepareWithScroll(placeId, oldPlaceId, score, lines, cutStep);
            }
        }

        private void PrepareWithTopLines(int placeId, int score, int lines, int cutStep)
        {
            _fakeLine1.RestoreToOld();
            _fakeLine2.RestoreToOld();
            _fakeLine3.RestoreToOld();

            LeaderboardLine fakeLine;
            if (score > _fakeLine1.GetFakeScore())
            {
                fakeLine = _fakeLine1;
                placeId = 1;
            }
            else if (score > _fakeLine2.GetFakeScore())
            {
                fakeLine = _fakeLine2;
                placeId = 2;
            }
            else
            {
                fakeLine = _fakeLine3;
                placeId = 3;
            }

            if (fakeLine != null)
            {
                fakeLine.Create(score, placeId, "Player", true, _flagsShuffled[placeId % _flagsShuffled.Length]);
            }
                
            for (var i = 4; i < lines + 4; i++)
            {
                CreateLine(i, score, false, cutStep);
            }
        }

        private void PrepareWithoutTopLines(int placeId, int score, int lines, int cutStep)
        {
            var maxPlace = _maxScore / _step;
            var bottomPlace = maxPlace - placeId;
            var placesBefore = placeId < lines ? placeId - 4 : Mathf.FloorToInt(lines * .5f);
            var dropCount = bottomPlace - (lines - placesBefore - 1);
            if (dropCount < 0)
            {
                placesBefore += -dropCount;
            }
            
            for (var i = 0; i < placesBefore; i++)
            {
                CreateLine(placeId - placesBefore + i, score, false, cutStep);
            }
                
            CreateLine(placeId, score, true, 0);
            
            for (var i = 0; i < lines - placesBefore - 1; i++)
            {
                CreateLine(placeId + 1 + i, score, false, i == 0 ? (score - (maxPlace - placeId + 1) * _step) :cutStep);
            }
        }

        private void PrepareWithScroll(int placeId, int oldPlaceId, int score, int lines, int cutStep)
        {
            var maxPlace = _maxScore / _step;
            
            ShowWinLine(placeId, score);
            
            var placesBefore = placeId == 1 ? 0 : 1;
            var placesAfter = maxPlace == placeId ? 0 : oldPlaceId - placeId;
            if (placesAfter == 0)
            {
                placesBefore = 2;
            }
            
            for (var i = 0; i < placesBefore; i++)
            {
                CreateLine(placeId - placesBefore + i, score, false, cutStep);
            }
            
            var playerLine = CreateLine(placeId, score, true, 0, true);
            
            for (var i = 0; i < placesAfter; i++)
            {
                CreateLine(placeId + 1 + i, score, false, i == 0 ? (score - (maxPlace - placeId + 1) * _step) :cutStep);
            }
            
            DOTween.To((value) =>
            {
                _scrollRect.verticalNormalizedPosition = value;
            }, 0, 1, 2f).OnComplete(() =>
            {
                playerLine.Show(true);
                HideWinline();
            });
        }

        private LeaderboardLine CreateLine(int placeId, int score, bool isPlayer, int limit, bool hideLine = false)
        {
            var line = _pool.Spawn();
            line.transform.parent = _contentTransform;
            var newScore = isPlayer 
                ? score 
                : (10000 - placeId) * _step + Random.Range(0, limit);

            if (newScore < 0)
            {
                newScore = 1;
            }
            
            
            line.Create(newScore, placeId, isPlayer? "Player" : GenerateName(), isPlayer, _flagsShuffled[placeId % _flagsShuffled.Length], hideLine);
            _lines.Add(line);

            return line;
        }

        private void Clear()
        {
            while (_lines.Count > 0)
            {
                var line = _lines[0];
                _pool.Despawn(line);
                _lines.RemoveAt(0);
            }

            HideWinline();
        }

        private void HideWinline()
        {
            if (_winLine != null)
            {
                _winLine.gameObject.SetActive(false);
            }
        }

        private void ShowWinLine(int placeId, int score)
        {
            if (_winLine == null)
            {
                return;
            }
            _winLine.Create(score, placeId, "Player", true, _flagsShuffled[placeId % _flagsShuffled.Length]);
            _winLine.gameObject.SetActive(true);
        }

        private string GenerateName()
        {
            var name1 = new[] { "gh", "br", "c", "cr", "dr", "g", "gr", "kr", "k", "kh", "n", "q", "qh", "sc", "scr", "str", "st", "t",
                "tr", "thr", "v", "vr", "x", "z", "", "", "", "", "" };
            var name2 = new[] { "a", "e", "ae", "ao", "ai", "au", "uo", "a", "e", "i", "o", "u", "i", "o", "u", "a", "e", "i", "o", "u",
                "a", "e", "i", "o", "u", "a", "e", "i", "o", "u", "a", "e", "i", "o", "u" };
            var name3 = new[] { "cr", "cz", "dr", "gr", "c", "k", "n", "q", "t", "v", "x", "z", "c", "cc", "gn", "gm", "gv", "gz", "k",
                "kk", "kn", "kr", "kt", "kv", "kz", "lg", "lk", "lq", "lx", "lz", "nc", "ndr", "nkr", "ngr", "nk", "nq", "nqr", "nz", "q", "qr",
                "qn", "rc", "rg", "rk", "rkr", "rq", "rqr", "sc", "sq", "str", "t", "v", "vr", "x", "z", "q", "k", "rr", "r", "t", "tt", "vv",
                "v", "x", "z", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "" };
            var name4 = new[] { "a", "e", "i", "", "a", "e", "i", "o", "u", "o", "u", "a", "e", "i", "o", "u", "oi", "ie", "ai", "ei", "eo", "ui" };
            var name5 = new[] { "ks", "l", "ls", "n", "d", "ds", "k", "ns", "ts", "x" };
            var r1 = Random.Range(0, name1.Length);
            var r2 = Random.Range(0, name2.Length);
            var r3 = Random.Range(0, name3.Length);
            var r4 = Random.Range(0, name4.Length);
            var r5 = Random.Range(0, name5.Length);
            while (name3[r3] == name1[r1] || name3[r3] == name5[r5]) r3 = Random.Range(0, name3.Length);
            if (name3[r3] == "") r4 = 0;
            else while (name4[r4] == "") r4 = Random.Range(0, name4.Length);
            return name1[r1] + name2[r2] + name3[r3] + name4[r4] + name5[r5];
        }
    }
}