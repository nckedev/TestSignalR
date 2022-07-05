namespace Shared;

public interface IAgentMethods
{
    Task RequestPart(Guid requestId, string partId);
    Task RequestListOfParts(Guid requestId);
    Task RequestInvalidPart(Guid requestId, string partId);
}