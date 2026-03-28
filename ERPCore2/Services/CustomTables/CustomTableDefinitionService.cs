using ERPCore2.Data.Context;
using ERPCore2.Data.Entities.CustomTables;
using ERPCore2.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERPCore2.Services.CustomTables
{
    /// <summary>
    /// 自訂資料表定義服務 - 管理表定義及其欄位定義
    /// </summary>
    public class CustomTableDefinitionService
        : GenericManagementService<CustomTableDefinition>, ICustomTableDefinitionService
    {
        public CustomTableDefinitionService(
            IDbContextFactory<AppDbContext> contextFactory,
            ILogger<GenericManagementService<CustomTableDefinition>> logger)
            : base(contextFactory, logger)
        {
        }

        public CustomTableDefinitionService(
            IDbContextFactory<AppDbContext> contextFactory)
            : base(contextFactory)
        {
        }

        #region Override Base Methods

        protected override IQueryable<CustomTableDefinition> BuildGetAllQuery(AppDbContext context)
        {
            return context.CustomTableDefinitions
                .Include(t => t.FieldDefinitions.OrderBy(f => f.SortOrder))
                .OrderByDescending(t => t.CreatedAt);
        }

        public override async Task<CustomTableDefinition?> GetByIdAsync(int id)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CustomTableDefinitions
                    .Include(t => t.FieldDefinitions.OrderBy(f => f.SortOrder))
                    .FirstOrDefaultAsync(t => t.Id == id);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(GetByIdAsync), GetType(), _logger,
                    new { Id = id });
                return null;
            }
        }

        public override async Task<List<CustomTableDefinition>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CustomTableDefinitions
                    .Include(t => t.FieldDefinitions.OrderBy(f => f.SortOrder))
                    .Where(t => t.TableName.Contains(searchTerm) ||
                                (t.Description != null && t.Description.Contains(searchTerm)) ||
                                (t.Code != null && t.Code.Contains(searchTerm)))
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(SearchAsync), GetType(), _logger,
                    new { SearchTerm = searchTerm });
                return new List<CustomTableDefinition>();
            }
        }

        public override async Task<ServiceResult> ValidateAsync(CustomTableDefinition entity)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(entity.TableName))
                errors.Add("資料表名稱為必填");

            if (await IsTableNameExistsAsync(entity.TableName, entity.Id == 0 ? null : entity.Id))
                errors.Add("此資料表名稱已存在");

            return errors.Any()
                ? ServiceResult.Failure(string.Join("; ", errors))
                : ServiceResult.Success();
        }

        public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
        {
            return await IsTableNameExistsAsync(name, excludeId);
        }

        #endregion

        #region Table Definition Methods

        public async Task<CustomTableDefinition?> GetByIdWithFieldsAsync(int id)
        {
            return await GetByIdAsync(id);
        }

        public async Task<bool> IsTableNameExistsAsync(string tableName, int? excludeId = null)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CustomTableDefinitions
                    .AnyAsync(t => t.TableName == tableName &&
                                   (!excludeId.HasValue || t.Id != excludeId.Value));
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(IsTableNameExistsAsync), GetType(), _logger,
                    new { TableName = tableName });
                return false;
            }
        }

        #endregion

        #region Field Definition Methods

        public async Task<List<CustomFieldDefinition>> GetFieldDefinitionsAsync(int tableDefinitionId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                return await context.CustomFieldDefinitions
                    .Where(f => f.CustomTableDefinitionId == tableDefinitionId)
                    .OrderBy(f => f.SortOrder)
                    .ThenBy(f => f.FieldName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(GetFieldDefinitionsAsync), GetType(), _logger,
                    new { TableDefinitionId = tableDefinitionId });
                return new List<CustomFieldDefinition>();
            }
        }

        public async Task<ServiceResult<CustomFieldDefinition>> AddFieldDefinitionAsync(
            CustomFieldDefinition fieldDefinition)
        {
            try
            {
                var errors = ValidateFieldDefinition(fieldDefinition);

                using var context = await _contextFactory.CreateDbContextAsync();

                // 檢查欄位名稱唯一性
                var nameExists = await context.CustomFieldDefinitions
                    .AnyAsync(f => f.CustomTableDefinitionId == fieldDefinition.CustomTableDefinitionId &&
                                   f.FieldName == fieldDefinition.FieldName);
                if (nameExists)
                    errors.Add($"欄位名稱「{fieldDefinition.FieldName}」已存在");

                if (errors.Any())
                    return ServiceResult<CustomFieldDefinition>.Failure(string.Join("; ", errors));

                fieldDefinition.CreatedAt = DateTime.UtcNow;
                context.CustomFieldDefinitions.Add(fieldDefinition);
                await context.SaveChangesAsync();

                return ServiceResult<CustomFieldDefinition>.Success(fieldDefinition);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(AddFieldDefinitionAsync), GetType(), _logger);
                return ServiceResult<CustomFieldDefinition>.Failure("新增欄位定義時發生錯誤");
            }
        }

        public async Task<ServiceResult<CustomFieldDefinition>> UpdateFieldDefinitionAsync(
            CustomFieldDefinition fieldDefinition)
        {
            try
            {
                var errors = ValidateFieldDefinition(fieldDefinition);

                using var context = await _contextFactory.CreateDbContextAsync();

                var existing = await context.CustomFieldDefinitions
                    .FirstOrDefaultAsync(f => f.Id == fieldDefinition.Id);
                if (existing == null)
                    return ServiceResult<CustomFieldDefinition>.Failure("找不到此欄位定義");

                // 檢查欄位名稱唯一性（排除自己）
                var nameExists = await context.CustomFieldDefinitions
                    .AnyAsync(f => f.CustomTableDefinitionId == existing.CustomTableDefinitionId &&
                                   f.FieldName == fieldDefinition.FieldName &&
                                   f.Id != fieldDefinition.Id);
                if (nameExists)
                    errors.Add($"欄位名稱「{fieldDefinition.FieldName}」已存在");

                if (errors.Any())
                    return ServiceResult<CustomFieldDefinition>.Failure(string.Join("; ", errors));

                existing.FieldName = fieldDefinition.FieldName;
                existing.DisplayName = fieldDefinition.DisplayName;
                existing.FieldType = fieldDefinition.FieldType;
                existing.IsRequired = fieldDefinition.IsRequired;
                existing.SortOrder = fieldDefinition.SortOrder;
                existing.ShowInList = fieldDefinition.ShowInList;
                existing.ShowInForm = fieldDefinition.ShowInForm;
                existing.Options = fieldDefinition.Options;
                existing.DefaultValue = fieldDefinition.DefaultValue;
                existing.Placeholder = fieldDefinition.Placeholder;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = fieldDefinition.UpdatedBy;

                await context.SaveChangesAsync();
                return ServiceResult<CustomFieldDefinition>.Success(existing);
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(UpdateFieldDefinitionAsync), GetType(), _logger);
                return ServiceResult<CustomFieldDefinition>.Failure("更新欄位定義時發生錯誤");
            }
        }

        public async Task<ServiceResult> DeleteFieldDefinitionAsync(int fieldDefinitionId)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                using var transaction = await context.Database.BeginTransactionAsync();

                var fieldDef = await context.CustomFieldDefinitions
                    .FirstOrDefaultAsync(f => f.Id == fieldDefinitionId);
                if (fieldDef == null)
                    return ServiceResult.Failure("找不到此欄位定義");

                // 先刪除所有關聯的欄位值（因為 NoAction cascade）
                await context.CustomFieldValues
                    .Where(v => v.CustomFieldDefinitionId == fieldDefinitionId)
                    .ExecuteDeleteAsync();

                context.CustomFieldDefinitions.Remove(fieldDef);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(DeleteFieldDefinitionAsync), GetType(), _logger,
                    new { FieldDefinitionId = fieldDefinitionId });
                return ServiceResult.Failure("刪除欄位定義時發生錯誤");
            }
        }

        public async Task<ServiceResult> ReorderFieldsAsync(int tableDefinitionId, List<int> orderedFieldIds)
        {
            try
            {
                using var context = await _contextFactory.CreateDbContextAsync();
                var fields = await context.CustomFieldDefinitions
                    .Where(f => f.CustomTableDefinitionId == tableDefinitionId)
                    .ToListAsync();

                for (int i = 0; i < orderedFieldIds.Count; i++)
                {
                    var field = fields.FirstOrDefault(f => f.Id == orderedFieldIds[i]);
                    if (field != null)
                        field.SortOrder = i;
                }

                await context.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await ErrorHandlingHelper.HandleServiceErrorAsync(
                    ex, nameof(ReorderFieldsAsync), GetType(), _logger,
                    new { TableDefinitionId = tableDefinitionId });
                return ServiceResult.Failure("重新排序時發生錯誤");
            }
        }

        #endregion

        #region Private Helpers

        private static List<string> ValidateFieldDefinition(CustomFieldDefinition field)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(field.FieldName))
                errors.Add("欄位名稱為必填");

            if (string.IsNullOrWhiteSpace(field.DisplayName))
                errors.Add("顯示名稱為必填");

            if (field.CustomTableDefinitionId <= 0)
                errors.Add("所屬資料表為必填");

            return errors;
        }

        #endregion
    }
}
