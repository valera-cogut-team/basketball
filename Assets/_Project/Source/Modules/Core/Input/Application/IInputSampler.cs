using Input.Domain;

namespace Input.Application
{
    /// <summary>
    /// Sampling port for polling input; implemented in Infrastructure (Unity).
    /// </summary>
    public interface IInputSampler
    {
        float SampleAxis(string axisName);

        bool SampleButton(string buttonName);

        bool SampleButtonDown(string buttonName);

        bool SampleButtonUp(string buttonName);

        int TouchCount { get; }

        void SampleTouch(int index, out float x, out float y, out InputTouchPhase phase);

        void SamplePointerPosition(out float x, out float y);
    }
}
