using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DIFC.Application.DTOs.Auth
{
    public class UserResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public object Data { get; set; }
        public IEnumerable<string> Errors { get; set; }

        public static UserResultDTO SuccessResult(string message, object data = null)
        {
            return new UserResultDTO    
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static UserResultDTO FailureResult(IEnumerable<string> errors)
        {
            return new UserResultDTO
            {
                Success = false,
                Message = errors?.FirstOrDefault(), // ensures message is always set
                Errors = errors
            };
        }

        public static UserResultDTO FailureResult(string error)
        {
            return new UserResultDTO
            {
                Success = false,
                Message = error,
                Errors = new List<string> { error } 
            };
        }
    }
}
