using ClosedXML.Excel;
using ERPCore2.Data.Context;
using ERPCore2.Models.Export;
using ERPCore2.Models.Import;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;

namespace ERPCore2.Services.Export
{
    /// <summary>
    /// 資料庫匯出服務 — SuperAdmin 工具
    /// 提供 Entity → Excel 匯出的完整流程
    /// </summary>
    public class DatabaseExportService : IDatabaseExportService
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        // 允許匯出的型別（primitive + 常用型別 + enum）
        private static readonly HashSet<Type> AllowedBaseTypes = new()
        {
            typeof(string), typeof(int), typeof(long), typeof(short), typeof(byte),
            typeof(decimal), typeof(double), typeof(float),
            typeof(bool), typeof(DateTime), typeof(DateOnly), typeof(TimeOnly),
            typeof(Guid)
        };

        public DatabaseExportService(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        #region GetEntityTableList

        /// <inheritdoc />
        public List<EntityTableInfo> GetEntityTableList()
        {
            var dbSetProperties = typeof(AppDbContext)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType.IsGenericType
                         && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .OrderBy(p => p.Name);

            return dbSetProperties.Select(p =>
            {
                var entityType = p.PropertyType.GetGenericArguments()[0];
                return new EntityTableInfo
                {
                    DbSetName = p.Name,
                    EntityTypeName = entityType.FullName ?? entityType.Name,
                    EntityShortName = entityType.Name
                };
            }).ToList();
        }

        #endregion

        #region GetExportableProperties

        /// <inheritdoc />
        public List<EntityPropertyInfo> GetExportableProperties(string dbSetName)
        {
            var entityType = GetEntityType(dbSetName);
            if (entityType == null) return new();

            var properties = entityType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .Where(p => IsAllowedPropertyType(p.PropertyType))
                .OrderBy(p => p.Name);

            return properties.Select(p => BuildPropertyInfo(p)).ToList();
        }

        private EntityPropertyInfo BuildPropertyInfo(PropertyInfo prop)
        {
            var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);
            var isNullable = underlyingType != null || IsNullableReferenceType(prop);
            var actualType = underlyingType ?? prop.PropertyType;
            var isEnum = actualType.IsEnum;
            var hasRequired = prop.GetCustomAttribute<RequiredAttribute>() != null;
            var maxLength = prop.GetCustomAttribute<MaxLengthAttribute>()?.Length;
            var isRequired = hasRequired || (actualType.IsValueType && underlyingType == null);
            var isFkLike = prop.Name.EndsWith("Id", StringComparison.Ordinal)
                          && prop.Name != "Id"
                          && (actualType == typeof(int) || actualType == typeof(long));

            return new EntityPropertyInfo
            {
                PropertyName = prop.Name,
                TypeDisplayName = GetTypeDisplayName(prop.PropertyType),
                PropertyType = prop.PropertyType,
                IsRequired = isRequired,
                IsNullable = isNullable,
                IsEnum = isEnum,
                EnumValues = isEnum ? Enum.GetNames(actualType).ToList() : new(),
                MaxLength = maxLength,
                IsForeignKeyLike = isFkLike
            };
        }

        #endregion

        #region GetTableRowCountAsync

        /// <inheritdoc />
        public async Task<int> GetTableRowCountAsync(string dbSetName)
        {
            var entityType = GetEntityType(dbSetName);
            if (entityType == null) return 0;

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            // 使用反射呼叫 context.Set<TEntity>().CountAsync()
            var setMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!;
            var genericSet = setMethod.MakeGenericMethod(entityType);
            var dbSet = genericSet.Invoke(context, null)!;

            // 取得 IQueryable 並呼叫 CountAsync
            var queryable = dbSet as IQueryable<object>;
            if (queryable != null)
            {
                return await queryable.CountAsync();
            }

            return 0;
        }

        #endregion

        #region ExportSingleTableAsync

        /// <inheritdoc />
        public async Task<ExportResult> ExportSingleTableAsync(string dbSetName, Action<int>? progressCallback = null)
        {
            return await ExportMultipleTablesAsync(new List<string> { dbSetName }, progressCallback);
        }

        #endregion

        #region ExportMultipleTablesAsync

