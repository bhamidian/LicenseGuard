namespace LicenseGuard.Domain.Dtos;

public sealed class ResultDto<T>
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();
    public T? Data { get; init; }
    public ResultFailureKind? FailureKind { get; init; }

    public static ResultDto<T> Success(string message, T? data = default) =>
        new() { IsSuccess = true, Message = message, Data = data };

    public static ResultDto<T> Fail(string message, IEnumerable<string>? errors = null,
        ResultFailureKind failureKind = ResultFailureKind.Validation) =>
        new() { IsSuccess = false, Message = message, Errors = errors?.ToArray() ?? Array.Empty<string>(), FailureKind = failureKind };
}

public enum ResultFailureKind
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Unavailable
}
