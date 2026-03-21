namespace DIFC.Application.DTOs.Auth
{
    public class GenericResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public IEnumerable<string> Errors { get; set; }

        public static GenericResultDTO SuccessResult(string message)
        {
            return new GenericResultDTO
            {
                Success = true,
                Message = message
            };
        }

        public static GenericResultDTO FailureResult(IEnumerable<string> errors)
        {
            return new GenericResultDTO
            {
                Success = false,
                Errors = errors
            };
        }

        public static GenericResultDTO FailureResult(string error)
        {
            return new GenericResultDTO
            {
                Success = false,
                Errors = new List<string> { error }
            };
        }
    }
}
