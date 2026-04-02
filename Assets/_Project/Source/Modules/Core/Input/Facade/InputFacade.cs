using System;
using Input.Application;
using Input.Domain;

namespace Input.Facade
{
    /// <summary>
    /// Default facade implementation delegating to <see cref="Input.Application.InputService"/>.
    /// </summary>
    public sealed class InputFacade : IInputFacade
    {
        private readonly InputService _service;

        public InputFacade(InputService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public float GetAxis(string axisName) => _service.GetAxis(axisName);

        public bool GetButton(string buttonName) => _service.GetButton(buttonName);

        public bool GetButtonDown(string buttonName) => _service.GetButtonDown(buttonName);

        public bool GetButtonUp(string buttonName) => _service.GetButtonUp(buttonName);

        public int TouchCount => _service.TouchCount;

        public void GetTouch(int index, out float x, out float y, out InputTouchPhase phase)
            => _service.GetTouch(index, out x, out y, out phase);

        public void GetPointerPosition(out float x, out float y)
            => _service.GetPointerPosition(out x, out y);
    }
}
