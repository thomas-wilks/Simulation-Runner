namespace Simulate.FostersAndPartners.Shared.Providers
{
    public interface IMessagePublisher
    {
        void PublishMessage(string simulationId);
    }
}