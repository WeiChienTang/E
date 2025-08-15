using ERPCore2.Data;
using Microsoft.AspNetCore.Components;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// Modal 操作的統一處理 Helper
    /// 提供新增、編輯、儲存、取消等 Modal 常用操作的統一處理方法
    /// </summary>
    public static class ModalHelper
    {
        /// <summary>
        /// 處理顯示新增 Modal 的操作
        /// </summary>
        /// <param name="setEditingId">設定編輯ID的委派</param>
        /// <param name="setModalVisible">設定Modal可見性的委派</param>
        /// <param name="stateHasChanged">通知組件狀態改變的委派</param>
        /// <param name="callerName">呼叫者名稱（用於錯誤記錄）</param>
        /// <param name="callerType">呼叫者類型（用於錯誤記錄）</param>
        /// <returns></returns>
        public static async Task HandleShowAddModalAsync(
            Action<int?> setEditingId,
            Action<bool> setModalVisible,
            Action stateHasChanged,
            string callerName,
            Type callerType)
        {
            try
            {
                setEditingId(null);
                setModalVisible(true);
                stateHasChanged();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandlePageErrorAsync(ex, callerName, callerType);
            }
        }

        /// <summary>
        /// 處理顯示編輯 Modal 的操作
        /// </summary>
        /// <typeparam name="TEntity">實體類型</typeparam>
        /// <param name="entity">要編輯的實體</param>
        /// <param name="setEditingId">設定編輯ID的委派</param>
        /// <param name="setModalVisible">設定Modal可見性的委派</param>
        /// <param name="stateHasChanged">通知組件狀態改變的委派</param>
        /// <param name="callerName">呼叫者名稱（用於錯誤記錄）</param>
        /// <param name="callerType">呼叫者類型（用於錯誤記錄）</param>
        /// <returns></returns>
        public static async Task HandleShowEditModalAsync<TEntity>(
            TEntity entity,
            Action<int?> setEditingId,
            Action<bool> setModalVisible,
            Action stateHasChanged,
            string callerName,
            Type callerType) where TEntity : BaseEntity
        {
            try
            {
                if (entity?.Id != null)
                {
                    setEditingId(entity.Id);
                    setModalVisible(true);
                    stateHasChanged();
                }
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandlePageErrorAsync(ex, callerName, callerType);
            }
        }

        /// <summary>
        /// 處理實體儲存後的操作
        /// </summary>
        /// <typeparam name="TEntity">實體類型</typeparam>
        /// <typeparam name="TComponent">組件類型</typeparam>
        /// <param name="savedEntity">儲存的實體</param>
        /// <param name="setEditingId">設定編輯ID的委派</param>
        /// <param name="setModalVisible">設定Modal可見性的委派</param>
        /// <param name="indexComponent">索引頁面組件參考</param>
        /// <param name="stateHasChanged">通知組件狀態改變的委派</param>
        /// <param name="callerName">呼叫者名稱（用於錯誤記錄）</param>
        /// <param name="callerType">呼叫者類型（用於錯誤記錄）</param>
        /// <returns></returns>
        public static async Task HandleEntitySavedAsync<TEntity, TComponent>(
            TEntity savedEntity,
            Action<int?> setEditingId,
            Action<bool> setModalVisible,
            TComponent? indexComponent,
            Action stateHasChanged,
            string callerName,
            Type callerType) where TEntity : BaseEntity
        {
            try
            {
                // 關閉 Modal
                setModalVisible(false);
                setEditingId(null);

                // 重新載入資料（如果組件支援 Refresh 方法）
                if (indexComponent != null)
                {
                    // 使用反射檢查是否有 Refresh 方法
                    var refreshMethod = indexComponent.GetType().GetMethod("Refresh");
                    if (refreshMethod != null)
                    {
                        var result = refreshMethod.Invoke(indexComponent, null);
                        if (result is Task task)
                        {
                            await task;
                        }
                    }
                }

                stateHasChanged();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandlePageErrorAsync(ex, callerName, callerType);
            }
        }

        /// <summary>
        /// 處理 Modal 取消操作
        /// </summary>
        /// <param name="setEditingId">設定編輯ID的委派</param>
        /// <param name="setModalVisible">設定Modal可見性的委派</param>
        /// <param name="stateHasChanged">通知組件狀態改變的委派</param>
        /// <param name="callerName">呼叫者名稱（用於錯誤記錄）</param>
        /// <param name="callerType">呼叫者類型（用於錯誤記錄）</param>
        /// <returns></returns>
        public static async Task HandleModalCancelAsync(
            Action<int?> setEditingId,
            Action<bool> setModalVisible,
            Action stateHasChanged,
            string callerName,
            Type callerType)
        {
            try
            {
                setModalVisible(false);
                setEditingId(null);
                stateHasChanged();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandlePageErrorAsync(ex, callerName, callerType);
            }
        }

        /// <summary>
        /// 為組件建立 Modal 處理器的簡化版本
        /// 返回一個包含所有 Modal 操作方法的物件
        /// </summary>
        /// <typeparam name="TEntity">實體類型</typeparam>
        /// <typeparam name="TComponent">組件類型</typeparam>
        /// <param name="setEditingId">設定編輯ID的委派</param>
        /// <param name="setModalVisible">設定Modal可見性的委派</param>
        /// <param name="getIndexComponent">取得索引組件的委派</param>
        /// <param name="stateHasChanged">通知組件狀態改變的委派</param>
        /// <param name="callerType">呼叫者類型</param>
        /// <returns>Modal處理器物件</returns>
        public static ModalHandler<TEntity, TComponent> CreateModalHandler<TEntity, TComponent>(
            Action<int?> setEditingId,
            Action<bool> setModalVisible,
            Func<TComponent?> getIndexComponent,
            Action stateHasChanged,
            Type callerType) where TEntity : BaseEntity
        {
            return new ModalHandler<TEntity, TComponent>(
                setEditingId,
                setModalVisible,
                getIndexComponent,
                stateHasChanged,
                callerType);
        }
    }

    /// <summary>
    /// Modal 處理器類別，包裝所有常用的 Modal 操作
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <typeparam name="TComponent">組件類型</typeparam>
    public class ModalHandler<TEntity, TComponent> where TEntity : BaseEntity
    {
        private readonly Action<int?> _setEditingId;
        private readonly Action<bool> _setModalVisible;
        private readonly Func<TComponent?> _getIndexComponent;
        private readonly Action _stateHasChanged;
        private readonly Type _callerType;

        public ModalHandler(
            Action<int?> setEditingId,
            Action<bool> setModalVisible,
            Func<TComponent?> getIndexComponent,
            Action stateHasChanged,
            Type callerType)
        {
            _setEditingId = setEditingId;
            _setModalVisible = setModalVisible;
            _getIndexComponent = getIndexComponent;
            _stateHasChanged = stateHasChanged;
            _callerType = callerType;
        }

        /// <summary>
        /// 顯示新增 Modal
        /// </summary>
        /// <returns></returns>
        public async Task ShowAddModalAsync()
        {
            await ModalHelper.HandleShowAddModalAsync(
                _setEditingId,
                _setModalVisible,
                _stateHasChanged,
                nameof(ShowAddModalAsync),
                _callerType);
        }

        /// <summary>
        /// 顯示編輯 Modal
        /// </summary>
        /// <param name="entity">要編輯的實體</param>
        /// <returns></returns>
        public async Task ShowEditModalAsync(TEntity entity)
        {
            await ModalHelper.HandleShowEditModalAsync(
                entity,
                _setEditingId,
                _setModalVisible,
                _stateHasChanged,
                nameof(ShowEditModalAsync),
                _callerType);
        }

        /// <summary>
        /// 處理實體儲存
        /// </summary>
        /// <param name="savedEntity">儲存的實體</param>
        /// <returns></returns>
        public async Task OnEntitySavedAsync(TEntity savedEntity)
        {
            await ModalHelper.HandleEntitySavedAsync(
                savedEntity,
                _setEditingId,
                _setModalVisible,
                _getIndexComponent(),
                _stateHasChanged,
                nameof(OnEntitySavedAsync),
                _callerType);
        }

        /// <summary>
        /// 處理 Modal 取消
        /// </summary>
        /// <returns></returns>
        public async Task OnModalCancelAsync()
        {
            await ModalHelper.HandleModalCancelAsync(
                _setEditingId,
                _setModalVisible,
                _stateHasChanged,
                nameof(OnModalCancelAsync),
                _callerType);
        }
    }
}
