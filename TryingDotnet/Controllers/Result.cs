using System.Text.Json.Serialization;

namespace TryingDotnet.Controllers;

public class Result<TError, TValue> where TError : struct, Enum where TValue : class
{
    [JsonPropertyName("error")]
    [JsonInclude]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    private readonly TError? maybeError;

    [JsonPropertyName("value")]
    [JsonInclude]
    private readonly TValue? maybeValue;
    
    [JsonConstructor]
    private Result(TError? maybeError, TValue? maybeValue)
    {
        this.maybeError = maybeError;
        this.maybeValue = maybeValue;
    }

    [JsonIgnore] 
    public bool IsSuccess => maybeValue != null;

    [JsonIgnore]
    public TValue Value => maybeValue ?? throw new InvalidOperationException("Check for success/failure first!");

    [JsonIgnore]
    public TError Error => maybeError ?? throw new InvalidOperationException("Check for success/failure first!");

    public static Result<TError, TValue> Success(TValue value) => new(null, value);

    public static Result<TError, TValue> Failure(TError errorCode) => new(errorCode, null);
}

public enum RpcError
{
    NotFound,
    GeneralError
}