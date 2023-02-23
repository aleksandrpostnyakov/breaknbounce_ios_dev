using System;
using System.Linq;
using Config;
using UnityEngine;
using Zenject;

namespace Items
{
    public class Ball : MonoBehaviour
    {
        [Inject] private GameConfig _gameConfig;
        
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _velocity;
        [SerializeField] private Vector3 _direction = new Vector3(0,0,1);

        public event Action<Ball> KillMe;
        public event Action<GameObject, int> HitBrick;
        public event Action<Vector3, int> HitEffect;

        private float _offset;
        private bool _killed;
        private bool _fallen;
        private DateTime _stepFlyTime;
        
        private void Start()
        {
            _offset = transform.localScale.x * 0.55f;
        }
        
        public void Init(Vector3 pos)
        {
            _velocity = 0;
            _killed = false;
            _fallen = false;
            transform.position = pos;
        }

        public float GetRadius()
        {
            return .25f;
        }

        public bool IsFallen()
        {
            return _fallen;
        }
        
        public bool IsKilled()
        {
            return _killed;
        }
        
        public void SetKilled()
        {
            _killed = true;
        }

        public void StartFly(Vector3 direction, float velocity)
        {
            _stepFlyTime = DateTime.Now;
            _direction = direction;
            _velocity = velocity;
        }

        public void StartFallen()
        {
            _fallen = true;
            _direction = new Vector3(0, 0, -1);
        }

        private void FixedUpdate()
        {
            if (_killed)
            {
                return;
            }

            if (Vector3.Distance(Vector3.zero, transform.position) > 20)
            {
                Debug.Log("BALL IS OUT " + _velocity + " " + _direction + " " + transform.position.x);
                KillBall();
                return;
            }
            
            if (_velocity != 0)
            {
                if ( _velocity < 1f && (DateTime.Now - _stepFlyTime).TotalSeconds > 2)
                {
                    _velocity += _gameConfig.GetDefaultFieldConfig.BallSpeedAddiction;
                    _stepFlyTime = DateTime.Now;
                }
                
                var layerMask = LayerMask.GetMask(new[] { "Obstacle", "Bonus"});
                var distance = _velocity + _offset;
                var fullDistance = distance;
                var inMove = true;
                var finalPosition = transform.position;

                while (inMove)
                {
                    var hits = Physics.Raycast(finalPosition, _direction, out var hit, distance, layerMask);
                    
                    if (!hits)
                    {
                        finalPosition += _direction * (_velocity * (distance / fullDistance));
                        inMove = false;
                    }
                    else
                    {
                        var bonus = hit.collider.gameObject.CompareTag("Bonus");
                        if (hit.collider.gameObject.CompareTag("BrickShield") || hit.collider.transform.parent.gameObject.CompareTag("BrickShield"))
                        {
                            HitEffect?.Invoke(hit.point, 1);
                        }
                        else if (hit.collider.gameObject.CompareTag("Fall"))
                        {
                            KillBall();
                            return;
                        }
                        else if (hit.collider.gameObject.CompareTag("Brick") || bonus)
                        {
                            HitBrick?.Invoke(hit.collider.gameObject, -1);
                        }
                        else if (hit.collider.transform.parent.gameObject.CompareTag("Brick"))
                        {
                            HitBrick?.Invoke(hit.collider.transform.parent.gameObject, -1);
                        }

                        if (!_fallen)
                        {
                            finalPosition = hit.point + _direction * (-1 * GetRadius());
                            if (!bonus)
                            {
                                _direction = Vector3.Reflect(_direction, hit.normal);
                                _direction.y = 0;
                            }
                        }
                        else
                        {
                            finalPosition += _direction * (_velocity * (distance / fullDistance));
                        }
                        
                        inMove = false;
                    }
                }

                transform.position = finalPosition;
            }
        }
        
        private void KillBall()
        {
            KillMe?.Invoke(this);
            _killed = true;
        }

        public class Pool : MonoMemoryPool<Ball> { }
    }
}