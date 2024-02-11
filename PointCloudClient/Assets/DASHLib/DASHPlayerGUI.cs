using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace KOM.DASHLib
{
    public class DASHPlayerGUI : MonoBehaviour
    {
        public int ButtonSize = 64;
        public int ButtonMargin = 8;
        public UnityEvent PlayButtonPressedEvent = null;
        public UnityEvent PauseButtonPressedEvent = null;
        public UnityEvent StopButtonPressedEvent = null;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(10 + 0 * (ButtonMargin + ButtonSize), ButtonMargin, ButtonSize, ButtonSize), "Play"))
            {
                PlayButtonPressedEvent?.Invoke();
            }

            if (GUI.Button(new Rect(10 + 1 * (ButtonMargin + ButtonSize), ButtonMargin, ButtonSize, ButtonSize), "Pause"))
            {
                PauseButtonPressedEvent?.Invoke();
            }

            if (GUI.Button(new Rect(10 + 2 * (ButtonMargin + ButtonSize), ButtonMargin, ButtonSize, ButtonSize), "Stop"))
            {
                StopButtonPressedEvent?.Invoke();
            }
        }
    }
}