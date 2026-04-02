using Input.Application;
using Input.Domain;
using UnityEngine;

namespace Input.Infrastructure
{
    public sealed class LegacyUnityInputSampler : IInputSampler
    {
        public float SampleAxis(string axisName) => UnityEngine.Input.GetAxis(axisName);

        public bool SampleButton(string buttonName) => UnityEngine.Input.GetButton(buttonName);

        public bool SampleButtonDown(string buttonName) => UnityEngine.Input.GetButtonDown(buttonName);

        public bool SampleButtonUp(string buttonName) => UnityEngine.Input.GetButtonUp(buttonName);

        public int TouchCount => UnityEngine.Input.touchCount;

        public void SampleTouch(int index, out float x, out float y, out InputTouchPhase phase)
        {
            var t = UnityEngine.Input.GetTouch(index);
            x = t.position.x;
            y = t.position.y;
            phase = (InputTouchPhase)(int)t.phase;
        }

        public void SamplePointerPosition(out float x, out float y)
        {
            if (UnityEngine.Input.touchCount > 0)
            {
                var t = UnityEngine.Input.GetTouch(0);
                x = t.position.x;
                y = t.position.y;
            }
            else
            {
                var p = UnityEngine.Input.mousePosition;
                x = p.x;
                y = p.y;
            }
        }
    }
}
