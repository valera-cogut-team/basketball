using System;
using Input.Domain;

namespace Input.Application
{
    public sealed class InputService
    {
        private readonly IInputSampler _sampler;

        public InputService(IInputSampler sampler)
        {
            _sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
        }

        public float GetAxis(string axisName) => _sampler.SampleAxis(axisName);

        public bool GetButton(string buttonName) => _sampler.SampleButton(buttonName);

        public bool GetButtonDown(string buttonName) => _sampler.SampleButtonDown(buttonName);

        public bool GetButtonUp(string buttonName) => _sampler.SampleButtonUp(buttonName);

        public int TouchCount => _sampler.TouchCount;

        public void GetTouch(int index, out float x, out float y, out InputTouchPhase phase)
            => _sampler.SampleTouch(index, out x, out y, out phase);

        public void GetPointerPosition(out float x, out float y)
            => _sampler.SamplePointerPosition(out x, out y);
    }
}
