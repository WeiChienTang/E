using ERPCore2.Data.Entities;
using ERPCore2.Services;

namespace ERPCore2.Services
{
    /// <summary>
    /// 員工服務介面
    /// </summary>
    public interface IEmployeeService : IGenericManagementService<Employee>
    {
        /// <summary>
        /// 根據使用者名稱取得員工
        /// </summary>
        /// <param name="username">使用者名稱</param>
        /// <returns>員工資料</returns>
        Task<ServiceResult<Employee>> GetByUsernameAsync(string username);

        /// <summary>
        /// 根據員工編號取得員工
        /// </summary>
        /// <param name="employeeCode">員工編號</param>
        /// <returns>員工資料</returns>
        Task<ServiceResult<Employee>> GetByEmployeeCodeAsync(string employeeCode);

        /// <summary>
        /// 檢查使用者名稱是否已存在
        /// </summary>
        /// <param name="username">使用者名稱</param>
        /// <param name="excludeEmployeeId">排除的員工ID（用於更新時檢查）</param>
        /// <returns>檢查結果</returns>
        Task<ServiceResult<bool>> IsUsernameExistsAsync(string username, int? excludeEmployeeId = null);

        /// <summary>
        /// 檢查員工編號是否已存在
        /// </summary>
        /// <param name="employeeCode">員工編號</param>
        /// <param name="excludeEmployeeId">排除的員工ID（用於更新時檢查）</param>
        /// <returns>檢查結果</returns>
        Task<ServiceResult<bool>> IsEmployeeCodeExistsAsync(string employeeCode, int? excludeEmployeeId = null);

        /// <summary>
        /// 搜尋員工（根據姓名、員工編號或使用者名稱）
        /// </summary>
        /// <param name="searchTerm">搜尋關鍵字</param>
        /// <returns>員工清單</returns>
        Task<ServiceResult<List<Employee>>> SearchEmployeesAsync(string searchTerm);

        /// <summary>
        /// 取得指定角色的所有員工
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>員工清單</returns>
        Task<ServiceResult<List<Employee>>> GetEmployeesByRoleAsync(int roleId);

        /// <summary>
        /// 取得指定部門的所有員工
        /// </summary>
        /// <param name="department">部門名稱</param>
        /// <returns>員工清單</returns>
        Task<ServiceResult<List<Employee>>> GetEmployeesByDepartmentAsync(string department);

        /// <summary>
        /// 變更員工角色
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="newRoleId">新角色ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> ChangeEmployeeRoleAsync(int employeeId, int newRoleId);

        /// <summary>
        /// 啟用/停用員工帳號
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="isActive">是否啟用</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> SetEmployeeActiveStatusAsync(int employeeId, bool isActive);

        /// <summary>
        /// 取得可登入的員工清單（啟用且未刪除）
        /// </summary>
        /// <returns>可登入員工清單</returns>
        Task<ServiceResult<List<Employee>>> GetActiveEmployeesAsync();

        /// <summary>
        /// 驗證員工資料完整性
        /// </summary>
        /// <param name="employee">員工資料</param>
        /// <returns>驗證結果</returns>
        ServiceResult<bool> ValidateEmployeeData(Employee employee);

        /// <summary>
        /// 產生下一個員工編號
        /// </summary>
        /// <param name="prefix">前綴（可選）</param>
        /// <returns>新的員工編號</returns>
        Task<ServiceResult<string>> GenerateNextEmployeeCodeAsync(string prefix = "EMP");

        /// <summary>
        /// 更新員工聯絡資料
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="contacts">聯絡資料清單</param>
        /// <returns>更新結果</returns>
        Task<ServiceResult> UpdateEmployeeContactsAsync(int employeeId, List<EmployeeContact> contacts);

        /// <summary>
        /// 更新員工地址資料
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="addresses">地址資料清單</param>
        /// <returns>更新結果</returns>
        Task<ServiceResult> UpdateEmployeeAddressesAsync(int employeeId, List<EmployeeAddress> addresses);
    }
}

