using System;
using UnityEngine;
using Zenject;

namespace Funcraft.Merge
{
    public class GameEntryPoint : MonoBehaviour
    {
        [Inject] private StateMashine.StateMashine _stateMashine;
        
        private void Awake()
        {
            _stateMashine.Initalize();
        }
        
        // JUST FOR INITIALIZE CLAENDARS
        public void AddCallendars()
        {
            var gregorianCalendar = new System.Globalization.GregorianCalendar();
            var persianCalendar = new System.Globalization.PersianCalendar();
            var umAlQuraCalendar = new System.Globalization.UmAlQuraCalendar();
            var thaiBuddhistCalendar = new System.Globalization.ThaiBuddhistCalendar();
        }
    }
}