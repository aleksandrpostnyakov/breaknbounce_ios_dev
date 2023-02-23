using System;
using UnityEngine;

namespace Items
{
    public class Cannon : MonoBehaviour
    {
        [SerializeField] private Transform _cannonTransform;

        private void Awake()
        {
            Hide();
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            _cannonTransform.localEulerAngles = Vector3.zero;
            Show();
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public void SetRotation(float rotation)
        {
            _cannonTransform.localEulerAngles = new Vector3(0, rotation, 0);
        }

        public Vector3 GetForward()
        {
            return _cannonTransform.forward;
        }

        private void Show()
        {
            gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}