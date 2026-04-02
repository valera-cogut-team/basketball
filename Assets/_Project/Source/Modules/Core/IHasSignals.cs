namespace Core
{
    /// <summary>Optional interface for a Facade that exposes signals: <c>IMyFacade : IHasSignals&lt;IMySignals&gt;</c>.</summary>
    public interface IHasSignals<out TSignals> where TSignals : class
    {
        TSignals Signals { get; }
    }
}
