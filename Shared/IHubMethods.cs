namespace Shared;

public interface IHubMethods
{
    Task<JobbsResponse<Part>> AppRequestPart(string id);
    Task<JobbsResponse<List<Part>>> AppRequestListOfParts();
    Task<JobbsResponse<Part>> AppRequestInvalidPart(string id);
}