        /// <inheritdoc />
        public async Task<ExportResult> ExportMultipleTablesAsync(List<string> dbSetNames, Action<int>? progressCallback = null)
        {
            if (dbSetNames == null || dbSetNames.Count == 0)
                return ExportResult.Failure("未選擇任何資料表");

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await using var context = await _dbContextFactory.CreateDbContextAsync();
                using var workbook = new XLWorkbook();

                var summaries = new List<ExportTableSummary>();
                int totalRows = 0;
                int completedTables = 0;

                foreach (var dbSetName in dbSetNames)
                {
                    var entityType = GetEntityType(dbSetName);
                    if (entityType == null) continue;

                    var properties = GetExportablePropertyInfos(entityType);
                    if (properties.Count == 0) continue;

                    // 取得資料
                    var data = await GetTableDataAsync(context, entityType);

                    // Worksheet 名稱限制 31 字元
                    var wsName = dbSetName.Length > 31 ? dbSetName[..31] : dbSetName;
                    // 確保 worksheet 名稱不重複
                    var originalWsName = wsName;
                    int suffix = 1;
                    while (workbook.Worksheets.Any(ws => ws.Name == wsName))
                    {
                        var suffixStr = $"_{suffix}";
                        wsName = originalWsName[..Math.Min(originalWsName.Length, 31 - suffixStr.Length)] + suffixStr;
                        suffix++;
                    }

                    var worksheet = workbook.Worksheets.Add(wsName);

                    // 寫入標頭
                    for (int col = 0; col < properties.Count; col++)
                    {
                        var headerCell = worksheet.Cell(1, col + 1);
                        headerCell.Value = properties[col].Name;
                        headerCell.Style.Font.Bold = true;
                        headerCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
                        headerCell.Style.Font.FontColor = XLColor.White;
                        headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }

                    // 寫入資料
                    int row = 2;
                    foreach (var entity in data)
                    {
                        for (int col = 0; col < properties.Count; col++)
                        {
                            var value = properties[col].GetValue(entity);
                            var cell = worksheet.Cell(row, col + 1);
                            SetCellValue(cell, value, properties[col].PropertyType);
                        }
                        row++;
                    }

                    // 自動調整欄寬
                    worksheet.Columns().AdjustToContents(1, Math.Min(row, 100));

                    // 新增自動篩選
                    if (row > 1)
                    {
                        worksheet.Range(1, 1, row - 1, properties.Count).SetAutoFilter();
                    }

                    // 凍結首行
                    worksheet.SheetView.FreezeRows(1);

                    var dataRowCount = row - 2;
                    totalRows += dataRowCount;
                    summaries.Add(new ExportTableSummary
                    {
                        DbSetName = dbSetName,
                        EntityShortName = entityType.Name,
                        RowCount = dataRowCount,
                        ColumnCount = properties.Count
                    });

                    completedTables++;
                    var progress = (int)((double)completedTables / dbSetNames.Count * 100);
                    progressCallback?.Invoke(progress);
                }

                if (summaries.Count == 0)
                    return ExportResult.Failure("沒有可匯出的資料表");

                // 產生 Excel 檔案
                using var ms = new MemoryStream();
                workbook.SaveAs(ms);

                var fileName = dbSetNames.Count == 1
                    ? $"{dbSetNames[0]}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                    : $"DatabaseExport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                stopwatch.Stop();

                return ExportResult.Success(
                    ms.ToArray(),
                    fileName,
                    summaries.Count,
                    totalRows,
                    summaries,
                    stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return ExportResult.Failure($"匯出過程發生錯誤：{ex.InnerException?.Message ?? ex.Message}");
            }
        }

        #endregion

        #region ExportAllTablesAsync

        /// <inheritdoc />
        public async Task<ExportResult> ExportAllTablesAsync(Action<int>? progressCallback = null)
        {
            var allTables = GetEntityTableList();
            var allDbSetNames = allTables.Select(t => t.DbSetName).ToList();
            return await ExportMultipleTablesAsync(allDbSetNames, progressCallback);
        }

        #endregion

        #region 內部工具方法

        /// <summary>
        /// 根據 DbSet 名稱取得 Entity 的 Type
        /// </summary>
        private Type? GetEntityType(string dbSetName)
        {
            var prop = typeof(AppDbContext)
                .GetProperty(dbSetName, BindingFlags.Public | BindingFlags.Instance);

            if (prop == null || !prop.PropertyType.IsGenericType)
                return null;

            return prop.PropertyType.GetGenericArguments().FirstOrDefault();
        }

