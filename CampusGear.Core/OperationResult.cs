namespace CampusGear.Core;

/// <summary>Outcome of an operation, with a message the UI can show as-is.</summary>
public class OperationResult
{
    public bool Success { get; }
    public string Message { get; }

    protected OperationResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public static OperationResult Ok(string message) => new(true, message);

    public static OperationResult Fail(string message) => new(false, message);
}

public sealed class OperationResult<T> : OperationResult
{
    public T? Value { get; }

    private OperationResult(bool success, string message, T? value) : base(success, message) => Value = value;

    public static OperationResult<T> Ok(string message, T value) => new(true, message, value);

    public new static OperationResult<T> Fail(string message) => new(false, message, default);
}
