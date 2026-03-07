using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DIFC.Application.DTOs.Auth
{
    public class RoleResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        public object Data { get; set; }
        public IEnumerable<string> Errors { get; set; }

        public static RoleResultDTO SuccessResult(string message, object data = null)
        {
            return new RoleResultDTO    
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static RoleResultDTO FailureResult(IEnumerable<string> errors)
        {
            return new RoleResultDTO
            {
                Success = false,
                Errors = errors
            };
        }

        public static RoleResultDTO FailureResult(string error)
        {
            return new RoleResultDTO
            {
                Success = false,
                Errors = new List<string> { error }
            };
        }
    }
}
