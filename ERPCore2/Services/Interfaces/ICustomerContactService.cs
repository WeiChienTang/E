using ERPCore2.Data.Entities;

namespace ERPCore2.Services.Interfaces
{
    /// <summary>
    /// 客戶聯絡資料服務介面
    /// </summary>
    public interface ICustomerContactService
    {
        /// <summary>
        /// 根據聯絡類型名稱取得客戶的聯絡資料值
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <param name="contactTypeName">聯絡類型名稱</param>
        /// <param name="contactTypes">聯絡類型清單</param>
        /// <param name="customerContacts">客戶聯絡資料清單</param>
        /// <returns>聯絡資料值</returns>
        string GetContactValue(int customerId, string contactTypeName, 
            List<ContactType> contactTypes, List<CustomerContact> customerContacts);

        /// <summary>
        /// 更新客戶聯絡資料值
        /// </summary>
        /// <param name="customerId">客戶ID</param>
        /// <param name="contactTypeName">聯絡類型名稱</param>
        /// <param name="value">新的聯絡資料值</param>
        /// <param name="contactTypes">聯絡類型清單</param>
        /// <param name="customerContacts">客戶聯絡資料清單（會被修改）</param>
        /// <returns>更新結果</returns>
        ServiceResult UpdateContactValue(int customerId, string contactTypeName, string value,
            List<ContactType> contactTypes, List<CustomerContact> customerContacts);

        /// <summary>
        /// 計算已完成的聯絡資料數量
        /// </summary>
        /// <param name="customerContacts">客戶聯絡資料清單</param>
        /// <returns>已完成的聯絡資料數量</returns>
        int GetContactCompletedFieldsCount(List<CustomerContact> customerContacts);

        /// <summary>
        /// 驗證客戶聯絡資料
        /// </summary>
        /// <param name="customerContacts">客戶聯絡資料清單</param>
        /// <returns>驗證結果</returns>
        ServiceResult ValidateCustomerContacts(List<CustomerContact> customerContacts);

        /// <summary>
        /// 確保每種聯絡類型只有一個主要聯絡方式
        /// </summary>
        /// <param name="customerContacts">客戶聯絡資料清單（會被修改）</param>
        /// <returns>修正結果</returns>
        ServiceResult EnsureUniquePrimaryContacts(List<CustomerContact> customerContacts);
    }
}
