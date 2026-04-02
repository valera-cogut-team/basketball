namespace Shaker
{
    public interface IShakerFacade : IShakerActions
    {
        float ImpulseScale { get; set; }
        float MaxVerticalKick { get; set; }
        float DecayPerSecond { get; set; }
    }
}
