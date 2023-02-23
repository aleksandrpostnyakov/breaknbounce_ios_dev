using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Config;
using DG.Tweening;
using Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Random = UnityEngine.Random;

namespace Level
{
    public class Field : MonoBehaviour
    {
        [Inject] private FieldCell.Pool _fieldCellPool;
        [Inject] private GameConfig _gameConfig;
        
        [SerializeField] private Transform _ground;
        [SerializeField] private Transform _rightBorder;
        [SerializeField] private Transform _leftBorder;
        [SerializeField] private Transform _topBorder;
        [SerializeField] private Transform _bottomBorder;
        [SerializeField] private Transform _spawnStartPoint;
        [SerializeField] private Transform _hpTransform;
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _hpValueTxt;
        [SerializeField] private Cannon _cannon;
        [SerializeField] private Transform _warningsTransform;
        [SerializeField] private Color _fieldColor1;
        [SerializeField] private Color _fieldColor2;

        private CameraHandler.CameraHandler _cameraHandler;

        private float _sizeOfBase = 1f;
        private LevelConfig _config;
        public float StartSpawnX { get; private set; }
        public float StartSpawnY { get; private set; }
        public int NumberOfRows { get; private set; }
        public int NumberOfCols { get; private set; }
        
        public Vector3 RightBorderPosition { get; private set; }
        public Vector3 LeftBorderPosition { get; private set; }
        public Vector3 TopBorderPosition { get; private set; }

        public Transform GetWarningsTransform => _warningsTransform;
        

        public event Action Created;

        public float GetBaseSize()
        {
            return _sizeOfBase;
        }

        public Cannon GetCannon()
        {
            return _cannon;
        }
        
        public Vector2 GetStartSpawnVector2()
        {
            return new Vector2();
        }

        public async Task Create(LevelConfig config, CameraHandler.CameraHandler cameraHandler)
        {
            _config = config;
            _cameraHandler = cameraHandler;
            
            _hpTransform.gameObject.SetActive(false);

            NumberOfRows = _config.NumberOfRows != 0
                ? _config.NumberOfRows
                : _gameConfig.GetDefaultFieldConfig.DefaultFieldRows;
                
            NumberOfCols = _config.NumberOfCols != 0
                ? _config.NumberOfCols
                : _gameConfig.GetDefaultFieldConfig.DefaultFieldCols;
            
            StartSpawnX = -(NumberOfCols - 1) * .5f * _sizeOfBase;
            StartSpawnY = (NumberOfRows - 1) * .5f * _sizeOfBase;
            
            _cameraHandler.PrepareCamera3D(NumberOfCols * .5f + _sizeOfBase * _gameConfig.GetDefaultFieldConfig.oversize3D);

            //_ground.localScale = new Vector3(NumberOfCols, NumberOfRows, 1);
            
            PrepareRightBorder();
            PrepareLeftBorder();
            PrepareTopBorder();
            PrepareBottomBorder();
            PrepareBorderXs();

            await CreateGround();
            
            PrepareHpTransform();
            PrepareSpawnPoint();

            PrepareWarningsTransform();
        }

        private void PrepareWarningsTransform()
        {
            _warningsTransform.position = new Vector3(StartSpawnX, _warningsTransform.position.y,
                StartSpawnY - (NumberOfRows - 1) * _sizeOfBase);
        }

        private void PrepareSpawnPoint()
        {
            var position = _bottomBorder.transform.position;
            _spawnStartPoint.transform.position = new Vector3(0,
                -_sizeOfBase * .25f, position.z + _sizeOfBase * .75f);
        }

        private void PrepareBottomBorder()
        {
            _bottomBorder.localScale = new Vector3(NumberOfCols + _sizeOfBase, 0, _sizeOfBase * .5f);
            _bottomBorder.transform.position = new Vector3(0, -_sizeOfBase * .25f, -(NumberOfRows + _sizeOfBase) * .5f + .25f * _sizeOfBase);
            _bottomBorder.gameObject.SetActive(false);
        }

        private void PrepareTopBorder()
        {
            _topBorder.localScale = new Vector3(NumberOfCols + _sizeOfBase, 0, _sizeOfBase * .5f);
            _topBorder.transform.position = new Vector3(0, -_sizeOfBase * .25f, (NumberOfRows + _sizeOfBase) * .5f - .25f * _sizeOfBase);
            _topBorder.gameObject.SetActive(false);
            TopBorderPosition = _topBorder.transform.position;
        }