        /// <summary>
        /// 取得 Entity 的所有可匯出的 PropertyInfo（排除導航屬性）
        /// </summary>
        private List<PropertyInfo> GetExportablePropertyInfos(Type entityType)
        {
            return entityType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .Where(p => IsAllowedPropertyType(p.PropertyType))
                .OrderBy(p => p.Name == "Id" ? 0 : 1)  // Id 排第一
                .ThenBy(p => p.Name)
                .ToList();
        }

        /// <summary>
        /// 取得指定 Entity 的所有資料
        /// </summary>
        private async Task<List<object>> GetTableDataAsync(AppDbContext context, Type entityType)
        {
            // 使用反射呼叫 context.Set<TEntity>().AsNoTracking().ToListAsync()
            var setMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!;
            var genericSet = setMethod.MakeGenericMethod(entityType);
            var dbSet = genericSet.Invoke(context, null)!;

            // 取得 IQueryable
            var queryable = dbSet as IQueryable<object>;
            if (queryable == null) return new();

            return await queryable.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// 設定 Excel 儲存格的值（依型別格式化）
        /// </summary>
        private static void SetCellValue(IXLCell cell, object? value, Type propertyType)
        {
            if (value == null)
            {
                cell.Value = Blank.Value;
                cell.Style.Font.FontColor = XLColor.LightGray;
                return;
            }

            var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (underlyingType == typeof(string))
            {
                cell.Value = value.ToString() ?? string.Empty;
            }
            else if (underlyingType == typeof(int) || underlyingType == typeof(long)
                  || underlyingType == typeof(short) || underlyingType == typeof(byte))
            {
                if (long.TryParse(value.ToString(), out var longVal))
                    cell.Value = longVal;
                else
                    cell.Value = value.ToString();
            }
            else if (underlyingType == typeof(decimal))
            {
                if (decimal.TryParse(value.ToString(), out var decVal))
                    cell.Value = (double)decVal; // ClosedXML 使用 double
                else
                    cell.Value = value.ToString();
            }
            else if (underlyingType == typeof(double))
            {
                if (double.TryParse(value.ToString(), out var dblVal))
                    cell.Value = dblVal;
                else
                    cell.Value = value.ToString();
            }
            else if (underlyingType == typeof(float))
            {
                if (float.TryParse(value.ToString(), out var fltVal))
                    cell.Value = (double)fltVal;
                else
                    cell.Value = value.ToString();
            }
            else if (underlyingType == typeof(bool))
            {
                cell.Value = (bool)value;
            }
            else if (underlyingType == typeof(DateTime))
            {
                cell.Value = (DateTime)value;
                cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
            }
            else if (underlyingType == typeof(DateOnly))
            {
                var dateOnly = (DateOnly)value;
                cell.Value = dateOnly.ToDateTime(TimeOnly.MinValue);
                cell.Style.DateFormat.Format = "yyyy-MM-dd";
            }
            else if (underlyingType == typeof(TimeOnly))
            {
                cell.Value = value.ToString();
            }
            else if (underlyingType == typeof(Guid))
            {
                cell.Value = value.ToString();
            }
            else if (underlyingType.IsEnum)
            {
                cell.Value = value.ToString();
            }
            else
            {
                cell.Value = value.ToString() ?? string.Empty;
            }
        }

        private static bool IsAllowedPropertyType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            return AllowedBaseTypes.Contains(underlyingType) || underlyingType.IsEnum;
        }

        private static bool IsNullableReferenceType(PropertyInfo prop)
        {
            var context = new NullabilityInfoContext();
            var info = context.Create(prop);
            return info.WriteState == NullabilityState.Nullable
                || info.ReadState == NullabilityState.Nullable;
        }

        private static string GetTypeDisplayName(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null)
                return GetSimpleTypeName(underlying) + "?";
            return GetSimpleTypeName(type);
        }

        private static string GetSimpleTypeName(Type type)
        {
            if (type == typeof(string)) return "string";
            if (type == typeof(int)) return "int";
            if (type == typeof(long)) return "long";
            if (type == typeof(short)) return "short";
            if (type == typeof(byte)) return "byte";
            if (type == typeof(decimal)) return "decimal";
            if (type == typeof(double)) return "double";
            if (type == typeof(float)) return "float";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(DateTime)) return "DateTime";
            if (type == typeof(DateOnly)) return "DateOnly";
            if (type == typeof(TimeOnly)) return "TimeOnly";
            if (type == typeof(Guid)) return "Guid";
            if (type.IsEnum) return type.Name;
            return type.Name;
        }

        #endregion
    }
}
