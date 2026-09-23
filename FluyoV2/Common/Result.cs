public class Result
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public static Result SuccessResult(string message) =>
        new Result { Success = true, Message = message };

    public static Result Failure(string message) =>
        new Result { Success = false, Message = message };
}
