namespace rsit.Services;

public class ServiceResult
{
    public bool Succeeded { get; private set; }

    public List<string> Errors { get; private set; } = new();

    public static ServiceResult Success()
    {
        return new ServiceResult
        {
            Succeeded = true
        };
    }

    public static ServiceResult Failure(params string[] errors)
    {
        return new ServiceResult
        {
            Succeeded = false,
            Errors = errors.ToList()
        };
    }
}