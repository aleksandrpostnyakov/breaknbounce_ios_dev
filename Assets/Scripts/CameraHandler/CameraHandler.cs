using UnityEngine;

namespace CameraHandler
{
    public class CameraHandler
    {
        public  Camera CurrentCamera { get; private set; }

        public CameraHandler()
        {
            CurrentCamera = Camera.main;
        }
        
        public void Init()
        {
            CurrentCamera = Camera.main;
        }

        public float GetAspect()
        {
            return CurrentCamera.aspect;
        }
        
        public void PrepareCamera3D(float size)
        {
            CurrentCamera.orthographic = false;
            var cameraPosition = CurrentCamera.gameObject.transform.position;

            var cameraPositionY = cameraPosition.y;// - size * .5f;
            var offset = new Vector3(0, 0, -(1 + size - 3.69f));
            
            CurrentCamera.fieldOfView = Camera.VerticalToHorizontalFieldOfView(
                Mathf.Atan2(size, cameraPositionY) * Mathf.Rad2Deg * 2,
                1 / CurrentCamera.aspect);

            var coef = 9f / 16 / CurrentCamera.aspect;
            //Debug.Log("CAMRS " + size + " " +  coef + " " + CurrentCamera.fieldOfView);
            if (coef < 1)
            {
                CurrentCamera.fieldOfView /= coef;
            }
            
            //Debug.Log("AA " + CurrentCamera.fieldOfView);
            
            CurrentCamera.transform.position = new Vector3(0, cameraPositionY,
                -cameraPosition.y / Mathf.Tan(CurrentCamera.transform.rotation.eulerAngles.x * Mathf.Deg2Rad)) + offset;
        }

        public void PrepareCamera2D(float size)
        {
            CurrentCamera.orthographic = true;
            CurrentCamera.orthographicSize = size * (9f/16/CurrentCamera.aspect);
        }
    }
}