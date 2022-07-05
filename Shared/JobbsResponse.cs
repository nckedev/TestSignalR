namespace Shared;

public class JobbsResponse
{
    public bool Success { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    

    public static JobbsResponse Ok(string title ="" , string message ="")
    {
        return new JobbsResponse()
        {
            Success = true,
            Title = title,
            Message = message,
        };
    }
}

public class JobbsResponse<T> : JobbsResponse
{
    public T? Data { get; set; }

    public static JobbsResponse<T> Error(string title = "", string errorMessage = "")
    {
        return new JobbsResponse<T>()
        {
            Success = false,
            Data = default(T),
            Title = title,
            Message = errorMessage,
        };
    }

    public static JobbsResponse<T> Ok(T data, string title = "", string message = "")
    {
        return new JobbsResponse<T>()
        {
            Success = true,
            Data = data,
            Title = title,
            Message = message,
        };
    }
}