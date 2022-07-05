namespace Shared;

public interface IAppClientMethods
{
    Task<JobbsResponse<Part>> AppRequestPart(string id);
    Task<JobbsResponse<List<Part>>> AppRequestListOfParts();
    Task<JobbsResponse<Part>> AppRequestInvalidPart(string id);
}