using System.Text.Json.Serialization;

namespace TryingDotnet.Controllers;

public class Result<TError, TValue> where TError : Enum
{
    [JsonPropertyName("error")]
    private readonly TError? _maybeError;
    [JsonPropertyName("value")]
    private readonly TValue? _maybeValue;

    [JsonConstructor]
    private Result(TError? error, TValue? value)
    {
        _maybeError = error;
        _maybeValue = value;
    }

    [JsonIgnore]
    public bool IsSuccess => _maybeValue != null;

    public TValue? Value => _maybeValue;

    public TError? Error => _maybeError;

    public static Result<TErr, TVal> Success<TErr, TVal>(TVal value) where TErr : Enum => new(default, value);

    public static Result<TErr, TVal> Failure<TErr, TVal>(TErr errorCode) where TErr : Enum => new(errorCode, default);
}

public enum RpcError
{
    NotFound,
    GeneralError
}