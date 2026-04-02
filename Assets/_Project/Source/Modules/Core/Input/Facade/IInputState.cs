using Input.Domain;

namespace Input.Facade
{
    /// <summary>
    /// Read-only polled input state (main thread).
    /// </summary>
    public interface IInputState
    {
        float GetAxis(string axisName);

        bool GetButton(string buttonName);

        bool GetButtonDown(string buttonName);

        bool GetButtonUp(string buttonName);

        int TouchCount { get; }

        void GetTouch(int index, out float x, out float y, out InputTouchPhase phase);

        void GetPointerPosition(out float x, out float y);
    }
}