        private void PrepareLeftBorder()
        {
            _leftBorder.localScale = new Vector3(_sizeOfBase * .5f, 0, NumberOfRows);
            _leftBorder.transform.position = new Vector3(-(NumberOfCols + _sizeOfBase) * .5f + .25f * _sizeOfBase, -_sizeOfBase * .25f, 0);
            _leftBorder.gameObject.SetActive(false);
            
        }

        private void PrepareRightBorder()
        {
            _rightBorder.localScale = new Vector3(_sizeOfBase * .5f, 0, NumberOfRows);
            _rightBorder.transform.position = new Vector3((NumberOfCols + _sizeOfBase) * .5f - .25f * _sizeOfBase, -_sizeOfBase * .25f, 0);
            _rightBorder.gameObject.SetActive(false);
        }
        
        private void PrepareBorderXs()
        {
            LeftBorderPosition = _leftBorder.transform.position;
            RightBorderPosition = _rightBorder.transform.position;
        }

        private void PrepareHpTransform()
        {
            _hpTransform.position = new Vector3(0, .01f, _bottomBorder.transform.position.z + _sizeOfBase * 0.75f);
            _hpTransform.gameObject.SetActive(true);
        }

        private async Task CreateGround()
        {
            var cells = new List<CellPositionOdd>();

            for (var i = 0; i < NumberOfRows; i++)
            {
                for (var j = 0; j < NumberOfCols; j++)
                {
                    cells.Add(new CellPositionOdd(){ Position = new Vector3(StartSpawnX + _sizeOfBase * j, -_sizeOfBase * .5f, StartSpawnY - _sizeOfBase * i), Odd = (i + j) % 2 == 0});
                }
            }

            cells = Shuffle(cells);
            var cellDiff = _gameConfig.GetDefaultFieldConfig.FieldGenerationSpeed / cells.Count;
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                var item = _fieldCellPool.Spawn();
                item.Create(transform, cell.Position, cell.Odd ? _fieldColor1 : _fieldColor2, i * cellDiff);

            }

            await new WaitForSeconds(_gameConfig.GetDefaultFieldConfig.FieldGenerationSpeed);
            
            _rightBorder.gameObject.SetActive(true);
            _leftBorder.gameObject.SetActive(true);
            _topBorder.gameObject.SetActive(true);
            _bottomBorder.gameObject.SetActive(true);
            
            DOTween.To((value) =>
            {
                ChangeScaleY(_leftBorder, value);
                ChangeScaleY(_rightBorder, value);
                ChangeScaleY(_topBorder, value);
                ChangeScaleY(_bottomBorder, value);

            }, 0, _sizeOfBase * .5f, 1.5f).OnComplete(() => { Created?.Invoke(); });
        }

        private void ChangeScaleY(Transform transformItem, float value)
        {
            var tr = transformItem.localScale;
            transformItem.localScale = new Vector3(tr.x, value, tr.z);
        }
        
        private List<T> Shuffle<T>( List<T> list)
        {
            var rnd = new System.Random(); 
            return list.OrderBy(_ => rnd.Next()).ToList();
        }

        public float GetBottomBorderScreenY()
        {
            return _cameraHandler.CurrentCamera.WorldToScreenPoint(_bottomBorder.transform.position).y;
        }
        
        public Vector3 GetSpawnPointPositionRounded()
        {
            var pos = _spawnStartPoint.transform.position;
            var offset = (NumberOfCols % 2 == 0 ? .5f : 0 ) * (pos.x >= 0 ? - 1 : 1);
            var rounded = (pos.x >= 0 ? (int)Mathf.Floor(pos.x) : (int)Mathf.Ceil(pos.x)) - offset;
            
            rounded = Mathf.Clamp(rounded, -(NumberOfCols*.5f), NumberOfCols*.5f);
            return new Vector3(rounded, pos.y, pos.z);
        }

        public void SetSpawnPointPosition(float x)
        {
            var spawnTransform = _spawnStartPoint.transform;
            var position = spawnTransform.position;
            var cutX = Math.Clamp(x, -NumberOfCols * .5f, NumberOfCols * .5f);
            position = new Vector3(cutX, position.y, position.z);
            spawnTransform.position = position;
        }
        
        public void UpdateHP(float currentValue, float maxValue)
        {
            _hpValueTxt.text = currentValue + "/" + maxValue;
            _slider.value = currentValue / maxValue;
        }
        
        
        private class CellPositionOdd
        {
            public Vector3 Position;
            public bool Odd;
        }
    }
}