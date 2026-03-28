using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.CustomTables;
using ERPCore2.Helpers;
using ERPCore2.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.CustomTables
{
    /// <summary>
    /// 自訂資料表資料列服務 - 管理資料列及其欄位值（EAV 模式）
    /// </summary>
    public class CustomTableRowService
        : GenericManagementService<CustomTableRow>, ICustomTableRowService
    {
        public CustomTableRowService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<CustomTableRow>> logger)
            : base(contextFactory, logger)
        {
        }

        public CustomTableRowService(
            IDbContextFactory<AppDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        #region Override Base Methods

        public override async Task<CustomTableRow?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CustomTableRows
                    .Include(r => r.FieldValues)
                        .ThenInclude(v => v.CustomFieldDefinition)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(GetByIdAsync), GetType(), _logger,
                    new { Id = id });
                return null;
            }
        }

        public override async Task<List<CustomTableRow>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();

                // 搜尋 Code 或任何欄位值中包含搜尋詞的資料列
                var matchingRowIds = await context.CustomFieldValues
                    .Where(v => v.Value != null && v.Value.Contains(searchTerm))
                    .Select(v => v.CustomTableRowId)
                    .Distinct()
                    .ToListAsync();

                return await context.CustomTableRows
                    .Include(r => r.FieldValues)
                    .Where(r => (r.Code != null && r.Code.Contains(searchTerm)) ||
                                matchingRowIds.Contains(r.Id))
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(SearchAsync), GetType(), _logger,
                    new { SearchTerm = searchTerm });
                return new List<CustomTableRow>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(CustomTableRow entity)
        {
            var errors = new List<string>();

            if (entity.CustomTableDefinitionId <= 0)
                errors.Add("所屬資料表為必填");
            else
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var tableExists = await context.CustomTableDefinitions
                    .AnyAsync(t => t.Id == entity.CustomTableDefinitionId);
                if (!tableExists)
                    errors.Add("指定的資料表不存在");
            }

            return errors.Any()
                ? ServiceResult.Failure(string.Join("; ", errors))
                : ServiceResult.Success();
        }

        #endregion

        #region Custom Methods

        public async Task<List<CustomTableRow>> GetRowsByTableIdAsync(int tableDefinitionId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CustomTableRows
                    .Include(r => r.FieldValues)
                        .ThenInclude(v => v.CustomFieldDefinition)
                    .Where(r => r.CustomTableDefinitionId == tableDefinitionId && !r.IsDraft)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(GetRowsByTableIdAsync), GetType(), _logger,
                    new { TableDefinitionId = tableDefinitionId });
                return new List<CustomTableRow>();
            }
        }

        public async Task<CustomTableRow?> GetByIdWithValuesAsync(int rowId)
        {
            return await GetByIdAsync(rowId);
        }

        public async Task<ServiceResult<CustomTableRow>> CreateRowWithValuesAsync(
            CustomTableRow row, List<CustomFieldValue> fieldValues)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                // 驗證資料列
                if (!row.IsDraft)
                {
                    var validateResult = await ValidateAsync(row);
                    if (!validateResult.IsSuccess)
                        return ServiceResult<CustomTableRow>.Failure(validateResult.ErrorMessage);

                    // 驗證欄位值
                    var fieldValidation = await ValidateFieldValuesInternalAsync(
                        context, row.CustomTableDefinitionId, fieldValues);
                    if (!fieldValidation.IsSuccess)
                        return ServiceResult<CustomTableRow>.Failure(fieldValidation.ErrorMessage);
                }

                // 設定審計欄位
                row.CreatedAt = DateTime.UtcNow;

                // 儲存資料列以取得 Id
                context.CustomTableRows.Add(row);
                await context.SaveChangesAsync();

                // 儲存欄位值
                foreach (var value in fieldValues)
                {
                    value.CustomTableRowId = row.Id;
                }
                context.CustomFieldValues.AddRange(fieldValues);
                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                // 重新載入完整資料
                return ServiceResult<CustomTableRow>.Success(
                    await GetByIdAsync(row.Id) ?? row);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(CreateRowWithValuesAsync), GetType(), _logger);
                return ServiceResult<CustomTableRow>.Failure("建立資料時發生錯誤");
            }
        }

        public async Task<ServiceResult<CustomTableRow>> UpdateRowWithValuesAsync(
            CustomTableRow row, List<CustomFieldValue> fieldValues)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                var existingRow = await context.CustomTableRows
                    .FirstOrDefaultAsync(r => r.Id == row.Id);
                if (existingRow == null)
                    return ServiceResult<CustomTableRow>.Failure("找不到此資料列");

                // 驗證
                if (!row.IsDraft)
                {
                    var fieldValidation = await ValidateFieldValuesInternalAsync(
                        context, existingRow.CustomTableDefinitionId, fieldValues);
                    if (!fieldValidation.IsSuccess)
                        return ServiceResult<CustomTableRow>.Failure(fieldValidation.ErrorMessage);
                }

                // 更新資料列審計欄位
                existingRow.Code = row.Code;
                existingRow.Status = row.Status;
                existingRow.Remarks = row.Remarks;
                existingRow.IsDraft = row.IsDraft;
                existingRow.UpdatedAt = DateTime.UtcNow;
                existingRow.UpdatedBy = row.UpdatedBy;

                // 差異更新欄位值
                var existingValues = await context.CustomFieldValues
                    .Where(v => v.CustomTableRowId == row.Id)
                    .ToListAsync();

                foreach (var newValue in fieldValues)
                {
                    var existing = existingValues
                        .FirstOrDefault(v => v.CustomFieldDefinitionId == newValue.CustomFieldDefinitionId);

                    if (existing != null)
                    {
                        existing.Value = newValue.Value;
                    }
                    else
                    {
                        newValue.CustomTableRowId = row.Id;
                        context.CustomFieldValues.Add(newValue);
                    }
                }

                // 移除不再存在的欄位值
                var incomingFieldIds = fieldValues
                    .Select(v => v.CustomFieldDefinitionId)
                    .ToHashSet();
                var toRemove = existingValues
                    .Where(v => !incomingFieldIds.Contains(v.CustomFieldDefinitionId));
                context.CustomFieldValues.RemoveRange(toRemove);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult<CustomTableRow>.Success(
                    await GetByIdAsync(row.Id) ?? existingRow);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(UpdateRowWithValuesAsync), GetType(), _logger);
                return ServiceResult<CustomTableRow>.Failure("更新資料時發生錯誤");
            }
        }

        public async Task<(List<CustomTableRow> Items, int TotalCount)> GetPagedByTableIdAsync(
            int tableDefinitionId, int pageNumber, int pageSize, string? searchTerm = null, bool? isDraft = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var query = context.CustomTableRows
                    .Include(r => r.FieldValues)
                        .ThenInclude(v => v.CustomFieldDefinition)
                    .Where(r => r.CustomTableDefinitionId == tableDefinitionId);

                // isDraft: null=正式(預設), true=草稿, false=全部
                if (isDraft == null)
                    query = query.Where(r => !r.IsDraft);
                else if (isDraft == true)
                    query = query.Where(r => r.IsDraft);

                // 搜尋
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var matchingRowIds = await context.CustomFieldValues
                        .Where(v => v.Value != null && v.Value.Contains(searchTerm) &&
                                    v.CustomTableRow!.CustomTableDefinitionId == tableDefinitionId)
                        .Select(v => v.CustomTableRowId)
                        .Distinct()
                        .ToListAsync();

                    query = query.Where(r =>
                        (r.Code != null && r.Code.Contains(searchTerm)) ||
                        matchingRowIds.Contains(r.Id));
                }

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(r => r.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(GetPagedByTableIdAsync), GetType(), _logger,
                    new { TableDefinitionId = tableDefinitionId });
                return (new List<CustomTableRow>(), 0);
            }
        }

        public async Task<ServiceResult> ValidateFieldValuesAsync(
            int tableDefinitionId, List<CustomFieldValue> fieldValues)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await ValidateFieldValuesInternalAsync(context, tableDefinitionId, fieldValues);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(ValidateFieldValuesAsync), GetType(), _logger);
                return ServiceResult.Failure("驗證欄位值時發生錯誤");
            }
        }

        #endregion

        #region Private Helpers

        private static async Task<ServiceResult> ValidateFieldValuesInternalAsync(
            AppDbContext context, int tableDefinitionId, List<CustomFieldValue> fieldValues)
        {
            var errors = new List<string>();

            var fieldDefinitions = await context.CustomFieldDefinitions
                .Where(f => f.CustomTableDefinitionId == tableDefinitionId && f.ShowInForm)
                .ToListAsync();

            foreach (var fieldDef in fieldDefinitions)
            {
                var value = fieldValues
                    .FirstOrDefault(v => v.CustomFieldDefinitionId == fieldDef.Id);
                var rawValue = value?.Value;

                // 必填檢查
                if (fieldDef.IsRequired && string.IsNullOrWhiteSpace(rawValue))
                {
                    errors.Add($"「{fieldDef.DisplayName}」為必填");
                    continue;
                }

                // 型別檢查（有值時才檢查）
                if (!string.IsNullOrWhiteSpace(rawValue))
                {
                    switch (fieldDef.FieldType)
                    {
                        case CustomFieldType.Number:
                            if (!decimal.TryParse(rawValue, out _))
                                errors.Add($"「{fieldDef.DisplayName}」必須為數值");
                            break;
                        case CustomFieldType.Date:
                            if (!DateTime.TryParse(rawValue, out _))
                                errors.Add($"「{fieldDef.DisplayName}」必須為有效日期");
                            break;
                        case CustomFieldType.DateTime:
                            if (!DateTime.TryParse(rawValue, out _))
                                errors.Add($"「{fieldDef.DisplayName}」必須為有效日期時間");
                            break;
                        case CustomFieldType.Boolean:
                            if (rawValue != "true" && rawValue != "false")
                                errors.Add($"「{fieldDef.DisplayName}」必須為 true 或 false");
                            break;
                    }
                }
            }

            return errors.Any()
                ? ServiceResult.Failure(string.Join("; ", errors))
                : ServiceResult.Success();
        }

        #endregion
    }
}
