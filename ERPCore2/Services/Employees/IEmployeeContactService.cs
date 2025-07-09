using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    /// <summary>
    /// 員工聯絡資料服務介面
    /// </summary>
    public interface IEmployeeContactService : IGenericManagementService<EmployeeContact>
    {
        /// <summary>
        /// 根據聯絡類型名稱取得員工的聯絡資料值
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="contactTypeName">聯絡類型名稱</param>
        /// <param name="contactTypes">聯絡類型清單</param>
        /// <param name="employeeContacts">員工聯絡資料清單</param>
        /// <returns>聯絡資料值</returns>
        string GetContactValue(int employeeId, string contactTypeName, 
            List<ContactType> contactTypes, List<EmployeeContact> employeeContacts);

        /// <summary>
        /// 更新員工聯絡資料值
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <param name="contactTypeName">聯絡類型名稱</param>
        /// <param name="value">新的聯絡資料值</param>
        /// <param name="contactTypes">聯絡類型清單</param>
        /// <param name="employeeContacts">員工聯絡資料清單（會被修改）</param>
        /// <returns>更新結果</returns>
        ServiceResult UpdateContactValue(int employeeId, string contactTypeName, string value,
            List<ContactType> contactTypes, List<EmployeeContact> employeeContacts);

        /// <summary>
        /// 計算已完成的聯絡資料數量
        /// </summary>
        /// <param name="employeeContacts">員工聯絡資料清單</param>
        /// <returns>已完成的聯絡資料數量</returns>
        int GetContactCompletedFieldsCount(List<EmployeeContact> employeeContacts);

        /// <summary>
        /// 驗證員工聯絡資料
        /// </summary>
        /// <param name="employeeContacts">員工聯絡資料清單</param>
        /// <returns>驗證結果</returns>
        ServiceResult ValidateEmployeeContacts(List<EmployeeContact> employeeContacts);

        /// <summary>
        /// 確保每種聯絡類型只有一個主要聯絡方式
        /// </summary>
        /// <param name="employeeContacts">員工聯絡資料清單（會被修改）</param>
        /// <returns>修正結果</returns>
        ServiceResult EnsureUniquePrimaryContacts(List<EmployeeContact> employeeContacts);

        /// <summary>
        /// 根據員工ID取得所有聯絡資料
        /// </summary>
        /// <param name="employeeId">員工ID</param>
        /// <returns>員工聯絡資料清單</returns>
        Task<ServiceResult<List<EmployeeContact>>> GetByEmployeeIdAsync(int employeeId);

        /// <summary>
        /// 根據聯絡類型取得所有員工聯絡資料
        /// </summary>
        /// <param name="contactTypeId">聯絡類型ID</param>
        /// <returns>員工聯絡資料清單</returns>
        Task<ServiceResult<List<EmployeeContact>>> GetByContactTypeAsync(int contactTypeId);

        /// <summary>
        /// 設定主要聯絡方式
        /// </summary>
        /// <param name="employeeContactId">員工聯絡資料ID</param>
        /// <returns>操作結果</returns>
        Task<ServiceResult> SetAsPrimaryAsync(int employeeContactId);
    }
}

