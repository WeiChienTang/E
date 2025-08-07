using Microsoft.AspNetCore.Components;

namespace ERPCore2.Services
{
    /// <summary>
    /// Modal 服務，用於在任何組件中開啟 Modal 視窗
    /// </summary>
    public class ModalService
    {
        public event Action<ModalRequest>? OnModalRequested;
        public event Action? OnModalClosed;

        /// <summary>
        /// 開啟新增部門 Modal
        /// </summary>
        public void OpenAddDepartmentModal(Action? onSuccess = null)
        {
            var request = new ModalRequest
            {
                Type = ModalType.AddDepartment,
                OnSuccess = onSuccess
            };
            OnModalRequested?.Invoke(request);
        }

        /// <summary>
        /// 開啟編輯部門 Modal
        /// </summary>
        public void OpenEditDepartmentModal(int departmentId, Action? onSuccess = null)
        {
            var request = new ModalRequest
            {
                Type = ModalType.EditDepartment,
                EntityId = departmentId,
                OnSuccess = onSuccess
            };
            OnModalRequested?.Invoke(request);
        }

        /// <summary>
        /// 關閉 Modal
        /// </summary>
        public void CloseModal()
        {
            OnModalClosed?.Invoke();
        }
    }

    /// <summary>
    /// Modal 請求類別
    /// </summary>
    public class ModalRequest
    {
        public ModalType Type { get; set; }
        public int? EntityId { get; set; }
        public Action? OnSuccess { get; set; }
    }

    /// <summary>
    /// Modal 類型列舉
    /// </summary>
    public enum ModalType
    {
        AddDepartment,
        EditDepartment,
        AddEmployee,
        EditEmployee
        // 可以繼續擴展其他實體類型
    }
}
