namespace DIFC.Application.DTOs.Auth
{
    public class AuthResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public IEnumerable<string> Errors { get; set; }

        public static AuthResultDTO SuccessResult(string message)
        {
            return new AuthResultDTO
            {
                Success = true,
                Message = message
            };
        }

        public static AuthResultDTO FailureResult(IEnumerable<string> errors)
        {
            return new AuthResultDTO
            {
                Success = false,
                Errors = errors
            };
        }

        public static AuthResultDTO FailureResult(string error)
        {
            return new AuthResultDTO
            {
                Success = false,
                Errors = new List<string> { error }
            };
        }
    }
}
