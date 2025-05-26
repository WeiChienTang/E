namespace ERPCore2.Services.Models
{
    /// <summary>
    /// 服務層操作結果的通用類別
    /// </summary>
    public class ServiceResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<string> ValidationErrors { get; set; } = new();

        public static ServiceResult Success()
        {
            return new ServiceResult { IsSuccess = true };
        }

        public static ServiceResult Failure(string errorMessage)
        {
            return new ServiceResult 
            { 
                IsSuccess = false, 
                ErrorMessage = errorMessage 
            };
        }

        public static ServiceResult ValidationFailure(List<string> validationErrors)
        {
            return new ServiceResult 
            { 
                IsSuccess = false, 
                ValidationErrors = validationErrors,
                ErrorMessage = "驗證失敗"
            };
        }
    }

    /// <summary>
    /// 服務層操作結果的泛型類別，包含回傳資料
    /// </summary>
    /// <typeparam name="T">回傳資料的類型</typeparam>
    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }

        public static ServiceResult<T> Success(T data)
        {
            return new ServiceResult<T> 
            { 
                IsSuccess = true, 
                Data = data 
            };
        }

        public static new ServiceResult<T> Failure(string errorMessage)
        {
            return new ServiceResult<T> 
            { 
                IsSuccess = false, 
                ErrorMessage = errorMessage 
            };
        }

        public static new ServiceResult<T> ValidationFailure(List<string> validationErrors)
        {
            return new ServiceResult<T> 
            { 
                IsSuccess = false, 
                ValidationErrors = validationErrors,
                ErrorMessage = "驗證失敗"
            };
        }
    }
}
