using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Items;
using TMPro;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Level
{
    public class BricksMover
    {
        [Inject] private Field _field;
        
        private FieldBrickInfo[,] _bricksIds;
        private List<BrickMoveResult> _moveResult;
        private int NumCols;
        private int NumRows;
        private int _currentBrickId;

        public void Init(int rows, int cols)
        {
            _bricksIds = new FieldBrickInfo[rows, cols];
            NumRows = rows;
            NumCols = cols;
        }

        public int GetCurrentBrickId(bool increase)
        {
            if (increase)
            {
                _currentBrickId++;
            }

            return _currentBrickId;
        }

        public void AddBrick(int row, int col, FieldBrickInfo brickInfo)
        {
            _bricksIds[row, col] = brickInfo;
        }
        
        public void AddBoss(int row, int col, FieldBrickInfo brickInfo)
        {
            _bricksIds[row, col] = brickInfo;
            _bricksIds[row, col + 1] = brickInfo;
            _bricksIds[row + 1, col] = brickInfo;
            _bricksIds[row + 1, col + 1] = brickInfo;
        }

        public List<BrickMoveResult> Move(List<BrickBase> bricks)
        {
            _moveResult = new List<BrickMoveResult>();
            CheckPassive(bricks);
            MoveFreeBricks();
            MoveLockedBricks();
            CheckActive(bricks);
            MoveLockedBricks(false);
            
            SetMarked(null, false, true);
            return _moveResult;
        }
        
#region +++ CheckPassive +++
        private void CheckPassive(List<BrickBase> bricks)
        {
            var result = bricks
                .OrderBy(b => b.GetPriority())
                .Select(brick => brick.PassiveAction(BrickOnLastLine(brick.Id)))
                .Where(actonResult => actonResult != null)
                .ToList();

            foreach (var brickPassiveMoveResult in result)
            {
                switch (brickPassiveMoveResult.type)
                {
                    case BrickPassiveMoveResultType.Ghost:
                        _moveResult.Add(new BrickMoveResult(){BrickType = BrickMoveResultType.Ghost, BrickId = brickPassiveMoveResult.BrickId});
                        break;
                    case BrickPassiveMoveResultType.Stone:
                        _moveResult.Add(new BrickMoveResult(){BrickType = BrickMoveResultType.Stone, BrickId = brickPassiveMoveResult.BrickId, OnLastLine = brickPassiveMoveResult.OnLastLine});
                        break;
                    case BrickPassiveMoveResultType.Clone:
                        CheckPassiveClone(brickPassiveMoveResult);
                         break;
                    case BrickPassiveMoveResultType.Heal:
                        _moveResult.Add(new BrickMoveResult() {BrickType = BrickMoveResultType.Heal, BrickId = brickPassiveMoveResult.BrickId});
                        break;
                    case BrickPassiveMoveResultType.GhostBoss:
                        _moveResult.Add(new BrickMoveResult(){BrickType = BrickMoveResultType.Ghost, BrickId = brickPassiveMoveResult.BrickId});
                        if (brickPassiveMoveResult.Count != 0)
                        {
                            CheckPassiveClone(brickPassiveMoveResult);
                        }
                        break;
                    case BrickPassiveMoveResultType.StoneBoss:
                        _moveResult.Add(new BrickMoveResult(){BrickType = BrickMoveResultType.Stone, BrickId = brickPassiveMoveResult.BrickId});
                        if (brickPassiveMoveResult.Count != 0)
                        {
                            CheckPassiveClone(brickPassiveMoveResult);
                        }
                        break;
                    case BrickPassiveMoveResultType.Shoot:
                        _moveResult.Add(new BrickMoveResult(){BrickType = BrickMoveResultType.Shoot, BrickId = brickPassiveMoveResult.BrickId});
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void CheckPassiveClone(BrickPassiveMoveResult brickPassiveMoveResult, bool notNeedWait = false)
        {
            for (var i = 0; i < brickPassiveMoveResult.Count; i++)
            {
                var position = GetNearestFreePlace(brickPassiveMoveResult.BrickId, brickPassiveMoveResult.FromBoss);
                if (position.Equals(Vector2.negativeInfinity))
                {
                    return;
                }
           
                var cloneResult = new BrickCloneResult
                {
                    CloneId = GetCurrentBrickId(true),
                    Position = position,
                    NotNeedWait = notNeedWait
                };
                cloneResult.SetType(brickPassiveMoveResult.ResultInt);
                
                AddBrick((int)position.x, (int)position.y, new FieldBrickInfo(){Id = cloneResult.CloneId, CanMove = brickPassiveMoveResult.CanMove, Marked = true});
                _moveResult.Add(new BrickMoveResult(){BrickType = BrickMoveResultType.Clone, BrickId = brickPassiveMoveResult.BrickId, CloneResult = cloneResult});
            }
        }
        
#endregion   --- CheckPassive ---
        
#region  +++ MoveFree +++
        private void MoveFreeBricks()
        {
            for (var row = NumRows - 2; row >= 0; row--)
            {
                for (var col = 0; col < NumCols; col++)
                {
                    var brick = _bricksIds[row, col];
                    if (brick != null && brick.Id > 0 && !brick.Marked && brick.CanMove)
                    {
                        var nextRow = row + 1;
                        if (nextRow == NumRows - 1)
                        {
                            if (brick.IsMinion)
                            {
                                var minionResult = new BrickPassiveMoveResult()
                                {
                                    type = BrickPassiveMoveResultType.Clone,
                                    BrickId = brick.Id,
                                    ResultInt = 2
                                };
                                
                                CheckPassiveClone(minionResult, true);
                                CheckPassiveClone(minionResult, true);
                            }
                            
                            AddAttackBrickResult(brick.Id);
                            SetMarked(brick.Id);
                            if (!brick.IsBoss)
                            {
                                RemoveBrick(brick.Id);
                            }
                        }
                        else if (!brick.IsBoss)
                        {
                            if (_bricksIds[nextRow, col] == null)
                            {
                                MoveCommonBrickOnFree(row, col, brick);
                            }
                        }
                        else
                        {
                            CheckAndMoveBossBrickOnFree(row, col, brick);
                        }
                    }
                }
            }
        }

        private void MoveCommonBrickOnFree(int row, int col, FieldBrickInfo brick)
        {
            var move = new Vector2Int(1, 0);
            FinalMoveBrick(row, col, brick, move);
        }
        
        private void CheckAndMoveBossBrickOnFree(int row, int col, FieldBrickInfo brick)
        {
            for (var i = 0; i < _bricksIds.GetLength(1); i++)
            {
                if (_bricksIds[row, i] != null && _bricksIds[row, i].Id == brick.Id)
                {
                    if (_bricksIds[row + 1, i] != null)
                    {
                        return;
                    }
                }
            }
            
            var move = new Vector2Int(1, 0);
            FinalMoveBoss(brick, move);
        }

        #endregion --- MoveFree ---
        
#region +++ MoveLocked +++
        private void MoveLockedBricks(bool checkLocked = true)
        {
            for (var row = NumRows - 2; row >= 0; row--)
            {
                for (var col = 0; col < NumCols; col++)
                {
                    var brick = _bricksIds[row, col];
                    if (brick != null && brick.Id > 0 && !brick.Marked && (!checkLocked || brick.CanMove))
                    {
                        if (!brick.IsBoss)
                        {
                            CheckAndMoveLockedBrickOnFree(row, col, brick);
                        }
                        else
                        {
                            CheckAndMoveLockedBossOnFree(row, col, brick);
                        }
                    }
                }
            }
        }

        private void CheckAndMoveLockedBrickOnFree(int row, int col, FieldBrickInfo brick)
        {
            if (col > 0 && _bricksIds[row, col - 1] == null)
            {
                if (col == NumCols - 1 || _bricksIds[row, col + 1] != null)
                {
                    var move = new Vector2Int(0, -1);
                    FinalMoveBrick(row, col, brick, move);
                }
                else
                {
                    MoveLockedBrickOnFreeAdvance(row, col, brick);
                }
            }
            else if (col < NumCols - 1 && _bricksIds[row, col + 1] == null)
            {
                if (col == 0 || _bricksIds[row, col - 1] != null)
                {
                    var move = new Vector2Int(0, 1);
                    FinalMoveBrick(row, col, brick, move);
                }
                else
                {
                    MoveLockedBrickOnFreeAdvance(row, col, brick);
                }
            }
        }

        private void CheckAndMoveLockedBossOnFree(int row, int col, FieldBrickInfo brick)
        {
            if (_bricksIds[row + 1, col] != null && _bricksIds[row + 1, col].Id == brick.Id)
            {
                return;
            }
            if (col > 0 && _bricksIds[row, col - 1] == null && _bricksIds[row - 1, col - 1] == null)
            {
                if (col == NumCols - 2 || _bricksIds[row, col + 2] != null || _bricksIds[row - 1, col + 2] != null)
                {
                    var move = new Vector2Int(0, -1);
                    FinalMoveBoss(brick, move);
                }
                else
                {
                    MoveLockedBrickOnFreeAdvance(row, col, brick);
                }
            }
            else if (col < NumCols - 2 && _bricksIds[row, col + 1] == null && _bricksIds[row - 1, col + 1] == null)
            {
                if (col == 0 || _bricksIds[row, col - 1] != null || _bricksIds[row - 1, col - 1] != null)
                {
                    var move = new Vector2Int(0, 1);
                    FinalMoveBoss(brick, move);
                }
                else
                {
                   MoveLockedBrickOnFreeAdvance(row, col, brick);
                }
            }
        }
#endregion --- MoveLocked ---

#region +++ CheckActive +++
        private void CheckActive(List<BrickBase> bricks)
        {
            var result = bricks
                .OrderBy(b => b.GetPriority())
                .Select(brick => brick.ActiveAction(BrickOnLastLine(brick.Id)))
                .Where(actonResult => actonResult != null)
                .OrderByDescending(ba => GetBrickPlace(ba.BrickId).x)
                .ToList();

            foreach (var brickActiveMoveResult in result)
            {
                switch (brickActiveMoveResult.type)
                {
                    case BrickActiveMoveResultType.Spider:
                        CheckActiveSpider(brickActiveMoveResult);
                        break;
                    case BrickActiveMoveResultType.Ram:
                        CheckActiveRam(brickActiveMoveResult);
                        break;
                    case BrickActiveMoveResultType.JumpBoss:
                        CheckActiveBossJump(brickActiveMoveResult);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void CheckActiveBossJump(BrickActiveMoveResult brickActiveMoveResult)
        {
             var brickPosition = GetBrickPlace(brickActiveMoveResult.BrickId);
            var brickPositionInt = new Vector2Int((int) brickPosition.x, (int) brickPosition.y);
            var freePlace = GetRandomFreePalceForBoss(brickActiveMoveResult.BrickId);
            if (freePlace.Equals(Vector2.negativeInfinity))
            {
                return;
            }
            
            _moveResult.Add(new BrickMoveResult(){BrickType = BrickMoveResultType.BossJump, BrickId = brickActiveMoveResult.BrickId, Move = freePlace});
            FinalMoveBoss(_bricksIds[brickPositionInt.x, brickPositionInt.y], new Vector2Int((int)freePlace.x - brickPositionInt.x, (int)freePlace.y - brickPositionInt.y), false);
        }

        private void CheckActiveSpider(BrickActiveMoveResult brickActiveMoveResult)
        {
            if (brickActiveMoveResult.Attack)
            {
                _moveResult.Add(new BrickMoveResult(){BrickType = BrickMoveResultType.Attack, BrickId = brickActiveMoveResult.BrickId});
                SetMarked(brickActiveMoveResult.BrickId);
                return;
            }
            
            Vector2 freePlace;
            if (brickActiveMoveResult.ResultBool)
            {
                freePlace = GetFreePlace(_bricksIds.GetLength(0) - 2);
                if (freePlace.Equals(Vector2.negativeInfinity) )
                {
                    return;
                }
                
                _moveResult.Add(new BrickMoveResult(){BrickType = BrickMoveResultType.Spider, BrickId = brickActiveMoveResult.BrickId, Move = freePlace});
            }
            else
            {
                freePlace = GetFreePlace();
                if (freePlace.Equals(Vector2.negativeInfinity) )
                {
                    return;
                }
                
                _moveResult.Add(new BrickMoveResult(){BrickType = BrickMoveResultType.Spider, BrickId = brickActiveMoveResult.BrickId, Move = freePlace});
            }
            
            MoveBrick(brickActiveMoveResult.BrickId, new Vector2Int((int)freePlace.x, (int)freePlace.y));
        }
        
        private void CheckActiveRam(BrickActiveMoveResult brickActiveMoveResult)
        {
            if (brickActiveMoveResult.Attack)
            {
                _moveResult.Add(new BrickMoveResult(){BrickType = BrickMoveResultType.Attack, BrickId = brickActiveMoveResult.BrickId});
                if (!brickActiveMoveResult.IsBoss)
                {
                    RemoveBrick(brickActiveMoveResult.BrickId);
                }
                else
                {
                    SetMarked(brickActiveMoveResult.BrickId);
                }
                
                return;
            }

            var brickPosition = GetBrickPlace(brickActiveMoveResult.BrickId);
            var brickPositionInt = new Vector2Int((int) brickPosition.x, (int) brickPosition.y);
            var freePlace = Vector2.negativeInfinity;

            if (brickActiveMoveResult.IsBoss)
            {
                if (_bricksIds[brickPositionInt.x + 2, brickPositionInt.y] != null || _bricksIds[brickPositionInt.x + 2, brickPositionInt.y + 1] != null)
                {
                    return;
                }
                
                for (var i = brickPositionInt.x + 3; i < _bricksIds.GetLength(0) - 1; i++)
                {
                    if (_bricksIds[i, brickPositionInt.y] != null ||_bricksIds[i, brickPositionInt.y + 1] != null)
                    {
                        freePlace = new Vector2(i - 2, brickPosition.y);
                        _moveResult.Add(new BrickMoveResult(){BrickType = BrickMoveResultType.Ram, BrickId = brickActiveMoveResult.BrickId, Move = freePlace});
                        break;
                    }
                }
                
                if (freePlace.Equals(Vector2.negativeInfinity))
                {
                    freePlace = new Vector2(_bricksIds.GetLength(0) - 3, brickPosition.y);
                    _moveResult.Add(new BrickMoveResult(){BrickType = BrickMoveResultType.Ram, BrickId = brickActiveMoveResult.BrickId, Move = freePlace});
                }
                FinalMoveBoss(_bricksIds[brickPositionInt.x, brickPositionInt.y], new Vector2Int((int)freePlace.x - brickPositionInt.x, (int)freePlace.y - brickPositionInt.y), false);
            }
            else
            {
                if (_bricksIds[brickPositionInt.x + 1, brickPositionInt.y] != null)
                {
                    return;
                }
                
                for (var i = brickPositionInt.x + 2; i < _bricksIds.GetLength(0) - 1; i++)
                {
                    if (_bricksIds[i, brickPositionInt.y] != null)
                    {
                        freePlace = new Vector2(i - 1, brickPosition.y);
                        _moveResult.Add(new BrickMoveResult(){BrickType = BrickMoveResultType.Ram, BrickId = brickActiveMoveResult.BrickId, Move = freePlace});
                        break;
                    }
                }
                
                if (freePlace.Equals(Vector2.negativeInfinity))
                {
                    freePlace = new Vector2(_bricksIds.GetLength(0) - 2, brickPosition.y);
                    _moveResult.Add(new BrickMoveResult(){BrickType = BrickMoveResultType.Ram, BrickId = brickActiveMoveResult.BrickId, Move = freePlace});
                }
                MoveBrick(brickActiveMoveResult.BrickId, new Vector2Int((int)freePlace.x, (int)freePlace.y));
            }
        }

#endregion

        private void FinalMoveBrick(int row, int col, FieldBrickInfo brick, Vector2Int move)
        {
            MoveBrick(new Vector2Int(row, col), move);
            SetMarked(brick, true);
            AddMoveBrickResult(brick.Id, move);
        }
        
        private void FinalMoveBoss(FieldBrickInfo brick, Vector2Int move, bool addMoveResult = true)
        {
            MoveBoss(GetBossPositions(brick.Id), move);
            SetMarked(brick, true);
            if (addMoveResult)
            {
                AddMoveBrickResult(brick.Id, move);
            }
        }
        
        private void MoveLockedBrickOnFreeAdvance(int row, int col, FieldBrickInfo brick)
        {
            var moveCol = col;
            var distance = NumCols;
            for (var i = 0; i < NumCols; i++)
            {
                if (i == col)
                {
                    continue;
                }
                if (_bricksIds[row + 1, i] == null)
                {
                    if (Math.Abs(col - i) < distance)
                    {
                        distance = Math.Abs(col - i);
                        moveCol = i;
                    }
                }
            }
            
            if (distance != NumCols)
            {
                var move =  new Vector2Int(0, moveCol < col ? -1 : 1);
                if (brick.IsBoss)
                {
                    FinalMoveBoss(brick, move);
                }
                else
                {
                    FinalMoveBrick(row, col, brick, move);
                }
            }
        }
        

        private void AddMoveBrickResult(int brickId, Vector2 move)
        {
            _moveResult.Add(new BrickMoveResult()
            {
                BrickId = brickId,
                Move = move
            });
        }

        private void AddAttackBrickResult(int brickId)
        {
            _moveResult.Add(new BrickMoveResult()
            {
                BrickType = BrickMoveResultType.Attack,
                BrickId = brickId,
                Move = new Vector2(1, 0)
            });
        }

        public List<int> RemoveBrick(int brickId)
        {
            var list = new List<int>();
            for (var x = 0; x < _bricksIds.GetLength(0); x += 1) {
                for (var y = 0; y < _bricksIds.GetLength(1); y += 1) {
                    if (_bricksIds[x, y] != null && _bricksIds[x, y].Id == brickId)
                    {
                        if (x == NumRows - 2)
                        {
                            list.Add(y);
                        }
                        _bricksIds[x, y] = null;
                    }
                }
            }

            return list;
        }

        public void MoveBrick(int brickId, Vector2Int newPlacement)
        {
            var brickInfo = new FieldBrickInfo();
            for (var x = 0; x < _bricksIds.GetLength(0); x += 1) {
                for (var y = 0; y < _bricksIds.GetLength(1); y += 1) {
                    if (_bricksIds[x, y] != null && _bricksIds[x, y].Id == brickId)
                    {
                        brickInfo.Clone(_bricksIds[x, y]);
                        brickInfo.Marked = true;
                        _bricksIds[x, y] = null;
                        _bricksIds[newPlacement.x, newPlacement.y] = brickInfo;
                        return;
                    }
                }
            }
        }
        
        private List<Vector2Int> GetBossPositions(int brickId)
        {
            var list = new List<Vector2Int>();
            
            for (var x = 0; x < _bricksIds.GetLength(0); x += 1) {
                for (var y = 0; y < _bricksIds.GetLength(1); y += 1) {
                    if (_bricksIds[x, y] != null && _bricksIds[x, y].Id == brickId)
                    {
                        list.Add(new Vector2Int(x, y));
                    }
                }
            }

            return list;
        }
        

        private void SetMarked(FieldBrickInfo brick, bool marked, bool all = false)
        {
            var brickId = 0;
            if (brick != null)
            {
                brick.Marked = marked;

                if (!brick.IsBoss)
                {
                    return;
                }

                brickId = brick.Id;
            }
            
            for (var x = 0; x < _bricksIds.GetLength(0); x += 1) {
                for (var y = 0; y < _bricksIds.GetLength(1); y += 1) {
                    if (_bricksIds[x, y] != null && (all || _bricksIds[x, y].Id == brickId))
                    {
                        _bricksIds[x, y].Marked = marked;
                    }
                }
            }
        }
        
        private void SetMarked(int brickId)
        {
            foreach (var brickInfo in _bricksIds)
            {
                if (brickInfo != null && brickInfo.Id == brickId)
                {
                    brickInfo.Marked = true;
                }
            }
        }

        private void MoveBrick(Vector2Int position, Vector2Int move)
        {
            var brick = _bricksIds[position.x, position.y];
            _bricksIds[position.x + move.x, position.y + move.y] = brick;
            _bricksIds[position.x, position.y] = null;
        }
        
        private void MoveBoss(List<Vector2Int> positions, Vector2Int move)
        {
            var brick = new FieldBrickInfo();
            brick.Clone(_bricksIds[positions[0].x, positions[0].y]);

            foreach (var t in positions)
            {
                _bricksIds[t.x, t.y] = null;
            }

            foreach (var position in positions)
            {
                try
                {
                    _bricksIds[position.x + move.x, position.y + move.y] = brick;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
        
        
        public int GetFreeGenerationLineBrickPlace()
        {
            var freePlaces = new List<int>();
            
            for (var i = 0; i < NumCols; i++)
            {
                if (_bricksIds[1, i] == null)
                {
                    freePlaces.Add(i);
                }
            }

            if (freePlaces.Count == 0)
            {
                return -1;
            }

            return freePlaces[Random.Range(0, freePlaces.Count)];
        }
        
        public Vector2 GetNearestFreePlace(int parentBrickId, bool isBoss = false)
        {
            var parentPlace = GetBrickPlace(parentBrickId);
            return GetNearestFreePlace(parentPlace, isBoss);
        }
        
        public Vector2 GetNearestFreePlace(Vector3 parentPosition, bool  isBoss = false)
        {
            var parentPlace = new Vector2(
                (_field.StartSpawnY - parentPosition.z)/_field.GetBaseSize(),
                    (parentPosition.x - _field.StartSpawnX)/_field.GetBaseSize()
                );
            return GetNearestFreePlace(parentPlace, isBoss);
        }
        
        public Vector2 GetNearestFreePlace(Vector2 parentPlace, bool  isBoss = false)
        {
            if (parentPlace.Equals(Vector2.negativeInfinity))
            {
                return parentPlace;
            }

            var result = Vector2.negativeInfinity;
            var distance = 100000f;
            var list = new List<Vector2>();
            var offset = isBoss ? .5f : 0;
            
            for (var x = 2; x < _bricksIds.GetLength(0) - 2; x += 1) {
                for (var y = 0; y < _bricksIds.GetLength(1); y += 1) {
                    if (_bricksIds[x, y] == null && !IsCellBelow(x, y, parentPlace, isBoss))
                    {
                        var newDistance = Mathf.Abs(x - (parentPlace.x + offset)) + Mathf.Abs(y - (parentPlace.y + offset));
                        
                        if (Math.Abs(newDistance - distance) < .1)
                        {
                            list.Add(new Vector2(x, y));
                        }
                        else if (newDistance < distance)
                        {
                            distance = newDistance;
                            list = new List<Vector2>(){new (x, y)};
                        }
                    }
                }
            }

            if (list.Count > 0)
            {
                result = list[Random.Range(0, list.Count)];
            }

            if (!isBoss && result.Equals(Vector2.negativeInfinity) && parentPlace.x < _bricksIds.GetLength(0) - 3 && _bricksIds[(int)parentPlace.x + 1, (int)parentPlace.y] == null)
            {
                result = parentPlace + new Vector2( 1, 0);
            }
            
            return result;
        }

        private bool IsCellBelow(int x, int y, Vector2 fromPlace, bool boss)
        {
            if (boss)
            {
                return (x == (int) fromPlace.x + 2 && y == (int) fromPlace.y) || (x == (int) fromPlace.x + 2 && y == (int) fromPlace.y + 1);
            }
            else
            {
                return x == (int) fromPlace.x + 1 && y == (int) fromPlace.y;
            }
        }
        
        public Vector2 GetFreePlace()
        {
            var result = new List<Vector2>();
            
            for (var x = 3; x < _bricksIds.GetLength(0) - 2; x += 1) {
                for (var y = 0; y < _bricksIds.GetLength(1); y += 1) {
                    if (_bricksIds[x, y] == null)
                    {
                        result.Add(new Vector2(x, y));
                    }
                }
            }

            return result.Count == 0 ? Vector2.negativeInfinity : result[Random.Range(0, result.Count)];
        }

        private Vector2 GetRandomFreePalceForBoss(int brickId)
        {
            var result = new List<Vector2>();
            for (var x = 3; x < _bricksIds.GetLength(0) - 2; x += 1) {
                for (var y = 0; y < _bricksIds.GetLength(1) - 1; y += 1) {
                    if (_bricksIds[x, y] == null && _bricksIds[x + 1, y] == null 
                        && _bricksIds[x, y + 1] == null && _bricksIds[x + 1, y + 1] == null)
                    {
                        result.Add(new Vector2(x, y));
                    }
                }
            }
            return result.Count == 0 ? Vector2.negativeInfinity : result[Random.Range(0, result.Count)];
        }
        
        public Vector2 GetFreePlace(int row)
        {
            var result = new List<Vector2>();
            
            for (var y = 0; y < _bricksIds.GetLength(1); y += 1) {
                if (_bricksIds[row, y] == null)
                {
                    result.Add(new Vector2(row, y));
                }
            }

            return result.Count == 0 ? Vector2.negativeInfinity : result[Random.Range(0, result.Count)];
        }
            

        public Vector2 GetBrickPlace(int brickId)
        {
            for (var x = 0; x < _bricksIds.GetLength(0); x += 1) {
                for (var y = 0; y < _bricksIds.GetLength(1); y += 1) {
                    if (_bricksIds[x, y] != null && _bricksIds[x, y].Id == brickId)
                    {
                        return new Vector2(x, y);
                    }
                }
            }
            
            return Vector2.negativeInfinity;
        }
        

        public bool SpawnRowIsFree(int row)
        {
            for (var i = 0; i < _bricksIds.GetLength(1); i++)
            {
                if (_bricksIds[row, i] != null && _bricksIds[row, i].Id > 0)
                {
                    return false;
                }
            }

            return true;
        }
        
        public bool LastRowHasFree()
        {
            var row = _bricksIds.GetLength(0) - 2;
            for (var i = 0; i < _bricksIds.GetLength(1); i++)
            {
                if (_bricksIds[row, i] == null)
                {
                    return true;
                }
            }

            return false;
        }

        public bool BrickOnLastLine(int brickId)
        {
            for (var i = 0; i < _bricksIds.GetLength(1); i++)
            {
                var checkRow = NumRows - 2;
                if (_bricksIds[checkRow, i] != null && _bricksIds[checkRow, i].Id == brickId)
                {
                    return true;
                }
            }

            return false;
        }

        public List<int> GetNeighbors(int brickId)
        {
            var place = GetBrickPlace(brickId);
            if (place.Equals(Vector2.negativeInfinity))
            {
                return null;
            }

            if (_bricksIds[(int)place.x, (int)place.y].IsBoss)
            {
                return GetBossNeighbors(brickId);
            }

            var list = new List<int>();
            CheckPlace((int)place.x - 1, (int)place.y, list);
            CheckPlace((int)place.x - 1, (int)place.y - 1, list);
            CheckPlace((int)place.x - 1, (int)place.y + 1, list);
            CheckPlace((int)place.x, (int)place.y - 1, list);
            CheckPlace((int)place.x, (int)place.y + 1, list);
            CheckPlace((int)place.x + 1, (int)place.y, list);
            CheckPlace((int)place.x + 1, (int)place.y - 1, list);
            CheckPlace((int)place.x + 1, (int)place.y + 1, list);
            
            return list;
        }
        
        private List<int> GetBossNeighbors(int brickId)
        {
            var place = GetBrickPlace(brickId);
            if (place.Equals(Vector2.negativeInfinity))
            {
                return null;
            }

            var list = new List<int>();
            CheckPlace((int)place.x - 1, (int)place.y - 1, list);
            CheckPlace((int)place.x - 1, (int)place.y, list);
            CheckPlace((int)place.x - 1, (int)place.y + 1, list);
            CheckPlace((int)place.x - 1, (int)place.y + 2, list);
            CheckPlace((int)place.x, (int)place.y - 1, list);
            CheckPlace((int)place.x, (int)place.y + 2, list);
            CheckPlace((int)place.x + 1, (int)place.y - 1, list);
            CheckPlace((int)place.x + 1, (int)place.y + 2, list);
            CheckPlace((int)place.x + 2, (int)place.y - 1, list);
            CheckPlace((int)place.x + 2, (int)place.y, list);
            CheckPlace((int)place.x + 2, (int)place.y + 1, list);
            CheckPlace((int)place.x + 2, (int)place.y + 2, list);
            
            return list;
        }

        private void CheckPlace(int x, int y, List<int> list)
        {
            var id = GetIdOnPlace(x, y);
            if (id > 0 && !list.Contains(id))
            {
                list.Add(id);
            }
        }

        private int GetIdOnPlace(int x, int y)
        {
            if (x < 0
                || x > NumRows - 1
                || y < 0
                || y > NumCols - 1
                || _bricksIds[x, y] == null
                || _bricksIds[x, y].Id <= 0)
            {
                return -1;
            }

            return _bricksIds[x, y].Id;
        }

        public List<Tuple<int, int>> GetIdsOnLastLine()
        {
            var list = new List<Tuple<int, int>>();
            for (var i = 0; i < _bricksIds.GetLength(1); i++)
            {
                var checkRow = NumRows - 2;
                if (_bricksIds[checkRow, i] != null)
                {
                    list.Add(new Tuple<int, int>(i, _bricksIds[checkRow, i].Id));
                }
            }

            return list;
        }
   }
    
    public class FieldBrickInfo
    {
        public int Id;
        public bool IsBoss;
        public bool Marked;
        public bool CanMove;
        public bool IsMinion;

        public void Clone(FieldBrickInfo copy)
        {
            Id = copy.Id;
            IsBoss = copy.IsBoss;
            Marked = copy.Marked;
            CanMove = copy.CanMove;
            IsMinion = copy.IsMinion;
        }
    }

    public class BrickMoveResult
    {
        public BrickMoveResultType BrickType = BrickMoveResultType.Move;
        public int BrickId;
        public Vector2 Move;
        public BrickCloneResult CloneResult;
        public bool OnLastLine;
    }

    public class BrickCloneResult
    {
        public BrickType Type;
        public int CloneId;
        public Vector2 Position;
        public bool NotNeedWait;

        public void SetType(int type)
        {
            Type = type switch
            {
                0 => BrickTypeHelper.GetRandomCommonType(),
                1 => BrickTypeHelper.GetRandomSpecialType(),
                2 => BrickType.EnemyMinionCyclop,
                3 => BrickType.EnemyMinion,
                4 => BrickType.EnemyRam,
                5 => BrickType.EnemyGhost,
                6 => BrickType.EnemyStoneBox,
                7 => BrickType.EnemyShieldBox,
                8 => BrickType.EnemySpider,
                9 => BrickType.EnemyHealer,
                10 => BrickType.EnemyBomber,
                _ => Type
            };
        }
    }

    public enum BrickMoveResultType
    {
        Move,
        Ghost,
        Stone,
        Attack,
        Clone, 
        Spider,
        Ram,
        Heal,
        Shoot,
        BossJump
    }
}