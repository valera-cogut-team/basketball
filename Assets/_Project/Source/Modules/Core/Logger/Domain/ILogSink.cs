namespace Logger.Domain
{
    public interface ILogSink
    {
        void Write(LogEntryData entry);
        void Flush();
    }
}
