using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Analytics;
using ByteBrewSDK;
using Config;
using DG.Tweening;
using Effects;
using Funcraft.Merge;
using GameLoader;
using Interhaptics.Platforms.Mobile;
using Items;
using Sound;
using UI;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Level
{
    public enum LevelState
    {
        Prepare,
        Ready,
        Aim,
        Fly,
        Bricks,
        Fail,
        Win
    }
    
    public class LevelController : MonoBehaviour, ISceneController
    {
        [Inject] private LevelsConfig _levelsConfig;
        [Inject] private GameConfig _gameConfig;
        [Inject] private EnemiesConfig _enemiesConfig;
        [Inject] private AnalyticsService _analytics;
        
        [Inject] private UserInputWrapper _userInputWrapper;
        [Inject] private PlayerInfo.PlayerGameInfo _playerGameInfo;
        [Inject] private Ball.Pool _ballPool;
        [Inject] private Bullet.Pool _bulletPool;
        [Inject] private Dot.Pool _dotPool;
        [Inject] private BrickPools _brickPools;
        [Inject] private UiService _uiService;
        [Inject] private CameraHandler.CameraHandler _camera;
        [Inject] private Field _field;
        [Inject] private BricksMover _bricksMover;
        [Inject] private AttackWarning _attackWarning;
        [Inject] private Effector _effector;
        [Inject] private Tutorial _tutorial;
        [Inject] private IAdsSystem _adsSystem;
        [Inject] private ISoundManager _soundManager;
        [Inject] private MobileHapticsVibration _vibration;
        
        private Transform _fieldTransform;
        private Cannon _cannon;
        
        public bool Initialized { get; private set; }
        public event Action OnInitialized;
        public event Action<LevelExitType> OnExit;

        private List<Ball> _balls;
        private List<BrickBase> _bricks;
        private List<ExtraBall> _takenExtraBalls;
        private List<int> _healedInThisTurn;
        private List<GenerateBrickData> _generateBrickDatas = new ();
        private LevelState _state;
        private Vector2 _aimPoint;
        private Vector3 _spawnPoint;
        private int _countOfDots;
        private int _countOfBalls;
        private int _checkedCountOfBalls;
        private int _health;
        private int _startHealth;
        private float _flyStartDelay;
        private float _betweenDots;
        private bool _firstBallBlood;
        private bool _canFly;
        private int _killedBalls;
        private bool _needKillBalls;
        private bool _allBallsStarted;
        private bool _canRevive = true;
        private bool _cameraIs2D;
        private bool _isTutorialHand;
        private bool _isTutorialLevel;
        private float _enemyTurnDelay;
        
        private int _currentWave;
        private int _playerTurnWaitCounter;
        private float _chanceOfEnemyStep;
        private Vector3 _startDirection;
        private LevelConfig _levelConfig;
        private DotsRender _dotsRender;
        private HUD _hud;
        private ReviveScreen _reviveScreen;
        private PauseScreen _pauseScreen;

        private void Start()
        {
            _adsSystem.ShowStartInterstitial(false);
            _camera.Init();

            _userInputWrapper.SetCurrentCamera(_camera.CurrentCamera, true);
            if (_tutorial.GetStady() == TutorialStady.FirstLevelPointer)
            {
                _userInputWrapper.ProcessedPointerDownInputReceived += UserInput_PointerDownInputReceivedTutorial;
            }
            else
            {
                _userInputWrapper.ProcessedPointerDownInputReceived += UserInput_PointerDownInputReceived;
            }
            _userInputWrapper.ProcessedPointerUpInputReceived += UserInput_PointerUpInputReceived;
            _userInputWrapper.ProcessedPointerMoveInputReceived += UserInput_InputMoveReceived;

            _balls = new List<Ball>();
            _bricks = new List<BrickBase>();

            _enemyTurnDelay = _gameConfig.GetDefaultFieldConfig.EnemyTurnDelay;

            var testMode = _gameConfig.GetTestModeConfig();
            if (testMode != null)
            {
                _levelConfig = _levelsConfig.GetLevel(testMode.StartLevel);
            }
            else
            {
                // if (_levelsConfig.CountOfLevels < _playerGameInfo.CurrentLevel)
                // {
                //     _playerGameInfo.IncreaseGameLevel(1);
                // }
            
                _levelConfig = _levelsConfig.GetLevel(_playerGameInfo.CurrentLevel);
                _levelConfig = _levelConfig.Clone();

                if (_playerGameInfo.CurrentLevel > 100)
                {
                    _levelConfig.LevelId = _playerGameInfo.CurrentLevel;
                    _levelConfig.StartObstacleHP = _levelConfig.LevelId;
                }
            }
            
            _hud = _uiService.Get<HUD>();
            _hud.OnKillBalls += HudOnOnKillBalls;
            _hud.OnPause += HudOnPause;
            _hud.OnChangeCameraProjection += HudOnOnChangeCameraProjection;
            _hud.OnSkip += HudOnSkip;
            _hud.ShowInfo(1 + "/" + _levelConfig.CountOfWaves, false);
            _hud.ShowLevel(_levelConfig.LevelId.ToString());
            

            _reviveScreen = _uiService.Get<ReviveScreen>();
            _reviveScreen.OnNoBtnClick += ReviveOnNoBtnClick;
            _reviveScreen.OnRessurectionBtnClick += ReviveOnRessurectionBtnClick;

            _pauseScreen = _uiService.Get<PauseScreen>();
            _pauseScreen.OnExit += PauseScreenOnOnExit;

            Initialized = true;
            OnInitialized?.Invoke();
        }

        private void PauseScreenOnOnExit()
        {
            LevelClosed();
        }
        
        private void HudOnOnKillBalls()
        {
            _needKillBalls = true;
        }
        
        private void HudOnPause()
        {
            _pauseScreen.Show();
        }
        
        private void HudOnOnChangeCameraProjection(bool to2D)
        {
            if (to2D)
            {
                _camera.PrepareCamera2D(_field.NumberOfCols + _gameConfig.GetDefaultFieldConfig.oversize2D);
                _playerGameInfo.ChangeScreenTo2D(true);
            }
            else
            {
                _camera.PrepareCamera3D(_field.NumberOfCols * .5f + _field.GetBaseSize() * _gameConfig.GetDefaultFieldConfig.oversize3D);
                _playerGameInfo.ChangeScreenTo2D(false);
            }
            _hud.SetBustersPosition(_field.GetBottomBorderScreenY());
        }
        
        private void HudOnSkip()
        {
            LevelWin();
        }

        private void ReviveOnNoBtnClick()
        {
            LevelFailed(false);
        }
        
        private async void ReviveOnRessurectionBtnClick()
        {
            ChangeHP((int)(_startHealth * .5f));
            
            await KillLastRow();
            
            SetState(LevelState.Prepare);
        }

        public async void Init()
        {
            _soundManager.PlayBackgroundMusic(BackgroundSoundId.MusicInGame);
            _soundManager.PlayAmbientMusic(AmbientSoundId.AmbientMusicInGame);
            
            await _field.Create(_levelConfig, _camera);
            
            HudOnOnChangeCameraProjection(_playerGameInfo.SettingsData.ScreenIn2D);
            
            _fieldTransform = _field.transform;
            _cannon = _field.GetCannon();
            
            var testMode = _gameConfig.GetTestModeConfig();
            _countOfBalls = testMode?.Balls ?? _playerGameInfo.GetBalls();
            _countOfDots = testMode?.Dots ?? _playerGameInfo.GetDots();
            _health = _startHealth =  _playerGameInfo.GetHealth( testMode?.Shield ?? 0);

            var difficultyDecrement = Math.Max(_levelConfig.StartChanсeOfEnemies, _levelConfig.ChanсeOfEnemies - _levelConfig.DifficultyDecrement * _playerGameInfo.GetLevelLoseCount);
            
            _chanceOfEnemyStep = (difficultyDecrement - _levelConfig.StartChanсeOfEnemies) / (_levelConfig.CountOfWaves - 1);
            
            
            _betweenDots = _gameConfig.GetDefaultFieldConfig.BetweenDots;
            _flyStartDelay = _gameConfig.GetDefaultFieldConfig.BallStartDelay;
            
            _bricksMover.Init(_field.NumberOfRows, _field.NumberOfCols);
            _dotsRender = new DotsRender(_dotPool, _fieldTransform, _gameConfig);
            _takenExtraBalls = new List<ExtraBall>();
            
            ChangeHP(0);
            
            if (_tutorial.GetStady() == TutorialStady.FirstLevelPointer)
            {
                _isTutorialHand = true;
            }

            if (!_tutorial.IsFinished())
            {
                _isTutorialLevel = true;
            }
            
            _hud.Show(_isTutorialHand, _playerGameInfo.SettingsData.ScreenIn2D, _field.GetBottomBorderScreenY());
            
            
            AddObstacles();
            
            SetState(LevelState.Prepare);

            if (_levelConfig.LevelId > 1 && !_playerGameInfo.SettingsData.AdsDisabled)
            {
                _adsSystem.ShowBanner();
            }
            _playerGameInfo.ClearUpgradeAdsDelay();
            
            _analytics.ByteBrewLevelEvent(_levelConfig.LevelId, $"started={(_playerGameInfo.GetLevelLoseCount == 0 ? 0 : 1)};");
            _analytics.ByteBrewLevelProgressionEvent(ByteBrewProgressionTypes.Started, _levelConfig.LevelId, _playerGameInfo.GetLevelLoseCount == 0 ? "first" : "restart");
        }

        private void AddObstacles()
        {
            foreach (var obstacle in _levelConfig.Obstacles)
            {
                _brickPools.CreateObstacle(new Vector2Int(obstacle.x - 1, obstacle.y), _currentWave);
            }
        }

        private void SetState(LevelState state)
        {
            _state = state;
            switch (state)
            {
                case LevelState.Prepare:
                    StartCycle();
                    break;
                case LevelState.Ready:
                    break;
                case LevelState.Aim:
                    StartAim();
                    break;
                case LevelState.Fly:
                    StartFly();
                    break;
                case LevelState.Bricks:
                    BricksTurn();
                    break;
                case LevelState.Fail:
                    LevelFailed(true);
                    break;
                case LevelState.Win:
                    LevelWin();
                    break;
            }
        }

        private async void StartCycle()
        {
            if (_health <= 0)
            {
                SetState(LevelState.Fail);
                return;
            }

            if (_currentWave >= _levelConfig.CountOfWaves && _bricks.Count == 0)
            {
                SetState(LevelState.Win);
                return;
            }
            
            CreateBricks();
            _firstBallBlood = false;
            _spawnPoint = _field.GetSpawnPointPositionRounded();
            TrySpawnCannon(true);

            var takeBallDuration = .5f;
            if (_takenExtraBalls.Count != 0)
            {
                foreach (var takenExtraBall in _takenExtraBalls)
                {
                    _countOfBalls++;
                    takenExtraBall.TweenMove(_cannon.GetPosition(), takeBallDuration, () =>
                    {
                        _brickPools.DestroyBrick(takenExtraBall);
                    });
                }
                _takenExtraBalls.Clear();
                await new WaitForSeconds(takeBallDuration);
            }

            _checkedCountOfBalls = _countOfBalls;
            
            SetState(LevelState.Ready);
        }
        
        private void StartAim()
        {
            _hud.ShowLevel(string.Empty);
            _dotsRender.Clear();
        }
        
        private async void StartFly()
        {
            _allBallsStarted = false;
            _dotsRender.Clear();
            _balls.Clear();

            for (var i = 0; i < _countOfBalls; i++)
            {
                if (_bricks.Count == 0)
                {
                    _checkedCountOfBalls = i;
                    break;
                }
                var ball = SpawnBall();
                ball.StartFly(_startDirection, .3f);
                
                await new WaitForSeconds(_flyStartDelay);
            }

            _hud.StartFly();
            _allBallsStarted = true;
            TrySpawnCannon();
        }

        private async void BricksTurn()
        {
            _attackWarning.Clear();
            _hud.EndFly();
            _healedInThisTurn = new List<int>();
            var moves = _bricksMover.Move(_bricks);

            foreach (var moveResult in moves)
            {
                await new WaitForSeconds(.02f);
                var brick = _bricks.FirstOrDefault(b => b.Id == moveResult.BrickId);
                if (brick == null)
                {
                    Debug.LogError("MOVE BRICK ERROR " + moveResult.BrickId);
                    continue;
                }

                switch (moveResult.BrickType)
                {
                    case BrickMoveResultType.Move:
                        brick.Move(moveResult.Move);
                        break;
                    case BrickMoveResultType.Ghost:
                        if (brick is EnemyGhost ghost) ghost.ChangeGhost(_bricksMover.BrickOnLastLine(brick.Id));
                        else if (brick is BossGhost ghostBoss) ghostBoss.ChangeGhost(_bricksMover.BrickOnLastLine(brick.Id));
                        break;
                    case BrickMoveResultType.Stone:
                        if (brick is EnemyStone stone) stone.ChangeStone(_bricksMover.BrickOnLastLine(brick.Id) || moveResult.OnLastLine);
                        else if (brick is BossStone stoneBoss) stoneBoss.ChangeStone(_bricksMover.BrickOnLastLine(brick.Id) || _bricksMover.BrickOnLastLine(brick.Id));
                        break;
                    case BrickMoveResultType.Attack:
                        var isBoss = BrickTypeHelper.IsBoss(brick.TypeOfBrick);
                        await Attack(brick, isBoss);
                        break;
                    case BrickMoveResultType.Clone:
                        await CreateClone(brick, moveResult.CloneResult);
                        break;
                    case BrickMoveResultType.Spider:
                        await Jump(brick, moveResult.Move);
                        break;
                    case BrickMoveResultType.Ram:
                        await Ram(brick, moveResult.Move);
                        break;
                    case BrickMoveResultType.Heal:
                        switch (brick)
                        {
                            case EnemyHealer healer:
                                await Heal(healer);
                                break;
                            case BossHealer bossHealer:
                                await Heal(bossHealer);
                                break;
                        }
                        break;
                    case BrickMoveResultType.BossJump:
                        await Jump(brick, moveResult.Move);
                        break;
                    case BrickMoveResultType.Shoot:
                        if (brick is BrickShooter shooter)
                        {
                            await Shooter(shooter);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            foreach (var brick in _bricks)
            {
                brick.EndTurn();
            }
            
            var attackList = _bricksMover.GetIdsOnLastLine();
            foreach (var (index, brickId) in attackList)
            {
                var brick = _bricks.FirstOrDefault(b => b.Id == brickId);
                if (brick != null && !BrickTypeHelper.IsJumpingBoss(brick.TypeOfBrick) && !BrickTypeHelper.IsBonus(brick.TypeOfBrick))
                {
                    _attackWarning.Add(index);
                }
            }
           
            SetState(LevelState.Prepare);
        }

        private async Task Attack(BrickBase brick, bool isBoss)
        {
            var attackValue = -brick.GetAttackValue();
            if (isBoss)
            {
                attackValue = (int) (attackValue * _levelConfig.TutorialBoss.TutorialBossDamage);
            }
            
            var startPosititon = brick.transform.position;
            var finishPosition = _field.GetCannon().GetPosition();
            var duration = .3f;

            brick.transform.DOMove(finishPosition, duration).SetEase(Ease.InExpo);
            
            await new WaitForSeconds(duration);
            
            ChangeHP(attackValue);
            
            if (isBoss)
            {
                _effector.AddCustomEffect(CustomEffectType.StoneHit, _fieldTransform, finishPosition, 1f, null, null);
                brick.transform.DOMove(startPosititon, duration).SetEase(Ease.InSine);
            }
            else
            {
                RemoveBrick(brick);
                _effector.AddCustomEffect(CustomEffectType.EnemyExplosive, _fieldTransform, brick.GetPosition(_field.GetBaseSize()), 3f, _enemiesConfig.GetConfig(brick.TypeOfBrick), null);
            }
        }

        private void CheckEndPlayerTurn()
        {
            if (_killedBalls == _checkedCountOfBalls && _playerTurnWaitCounter == 0)
            {
                _balls.Clear();
                _killedBalls = 0;
                SetState(LevelState.Bricks);
            }
        }

        private Ball SpawnBall()
        {
            var ball = _ballPool.Spawn();
            ball.Init(_spawnPoint);
            ball.KillMe += KillBall;
            ball.HitBrick += HitBrick;
            ball.HitEffect += HitEffect;
            _balls.Add(ball);
            return ball;
        }

        private void TrySpawnCannon(bool direct = false)
        {
            if ((_allBallsStarted && _firstBallBlood) || direct)
            {
                _cannon.SetPosition(_field.GetSpawnPointPositionRounded());
            }
        }

        private void KillBall(Ball ball)
        {
            if (!_firstBallBlood)
            {
                _firstBallBlood = true;
                _field.SetSpawnPointPosition(ball.transform.position.x);
                TrySpawnCannon();
            }

            if (ball.IsKilled())
            {
                return;
            }

            _killedBalls++;
            ball.KillMe -= KillBall;
            ball.HitBrick -= HitBrick;
            ball.HitEffect -= HitEffect;
            ball.SetKilled();
            _ballPool.Despawn(ball);

           CheckEndPlayerTurn();
        }
        
        private void HitBrick(GameObject hitObject, int value)
        {
            var brick = _bricks.FirstOrDefault(b => b.GetMainObject() == hitObject);
            if (brick != null)
            {
                var brickLive = brick.UpdateHealth(value, out var takenDamage);

                var isBoss = BrickTypeHelper.IsBoss(brick.TypeOfBrick);
                _playerGameInfo.AddXP(takenDamage);
                _soundManager.PlayMetaGameSound(isBoss ? MetaGameSoundId.BallKickBoss : MetaGameSoundId.BallKickUnit);
                PlayVibro(isBoss ? 1 : 0);
                
                if (!brickLive)
                {
                    BrickDeath(brick);
                }
            }
        }
        
        private void HitEffect(Vector3 position, int effectId)
        {
            _effector.AddCustomEffect(CustomEffectType.StoneHit, _fieldTransform, position, 1f, null, null, false);
        }

        private void BrickDeath(BrickBase brick)
        {
            switch (brick.TypeOfBrick)
            {
                case BrickType.BossMinion:
                {
                    var config = _enemiesConfig.GetConfig(brick.TypeOfBrick) as BossMinionConfig;
                    for (var i = 0; i < config.DeathGeneration; i++)
                    {
                        var minion = new GenerateBrickData(brick, BrickType.EnemyMinion);
                        minion.ParentBaseHealth = (int)(minion.ParentBaseHealth / config.StartHpCoefficient * config.MinionHPCoefficient);
                        _generateBrickDatas.Add(minion);
                    }
                    break;
                }
                case BrickType.EnemyMinion:
                {
                    for (var i = 0; i < 2; i++)
                    {
                        _generateBrickDatas.Add(new GenerateBrickData(brick, BrickType.EnemyMinionCyclop));
                    }
                    break;
                }
            }

            var isBoss = BrickTypeHelper.IsBoss(brick.TypeOfBrick);
            
            if (isBoss)
            {
                _bricks.Where(b => b.Id != brick.Id).ToList().ForEach(b => b.JumpTween());
            }

            if (isBoss || BrickTypeHelper.IsEnemy(brick.TypeOfBrick) || BrickTypeHelper.IsCommon(brick.TypeOfBrick))
            {
                _effector.AddCustomEffect(BrickTypeHelper.IsBoss(brick.TypeOfBrick) ? CustomEffectType.BossExplosive : CustomEffectType.EnemyExplosive, _fieldTransform, brick.GetPosition(_field.GetBaseSize()), 3f, _enemiesConfig.GetConfig(brick.TypeOfBrick), null);
                _soundManager.PlayMetaGameSound(isBoss ? MetaGameSoundId.BossDeath : MetaGameSoundId.UnitDeath);
            }
            RemoveBrick(brick);
        }

        private void RemoveBrick(BrickBase brick)
        {
            if (brick.TypeOfBrick == BrickType.ExtraBall)
            {
                _soundManager.PlayMetaGameSound(MetaGameSoundId.GetBonusBall);
                AddNewExtraBall(brick.transform.position);
            }
            if (brick is EnemyBomber bomber)
            {
                bomber.OnExplode -= BomberOnExplode;
                bomber.OnStartExplodeTimer -= BomberStartExplode;
            }
            else if (brick is BossBomber bomberBoss)
            {
                bomberBoss.OnExplode -= BomberOnExplode;
                bomberBoss.OnStartExplodeTimer -= BomberStartExplode;
            }
            
            brick.DisableCollider();
            _bricks.Remove(brick);
            var lastrowIds = _bricksMover.RemoveBrick(brick.Id);
            lastrowIds.ForEach(id => _attackWarning.Remove(id));
            _brickPools.DestroyBrick(brick);
        }

        private void AddNewExtraBall(Vector3 transformPosition)
        {
            var extraBall = _brickPools.CreateNewExtraBall(transformPosition,
                new Vector3(transformPosition.x, transformPosition.y, _spawnPoint.z));
            _takenExtraBalls.Add(extraBall);
        }

        private void CreateBricks()
        {
            if (!_bricksMover.SpawnRowIsFree(1))
            {
                return;
            }
            
            if (_currentWave == _levelConfig.MiddleBossWave - 1 || _currentWave == _levelConfig.CountOfWaves - 1)
            {
                if (!_bricksMover.SpawnRowIsFree(2))
                {
                    return;
                }
            }
            
            _currentWave++;

            if (_currentWave > _levelConfig.CountOfWaves)
            {
                return;
            }

            if (_levelConfig.MiddleBoss != BrickType.None)
            {
                if (_currentWave == _levelConfig.MiddleBossWave - 1)
                {
                    _hud.ShowInfo("", true);
                }
                else if (_currentWave == _levelConfig.MiddleBossWave)
                {
                    _hud.ShowInfo(_currentWave + "/" + _levelConfig.CountOfWaves, false);
                    CreateBoss((_levelConfig.MiddleBoss));
                    return;
                }
                else
                {
                    _hud.ShowInfo(_currentWave + "/" + _levelConfig.CountOfWaves, false);
                }
            }
            
            if (_currentWave == _levelConfig.CountOfWaves - 1)
            {
                _hud.ShowInfo("", true);
            }
            else if (_currentWave == _levelConfig.CountOfWaves)
            {
                _hud.ShowInfo(_currentWave + "/" + _levelConfig.CountOfWaves, false);
                CreateBoss(_levelConfig.Boss);
                return;
            }
            else
            {
                _hud.ShowInfo(_currentWave + "/" + _levelConfig.CountOfWaves, false);
            }
            
            var needCreateBrickTypes = new List<BrickType>();
            var countBricks = Random.Range(_levelConfig.MinCountOfObstacles, _field.NumberOfCols);
            
            if (Random.value > .5f)
            {
                needCreateBrickTypes.Add(BrickTypeHelper.GetRandomSpecialType());
                countBricks--;
            }

            var chanceOfEnemy =_levelConfig.StartChanсeOfEnemies + (_currentWave - 1) * _chanceOfEnemyStep;

            for (var i = 0; i < countBricks; i++)
            {
                if (Random.value > chanceOfEnemy)
                {
                    needCreateBrickTypes.Add(BrickType.Cube);
                }
                else if(_levelConfig.EnemyTypes.Length > 0)
                {
                    var brickType = _levelConfig.EnemyTypes[Random.Range(0, _levelConfig.EnemyTypes.Length)];
                    needCreateBrickTypes.Add(brickType);
                }
            }

            if (_levelConfig.BonusBalls != 0 && _currentWave % _levelConfig.BonusBalls == 0)
            {
                needCreateBrickTypes.Add(BrickType.ExtraBall);
            }
            
            foreach (var brickType in needCreateBrickTypes)
            {
                CreateBrick(brickType);
            }
        }

        private void CreateBoss(BrickType type)
        {
            var anotherBoss = _bricks.FirstOrDefault(b => BrickTypeHelper.IsBoss(b.TypeOfBrick));
            if (anotherBoss != null)
            {
                RemoveBrick(anotherBoss);
            }
            
            var brick = _brickPools.CreateBoss(type, _levelConfig, _currentWave);
            if (brick is BossBomber bomberBoss)
            {
                bomberBoss.OnExplode += BomberOnExplode;
                bomberBoss.OnStartExplodeTimer += BomberStartExplode;
            }
            _bricks.Add(brick);
        }

        private void CreateBrick(BrickType type)
        {
            var brick = _brickPools.CreateBrick(type, _levelConfig, _currentWave);
            if (brick != null)
            {
                if (brick is EnemyBomber bomber)
                {
                    bomber.OnExplode += BomberOnExplode;
                    bomber.OnStartExplodeTimer += BomberStartExplode;
                }
                _bricks.Add(brick);
            }
        }

        private void BomberStartExplode(BrickBase brick)
        {
            var config = _enemiesConfig.GetConfig(brick.TypeOfBrick);
            var duration = 3f;
            _effector.AddCustomEffect(CustomEffectType.BomberStart, brick.GetEffectPointTransform(), Vector3.zero, duration, _enemiesConfig.GetConfig(brick.TypeOfBrick), null);
            AddWaitTurnCounter();
        }


        private void CreateGeneratedBrick(GenerateBrickData generateBrickData)
        {
            void onComplete(BrickBase brick)
            {
                _bricks.Add(brick);
                SubtractWaitTurnCounter();
                CheckEndPlayerTurn();
            }
            var created = _brickPools.CreateGeneratedBrick(generateBrickData, onComplete);
            if (created)
            {
                AddWaitTurnCounter();
            }
        }

        private async Task CreateClone(BrickBase parentBrick, BrickCloneResult clone)
        {
            void onComplete(BrickBase brick)
            {
                if (brick is EnemyBomber bomber)
                {
                    bomber.OnExplode += BomberOnExplode;
                    bomber.OnStartExplodeTimer += BomberStartExplode;
                }
                _bricks.Add(brick);
            }

            var healthCoefficient = 1f;
            
            if (BrickTypeHelper.IsBoss(parentBrick.TypeOfBrick))
            {
                healthCoefficient /= _levelConfig.TutorialBoss.TutorialBossHP;
            }

            await _brickPools.CreateClone(parentBrick, clone, onComplete, healthCoefficient);
        }
        
        private async Task Jump(BrickBase brick, Vector2 position)
        {
            var brickConfig = _enemiesConfig.GetConfig(BrickType.EnemySpider) as BaseBrickConfig;
            
            var startPosititon = brick.transform.position;
            var finishPosition = new Vector3(_field.StartSpawnX + position.y * _field.GetBaseSize(), 0,
                _field.StartSpawnY - position.x * _field.GetBaseSize());
            var heightJump = brickConfig.HeightOfJump;
            var duration = _enemyTurnDelay;

            DOTween.To((value) =>
            {
                var currPosition = Vector3.Lerp(startPosititon, finishPosition, value);
                currPosition.y = Mathf.Sin(Mathf.PI * value) * heightJump;
                brick.transform.position = currPosition;

            }, 0, 1, duration).OnComplete(() =>
            {
                
            });
            await new WaitForSeconds(duration);
        }
        
        private async Task Ram(BrickBase brick, Vector2 position)
        {
            var startPosititon = brick.transform.position;
            var finishPosition = new Vector3(_field.StartSpawnX + position.y * _field.GetBaseSize(), 0,
                _field.StartSpawnY - position.x * _field.GetBaseSize());
            var duration = _enemyTurnDelay;

            DOTween.To((value) =>
            {
                var currPosition = Vector3.Lerp(startPosititon, finishPosition, value);
                brick.transform.position = currPosition;
            }, 0, 1, duration).OnComplete(() =>
            {
                
            });
            await new WaitForSeconds(duration);
        }

        private async Task Heal(EnemyHealer healer)
        {
            if (!healer.CanHeal())
            {
                return;
            }
            
            var brick = _bricks.Where(b => b.Id != healer.Id && !_healedInThisTurn.Contains(b.Id) && Math.Abs(b.GetHealthCoeff() - 1) != 0)
                                .OrderBy(b => b.GetHealthCoeff())
                                .FirstOrDefault();
            if (brick != null)
            {
                if (_enemiesConfig.GetConfig(BrickType.EnemyHealer) is HealerConfig config)
                {
                    var healValue = Mathf.CeilToInt(config.PercentOfHealing * brick.BaseHealth);
                    _healedInThisTurn.Add(brick.Id);

                    var startPoint = healer.GetPosition(_field.GetBaseSize())+ new Vector3(0, 1, 0);
                    var finishPoint = brick.GetPosition(_field.GetBaseSize()) + new Vector3(0, 1, 0);
                    var distance = Vector3.Distance(startPoint, finishPoint);
                    var duration = distance * _gameConfig.GetDefaultFieldConfig.HealFlyTime + _enemyTurnDelay;
            
                    _effector.AddFlyEffect(FlyEffectType.Heal, _fieldTransform, startPoint, finishPoint, duration, _enemiesConfig.GetConfig(healer.TypeOfBrick),
                        null,
                        null);  
                    _soundManager.PlayMetaGameSound(MetaGameSoundId.HealerHealing);
                    await new WaitForSeconds(duration);
                    brick.UpdateHealth(healValue, out var takenDamage);
                }
            }
        }
        
        private async Task Heal(BossHealer healer)
        {
            var config = _enemiesConfig.GetConfig(BrickType.BossHealer) as BossHealerConfig;

            for (int i = 0; i < config.CountOfHealing; i++)
            {
                var brick = _bricks.Where(b => b.Id != healer.Id && !_healedInThisTurn.Contains(b.Id) && Math.Abs(b.GetHealthCoeff() - 1) != 0)
                    .OrderBy(b => b.GetHealthCoeff())
                    .FirstOrDefault();
                if (brick != null)
                {
                    var healValue = Mathf.CeilToInt(config.PercentOfHealing * brick.BaseHealth);
                    brick.UpdateHealth(healValue, out var takenDamage);
                    _healedInThisTurn.Add(brick.Id);
                    await new WaitForSeconds(_enemyTurnDelay);
                }
            }
        }

        private async Task Shooter(BrickShooter shooter)
        {
            var attackValue = shooter.GetAttackValue();
            var startPoint = shooter.GetShootPoint();
            var finishPoint = _cannon.GetPosition();
            var distance = Vector3.Distance(startPoint, finishPoint);
            
            _effector.AddFlyEffect(FlyEffectType.Bullet, _fieldTransform, startPoint, finishPoint, distance * _gameConfig.GetDefaultFieldConfig.BulletFlyTime, _enemiesConfig.GetConfig(shooter.TypeOfBrick),
                () =>
                        {
                            if (_state == LevelState.Bricks)
                            {
                                ChangeHP(-attackValue);
                            }
                        }, 
                null);
            
            _soundManager.PlayMetaGameSound(MetaGameSoundId.BossShot);
            
            await new WaitForSeconds(_enemyTurnDelay);
        }
        
        private void BomberOnExplode(BrickBase brick)
        {
            var config = _enemiesConfig.GetConfig(brick.TypeOfBrick) as BomberConfig;
            var list = _bricksMover.GetNeighbors(brick.Id);
            var isBoss = BrickTypeHelper.IsBoss(brick.TypeOfBrick);
            if (list != null)
            {
                foreach (var bombedBrickId in list)
                {
                    var bombedBrick = _bricks.FirstOrDefault(b => b.Id == bombedBrickId);
                    if (bombedBrick != null)
                    {
                        if (!isBoss)
                        {
                            bombedBrick.JumpTween();
                        }
                        HitBrick(bombedBrick.GetMainObject(), (int)(-config.PercentOfDamage * bombedBrick.BaseHealth));
                    }
                }
            }

            if (isBoss)
            {
                _bricks.Where(b => b.Id != brick.Id).ToList().ForEach(b => b.JumpTween());
            }
            
            _effector.AddCustomEffect(CustomEffectType.BombExplosive, _fieldTransform, brick.GetPosition(_field.GetBaseSize()), 3f, _enemiesConfig.GetConfig(brick.TypeOfBrick), null, BrickTypeHelper.IsBoss(brick.TypeOfBrick));
            _soundManager.PlayMetaGameSound(isBoss ? MetaGameSoundId.BomberBossDeath : MetaGameSoundId.BomberUnitDeath);

            RemoveBrick(brick);
            SubtractWaitTurnCounter();
            CheckEndPlayerTurn();
        }

        private async Task KillLastRow()
        {
            var bricks =
                _bricks.Where(b => !BrickTypeHelper.IsBoss(b.TypeOfBrick) && _bricksMover.BrickOnLastLine(b.Id));

            var brickBases = bricks.ToList();
            while (brickBases.Any())
            {
                var brick = brickBases[0];
                brickBases.RemoveAt(0);
                var brickLive = brick.UpdateHealth(-10000000, out var takenDamage);

                BrickDeath(brick);

                await new WaitForSeconds(.3f);
            }
        }

        private void FixedUpdate()
        {
            if (_isTutorialHand)
            {
                _aimPoint = _hud.GetTutorialHandPosition();
            }
            
            if (_needKillBalls)
            {
                _needKillBalls = false;
                _balls.Where(ball => !ball.IsKilled())
                    .ToList()
                    .ForEach(KillBall);

                _killedBalls = _checkedCountOfBalls;
                CheckEndPlayerTurn();
                return;
            }

            GenerateNewRealtimeItems();
            
            if (_state == LevelState.Aim || _isTutorialHand)
            {
                DrawPoints();
            }
            else if (_state == LevelState.Fly)
            {
                if (_bricks.Count == 0 && _generateBrickDatas.Count == 0)
                {
                    _balls.Where(ball => ball.gameObject.activeSelf && !ball.IsFallen())
                        .ToList()
                        .ForEach(b => b.StartFallen());
                }
            }
        }

        private void GenerateNewRealtimeItems()
        {
            if (_generateBrickDatas.Count == 0)
            {
                return;
            }

            foreach (var generateBrickData in _generateBrickDatas)
            {
                switch (generateBrickData.Type)
                {
                    case BrickType.EnemyMinionCyclop:
                    case BrickType.EnemyMinion:
                        CreateGeneratedBrick(generateBrickData);
                        break;
                }
            }
            
            _generateBrickDatas.Clear();
        }

        private void DrawPoints()
        {
            _dotsRender.Clear();

            float rotation = 0;
                
            var first = _cannon.GetPosition();
            var firstV = _camera.CurrentCamera.WorldToViewportPoint(first);
            var firstVx = firstV.x;
            var aimV = _camera.CurrentCamera.ScreenToViewportPoint(_aimPoint);
            var aimVx = aimV.x;

            if (firstV.y + _gameConfig.GetDefaultFieldConfig.AimOffset > aimV.y)
            {
                var leftV = _camera.CurrentCamera.WorldToViewportPoint(_field.LeftBorderPosition).x;
                var rightV = _camera.CurrentCamera.WorldToViewportPoint(_field.RightBorderPosition).x;
            
                aimVx = 1 - (rightV - aimVx) / (rightV - leftV);

                rotation = firstVx - aimVx >= 0
                    ? Mathf.Lerp(-90, 0, aimVx / firstVx)
                    : Mathf.Lerp(0, 90, (aimVx - firstVx) / (1 - firstVx));
            }
            else
            {
                Vector3 second = _aimPoint;
                var firstScreen = _camera.CurrentCamera.WorldToScreenPoint(first);
                var tdirection = Vector3.Normalize(second - firstScreen);
                tdirection = new Vector3(tdirection.x, 0, tdirection.y);
                rotation = Mathf.Rad2Deg * Mathf.Atan2(tdirection.x, tdirection.z);
                if (rotation > 180)
                {
                    rotation -= 360;
                }
            }
            
            if ((rotation < 0 && rotation <= -90 + _gameConfig.GetDefaultFieldConfig.CannonBanAngle))
            {
                rotation = -90 + _gameConfig.GetDefaultFieldConfig.CannonBanAngle;
            }
            else if (rotation > 0 && rotation >=  90 - _gameConfig.GetDefaultFieldConfig.CannonBanAngle)
            {
                rotation = 90 - _gameConfig.GetDefaultFieldConfig.CannonBanAngle;
            }
            
            _cannon.SetRotation(rotation);
            
            var direction = _cannon.GetForward();
            _startDirection = direction;
           

            var points = new List<DotsInfo>();
            
                //var results = new RaycastHit[1];
            var layerMask = LayerMask.GetMask("Obstacle");
            
            var finalPosition = first;
            var lineStartPosition = finalPosition;
            var count = _countOfDots;
            
            while (count > 0)
            {
                var dist = .5425f; // .05f * 0.55f;
                
                var hits = Physics.Raycast(finalPosition, direction, out var hit, dist, layerMask);
                
                if (!hits)
                {
                    finalPosition += direction * dist;
                }
                else
                {
                    var lineDotsCount = 0;
                    
                    if (hit.collider.gameObject.CompareTag("Fall"))
                    {
                        lineDotsCount = CountOfDots(lineStartPosition, hit.point);
                        if (lineDotsCount <= count)
                        {
                            points.Add(new DotsInfo(){Count = lineDotsCount, StartPosition = lineStartPosition, FinishPosition = hit.point});
                        }
                        else
                        {
                            var correctedPosition = Vector3.Lerp(lineStartPosition, hit.point, (float)count / lineDotsCount);
                            points.Add(new DotsInfo(){Count = count, StartPosition = lineStartPosition, FinishPosition = correctedPosition});
                        }
                        break;
                    }
                    var newPosition = hit.point + direction * (-1 * .25f);
                    lineDotsCount = CountOfDots(lineStartPosition, newPosition);
                    
                    if (lineDotsCount <= count)
                    {
                        points.Add(new DotsInfo(){Count = lineDotsCount, StartPosition = lineStartPosition, FinishPosition = newPosition});
                        finalPosition = newPosition;
                        lineStartPosition = newPosition;
                    }
                    else
                    {
                        var correctedPosition = Vector3.Lerp(lineStartPosition, newPosition, (float)count / lineDotsCount);
                        points.Add(new DotsInfo(){Count = count, StartPosition = lineStartPosition, FinishPosition = correctedPosition});
                    }
                    
                    direction = Vector3.Reflect(direction, hit.normal);
                    direction.y = 0;
                    count -= lineDotsCount;
                }
            }

            _canFly = true;
            _dotsRender.Draw(points);
        }
        

        private int CountOfDots(Vector3 start, Vector3 finish)
        {
            var distance = Vector3.Distance(start, finish);
            var count = Mathf.Max(Mathf.RoundToInt(distance / _betweenDots), 1);
            return count;
        }

        private void AddWaitTurnCounter()
        {
            _playerTurnWaitCounter++;
        }
        
        private void SubtractWaitTurnCounter()
        {
            _playerTurnWaitCounter--;
        }

        private void UserInput_PointerDownInputReceived(Vector2 pos)
        {
            if (_state == LevelState.Ready)
            {
                _aimPoint = pos;
                SetState(LevelState.Aim);
            }
        }
        
        private void UserInput_PointerDownInputReceivedTutorial(Vector2 pos)
        {
            if (_tutorial.GetStady() == TutorialStady.FirstLevelPointer)
            {
                _isTutorialHand = false;
                _hud.ShowTutorialHand(false);
                _tutorial.SetStady(TutorialStady.FirstLevelPointerUp);
            }
            
            if (_state == LevelState.Ready)
            {
                _aimPoint = pos;
                SetState(LevelState.Aim);
            }
        }

        private void UserInput_PointerUpInputReceived(Vector2 pos)
        {
            if (_state == LevelState.Aim && _canFly)
            {
                SetState(LevelState.Fly);
            }
        }

        private void UserInput_InputMoveReceived(Vector2 pos, Vector2 delta)
        {
            if (_state == LevelState.Aim)
            {
                _aimPoint = pos;
            }
        }

        private void ChangeHP(int value)
        {
            if (value < 0)
            {
                _soundManager.PlayMetaGameSound(MetaGameSoundId.UnitKickCanon);
                PlayVibro(2);
            }
            _health += value;
            _health = Math.Max(_isTutorialLevel ? 1 : 0, _health);
            
            _field.UpdateHP(_health, _startHealth);
        }

        private async void LevelFailed(bool showEffect)
        {
            if (showEffect)
            {
                _soundManager.PlayMetaGameSound(MetaGameSoundId.CannonDeath);
                _effector.AddCustomEffect(CustomEffectType.PlayerDead, _fieldTransform, _field.GetCannon().GetPosition(), 2f, null, null);
                _cannon.Hide();
                await new WaitForSeconds(2f);
            }
            
            if (_canRevive)
            {
                _reviveScreen.Show();
                _canRevive = false;
            }
            else
            {
                var coinsConfig = _gameConfig.GetCoinsConfig;
                _playerGameInfo.AddCoins((int)((coinsConfig.StartReward + coinsConfig.RewardIncrement * (_levelConfig.LevelId - 1)) * coinsConfig.ConsolationPrize));

                _analytics.ByteBrewLevelEvent(_levelConfig.LevelId, "failed=1;");
                _analytics.ByteBrewLevelProgressionEvent(ByteBrewProgressionTypes.Failed, _levelConfig.LevelId, "no_skip_revive");

                _playerGameInfo.IncreaseLevelLoseCount();
                UiHide();
                OnExit?.Invoke(LevelExitType.Lose);
            }
        }

        private void LevelWin()
        {
            var coinsConfig = _gameConfig.GetCoinsConfig;
            if (_isTutorialLevel)
            {
                _tutorial.SetStady(TutorialStady.WinFirstLevel);
                _playerGameInfo.AddCoins(coinsConfig.StartReward + coinsConfig.RewardIncrement * (_levelConfig.LevelId - 1));
            }
           
            _analytics.ByteBrewLevelEvent(_levelConfig.LevelId, _canRevive ? "won=1;" : "won_revive=1;");
            _analytics.ByteBrewLevelProgressionEvent(ByteBrewProgressionTypes.Completed, _levelConfig.LevelId, _canRevive ? "won" : "won_revive");

            _playerGameInfo.IncreaseGameLevel(_levelConfig.LevelId + 1);
            UiHide();
            OnExit?.Invoke(_isTutorialLevel? LevelExitType.WinTutorial : LevelExitType.Win);
        }

        private void LevelClosed()
        {
            UiHide();
            OnExit?.Invoke(LevelExitType.Close);
        }

        private void UiHide()
        {
            _hud.Hide();
            _reviveScreen.Hide();
            _pauseScreen.Hide();
        }

        private void PlayVibro(int id)
        {
            if (_soundManager.Settings.VibroOn)
            {
                _vibration.PlayVibration(id);
            }
        }
        
        private void OnDestroy()
        {
            _userInputWrapper.ProcessedPointerDownInputReceived -= UserInput_PointerDownInputReceived;
            _userInputWrapper.ProcessedPointerDownInputReceived += UserInput_PointerDownInputReceivedTutorial;
            _userInputWrapper.ProcessedPointerUpInputReceived -= UserInput_PointerUpInputReceived;
            _userInputWrapper.ProcessedPointerMoveInputReceived -= UserInput_InputMoveReceived;
            _hud.OnKillBalls -= HudOnOnKillBalls;
            _hud.OnPause -= HudOnPause;
            _hud.OnSkip -= HudOnSkip;
            _hud.OnChangeCameraProjection -= HudOnOnChangeCameraProjection;
            _reviveScreen.OnNoBtnClick -= ReviveOnNoBtnClick;
            _reviveScreen.OnRessurectionBtnClick -= ReviveOnRessurectionBtnClick;
            _pauseScreen.OnExit -= PauseScreenOnOnExit;
        }
    }
    
    public class GenerateBrickData
    {
        public BrickType Type;
        public BrickType ParentType;
        public int ParentId;
        public Vector3 ParentPosition;
        public int ParentBaseHealth;
        
        public GenerateBrickData(BrickBase parentBrick, BrickType newBrickType)
        {
            if (parentBrick != null)
            {
                ParentType = parentBrick.TypeOfBrick;
                ParentId = parentBrick.Id;
                ParentPosition = parentBrick.transform.position;
                ParentBaseHealth = parentBrick.BaseHealth;
            }

            if (newBrickType != BrickType.None)
            {
                Type = newBrickType;
            }
        }
        
    }
}