namespace Sample.Contracts
{
    public interface IOrderSubmissionAccepted
    {
        string CustomerNumber { get; }
        Guid OrderId { get; }
        DateTime Timestamp { get; }
    }
}