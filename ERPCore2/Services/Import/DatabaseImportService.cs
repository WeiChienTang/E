using ClosedXML.Excel;
using ERPCore2.Data;
using ERPCore2.Data.Context;
using ERPCore2.Models.Enums;
using ERPCore2.Models.Import;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;

namespace ERPCore2.Services.Import
{
    /// <summary>
    /// 資料庫匯入服務 — SuperAdmin 工具
    /// 提供 Excel → Entity 匯入的完整流程
    /// </summary>
    public class DatabaseImportService : IDatabaseImportService
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        // BaseEntity 共用欄位名稱 — 排除在對應清單外，系統自動處理
        private static readonly HashSet<string> BaseEntityFields = new(StringComparer.OrdinalIgnoreCase)
        {
            "Id", "Status", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy"
        };

        // 允許對應的型別（primitive + 常用型別 + enum）
        private static readonly HashSet<Type> AllowedBaseTypes = new()
        {
            typeof(string), typeof(int), typeof(long), typeof(short), typeof(byte),
            typeof(decimal), typeof(double), typeof(float),
            typeof(bool), typeof(DateTime), typeof(DateOnly), typeof(TimeOnly),
            typeof(Guid)
        };

        public DatabaseImportService(IDbContextFactory<AppDbContext> dbContextFactory)
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

        #region GetEntityProperties

        /// <inheritdoc />
        public List<EntityPropertyInfo> GetEntityProperties(string dbSetName)
        {
            var entityType = GetEntityType(dbSetName);
            if (entityType == null) return new();

            var properties = entityType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .Where(p => !BaseEntityFields.Contains(p.Name))
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

            // 必填判斷：value type（非 Nullable）或有 [Required]
            var isRequired = hasRequired || (actualType.IsValueType && underlyingType == null);

            // FK 判斷：名稱以 "Id" 結尾、型別為 int 或 int?、且不是 "Id" 本身
            var isFkLike = prop.Name.EndsWith("Id", StringComparison.Ordinal)
                          && prop.Name != "Id"
                          && (actualType == typeof(int) || actualType == typeof(long));

            var typeDisplay = GetTypeDisplayName(prop.PropertyType);

            return new EntityPropertyInfo
            {
                PropertyName = prop.Name,
                TypeDisplayName = typeDisplay,
                PropertyType = prop.PropertyType,
                IsRequired = isRequired,
                IsNullable = isNullable,
                IsEnum = isEnum,
                EnumValues = isEnum ? Enum.GetNames(actualType).ToList() : new(),
                MaxLength = maxLength,
                IsForeignKeyLike = isFkLike
            };
        }

        private static bool IsNullableReferenceType(PropertyInfo prop)
        {
            // 使用 NullabilityInfoContext 判斷 reference type 的 nullable 註解
            var context = new NullabilityInfoContext();
            var info = context.Create(prop);
            return info.WriteState == NullabilityState.Nullable
                || info.ReadState == NullabilityState.Nullable;
        }

        private static bool IsAllowedPropertyType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            return AllowedBaseTypes.Contains(underlyingType) || underlyingType.IsEnum;
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

        #region ParseExcel

        /// <inheritdoc />
        public ExcelParseResult ParseExcel(Stream stream, string fileName)
        {
            try
            {
                if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    return ExcelParseResult.Failure("僅支援 .xlsx 格式的 Excel 檔案");

                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    return ExcelParseResult.Failure("Excel 檔案中沒有找到任何 Worksheet");

                var usedRange = worksheet.RangeUsed();
                if (usedRange == null)
                    return ExcelParseResult.Failure("Worksheet 中沒有任何資料");

                var firstRow = usedRange.FirstRow();
                var headers = new List<string>();
                foreach (var cell in firstRow.Cells())
                {
                    var value = cell.GetString().Trim();
                    if (!string.IsNullOrEmpty(value))
                        headers.Add(value);
                    else
                        headers.Add($"Column{cell.Address.ColumnNumber}");
                }

                if (headers.Count == 0)
                    return ExcelParseResult.Failure("Excel 第一行（標頭行）沒有有效的欄位名稱");

                var rows = new List<Dictionary<string, string?>>();
                var dataRows = usedRange.RowsUsed().Skip(1); // 跳過標頭行

                foreach (var row in dataRows)
                {
                    var rowData = new Dictionary<string, string?>();
                    for (int i = 0; i < headers.Count; i++)
                    {
                        var cell = row.Cell(i + 1);
                        var cellValue = cell.IsEmpty() ? null : cell.GetString().Trim();
                        rowData[headers[i]] = cellValue;
                    }
                    rows.Add(rowData);
                }

                return ExcelParseResult.Success(worksheet.Name, headers, rows);
            }
            catch (Exception ex)
            {
                return ExcelParseResult.Failure($"解析 Excel 失敗：{ex.Message}");
            }
        }

        #endregion

        #region AutoMapColumns

        /// <inheritdoc />
        public List<ColumnMapping> AutoMapColumns(List<EntityPropertyInfo> targetProperties, List<string> sourceHeaders)
        {
            var mappings = new List<ColumnMapping>();
            var usedSources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var prop in targetProperties)
            {
                var mapping = new ColumnMapping
                {
                    TargetPropertyName = prop.PropertyName,
                    TargetProperty = prop
                };

                // 嘗試自動配對
                var bestMatch = FindBestMatch(prop.PropertyName, sourceHeaders, usedSources);

                if (bestMatch.Score >= 70)
                {
                    mapping.SourceColumnName = bestMatch.Header;
                    mapping.AutoMatchScore = bestMatch.Score;
                    mapping.Status = MappingStatus.AutoSuggested;
                    usedSources.Add(bestMatch.Header);
                }
                else
                {
                    mapping.Status = MappingStatus.Unmapped;
                }

                mappings.Add(mapping);
            }

            return mappings;
        }

        private static (string Header, int Score) FindBestMatch(
            string propertyName, List<string> sourceHeaders, HashSet<string> usedSources)
        {
            string bestHeader = string.Empty;
            int bestScore = 0;

            var normalizedProp = NormalizeName(propertyName);

            foreach (var header in sourceHeaders)
            {
                if (usedSources.Contains(header)) continue;

                var normalizedHeader = NormalizeName(header);
                var score = CalculateSimilarity(normalizedProp, normalizedHeader, propertyName, header);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestHeader = header;
                }
            }

            return (bestHeader, bestScore);
        }

        private static int CalculateSimilarity(string normalizedProp, string normalizedHeader, string rawProp, string rawHeader)
        {
            // 1. 完全相符（忽略大小寫）
            if (string.Equals(rawProp, rawHeader, StringComparison.OrdinalIgnoreCase))
                return 100;

            // 2. 正規化後完全相符（去底線/空格/連字號）
            if (string.Equals(normalizedProp, normalizedHeader, StringComparison.OrdinalIgnoreCase))
                return 90;

            // 3. 包含關係
            if (normalizedProp.Contains(normalizedHeader, StringComparison.OrdinalIgnoreCase)
                || normalizedHeader.Contains(normalizedProp, StringComparison.OrdinalIgnoreCase))
                return 70;

            // 4. 去除常見前綴後比對
            var strippedHeader = StripCommonPrefixes(normalizedHeader);
            if (!string.IsNullOrEmpty(strippedHeader)
                && string.Equals(normalizedProp, strippedHeader, StringComparison.OrdinalIgnoreCase))
                return 60;

            return 0;
        }

        private static string NormalizeName(string name)
        {
            // 移除底線、空格、連字號
            return name.Replace("_", "").Replace(" ", "").Replace("-", "");
        }

        private static string StripCommonPrefixes(string name)
        {
            // 常見的表名前綴（3~5 字元後接底線）
            string[] prefixes = { "prod", "cust", "emp", "dept", "item", "inv", "ord", "pur", "sal" };
            var lower = name.ToLowerInvariant();
            foreach (var prefix in prefixes)
            {
                if (lower.StartsWith(prefix) && lower.Length > prefix.Length)
                    return name[prefix.Length..];
            }
            return name;
        }

        #endregion

        #region GetSmartDefaultValue

        /// <inheritdoc />
        public string? GetSmartDefaultValue(EntityPropertyInfo property)
        {
            var type = property.PropertyType;
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            if (underlyingType == typeof(string)) return "";
            if (underlyingType == typeof(int) || underlyingType == typeof(long)
                || underlyingType == typeof(short) || underlyingType == typeof(byte))
                return "0";
            if (underlyingType == typeof(decimal) || underlyingType == typeof(double)
                || underlyingType == typeof(float))
                return "0";
            if (underlyingType == typeof(bool)) return "false";
            if (underlyingType == typeof(DateTime)) return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (underlyingType == typeof(DateOnly)) return DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd");
            if (underlyingType == typeof(TimeOnly)) return "00:00:00";
            if (underlyingType == typeof(Guid)) return Guid.NewGuid().ToString();

            if (underlyingType.IsEnum)
            {
                var names = Enum.GetNames(underlyingType);
                return names.Length > 0 ? names[0] : null;
            }

            return null;
        }

        #endregion

        #region GeneratePreview

        /// <inheritdoc />
        public List<ImportPreviewRow> GeneratePreview(
            string dbSetName,
            List<ColumnMapping> mappings,
            List<Dictionary<string, string?>> sourceData,
            int previewCount = 10)
        {
            var entityType = GetEntityType(dbSetName);
            if (entityType == null) return new();

            var previewRows = new List<ImportPreviewRow>();
            var rowsToPreview = sourceData.Take(previewCount);

            int rowNumber = 1;
            foreach (var sourceRow in rowsToPreview)
            {
                var previewRow = new ImportPreviewRow { RowNumber = rowNumber };

                foreach (var mapping in mappings)
                {
                    var cell = new ImportCellValue();

                    // 取得原始值
                    string? rawValue = null;
                    if (!string.IsNullOrEmpty(mapping.SourceColumnName)
                        && sourceRow.TryGetValue(mapping.SourceColumnName, out var sourceValue))
                    {
                        rawValue = sourceValue;
                    }
                    else if (!string.IsNullOrEmpty(mapping.DefaultValue))
                    {
                        rawValue = mapping.DefaultValue;
                    }

                    cell.RawValue = rawValue;

                    // 嘗試型別轉換
                    if (rawValue == null)
                    {
                        if (mapping.TargetProperty.IsRequired)
                        {
                            cell.HasError = true;
                            cell.ErrorMessage = "必填欄位無資料";
                            cell.DisplayValue = "⚠ NULL";
                        }
                        else
                        {
                            cell.DisplayValue = "(null)";
                        }
                    }
                    else
                    {
                        var (success, displayValue, errorMsg) = TryConvertValue(
                            rawValue, mapping.TargetProperty.PropertyType);
                        cell.HasError = !success;
                        cell.ErrorMessage = errorMsg;
                        cell.DisplayValue = displayValue;
                    }

                    previewRow.Cells[mapping.TargetPropertyName] = cell;
                }

                previewRows.Add(previewRow);
                rowNumber++;
            }

            return previewRows;
        }

        #endregion

        #region ValidateMappings

        /// <inheritdoc />
        public List<string> ValidateMappings(List<ColumnMapping> mappings)
        {
            return mappings
                .Where(m => m.HasProblem)
                .Select(m => m.TargetPropertyName)
                .ToList();
        }

        #endregion

        #region ExecuteImportAsync

        /// <inheritdoc />
        public async Task<ImportResult> ExecuteImportAsync(
            string dbSetName,
            List<ColumnMapping> mappings,
            List<Dictionary<string, string?>> sourceData,
            string? currentUserId,
            Action<int>? progressCallback = null)
        {
            var entityType = GetEntityType(dbSetName);
            if (entityType == null)
                return ImportResult.Failure("找不到指定的 Entity 型別", sourceData.Count);

            var stopwatch = Stopwatch.StartNew();

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var dbSetMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes);
                var genericSet = dbSetMethod!.MakeGenericMethod(entityType);
                var dbSet = genericSet.Invoke(context, null);
                var addMethod = dbSet!.GetType().GetMethod("Add");

                int totalRows = sourceData.Count;
                int batchSize = 100;
                var rowErrors = new List<ImportRowError>();

                for (int i = 0; i < totalRows; i++)
                {
                    var sourceRow = sourceData[i];
                    int rowNumber = i + 1;

                    try
                    {
                        var entity = CreateEntityFromMapping(entityType, mappings, sourceRow, currentUserId);
                        addMethod!.Invoke(dbSet, new[] { entity });

                        // 分批 SaveChanges
                        if ((i + 1) % batchSize == 0)
                        {
                            await context.SaveChangesAsync();
                            var progress = (int)((i + 1.0) / totalRows * 100);
                            progressCallback?.Invoke(progress);
                        }
                    }
                    catch (Exception ex)
                    {
                        // 任何一行失敗 → 收集錯誤並後續 Rollback
                        rowErrors.Add(new ImportRowError
                        {
                            RowNumber = rowNumber,
                            ErrorMessage = ex.InnerException?.Message ?? ex.Message
                        });

                        // 立即 Rollback
                        await transaction.RollbackAsync();
                        stopwatch.Stop();

                        return ImportResult.Failure(
                            $"第 {rowNumber} 行匯入失敗：{ex.InnerException?.Message ?? ex.Message}",
                            totalRows, rowErrors);
                    }
                }

                // 最後一批
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                progressCallback?.Invoke(100);

                stopwatch.Stop();
                return ImportResult.Success(totalRows, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                stopwatch.Stop();

                return ImportResult.Failure(
                    $"匯入過程發生錯誤：{ex.InnerException?.Message ?? ex.Message}",
                    sourceData.Count);
            }
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
        /// 根據對應定義，從來源資料行建立一個 Entity 物件
        /// </summary>
        private object CreateEntityFromMapping(
            Type entityType,
            List<ColumnMapping> mappings,
            Dictionary<string, string?> sourceRow,
            string? currentUserId)
        {
            var entity = Activator.CreateInstance(entityType)!;

            // 設定 BaseEntity 欄位
            SetBaseEntityFields(entity, entityType, currentUserId);

            // 逐欄位賦值
            foreach (var mapping in mappings)
            {
                if (!mapping.IsResolved) continue;

                string? rawValue = null;

                // 優先使用來源欄位
                if (!string.IsNullOrEmpty(mapping.SourceColumnName)
                    && sourceRow.TryGetValue(mapping.SourceColumnName, out var sourceValue))
                {
                    rawValue = sourceValue;
                }
                // 其次使用預設值
                else if (!string.IsNullOrEmpty(mapping.DefaultValue))
                {
                    rawValue = mapping.DefaultValue;
                }

                if (rawValue == null && mapping.TargetProperty.IsNullable)
                    continue; // 可空欄位且無值，保持 null

                if (rawValue == null)
                    continue; // 無值且走到這裡，跳過（會在驗證階段攔截）

                var prop = entityType.GetProperty(mapping.TargetPropertyName);
                if (prop == null || !prop.CanWrite) continue;

                var convertedValue = ConvertToTargetType(rawValue, prop.PropertyType);
                if (convertedValue != null)
                {
                    prop.SetValue(entity, convertedValue);
                }
            }

            return entity;
        }

        /// <summary>
        /// 設定 BaseEntity 共用欄位
        /// </summary>
        private void SetBaseEntityFields(object entity, Type entityType, string? currentUserId)
        {
            // CreatedAt = DateTime.UtcNow
            var createdAt = entityType.GetProperty("CreatedAt");
            createdAt?.SetValue(entity, DateTime.Now);

            // CreatedBy = 當前用戶
            var createdBy = entityType.GetProperty("CreatedBy");
            createdBy?.SetValue(entity, currentUserId);

            // Status = Active
            var status = entityType.GetProperty("Status");
            status?.SetValue(entity, EntityStatus.Active);

            // UpdatedAt = null（保持預設）
            // UpdatedBy = null（保持預設）
            // Id = 0（由 DB 自動生成）
        }

        /// <summary>
        /// 字串值轉換為目標 C# 型別
        /// </summary>
        private object? ConvertToTargetType(string rawValue, Type targetType)
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (string.IsNullOrEmpty(rawValue))
            {
                if (Nullable.GetUnderlyingType(targetType) != null)
                    return null;
                if (targetType == typeof(string))
                    return string.Empty;
                return null;
            }

            try
            {
                // String
                if (underlyingType == typeof(string))
                    return rawValue;

                // Int
                if (underlyingType == typeof(int))
                    return int.TryParse(rawValue, out var i) ? i : null;

                // Long
                if (underlyingType == typeof(long))
                    return long.TryParse(rawValue, out var l) ? l : null;

                // Short
                if (underlyingType == typeof(short))
                    return short.TryParse(rawValue, out var s) ? s : null;

                // Byte
                if (underlyingType == typeof(byte))
                    return byte.TryParse(rawValue, out var b) ? b : null;

                // Decimal
                if (underlyingType == typeof(decimal))
                    return decimal.TryParse(rawValue, out var d) ? d : null;

                // Double
                if (underlyingType == typeof(double))
                    return double.TryParse(rawValue, out var db) ? db : null;

                // Float
                if (underlyingType == typeof(float))
                    return float.TryParse(rawValue, out var f) ? f : null;

                // Bool
                if (underlyingType == typeof(bool))
                {
                    if (bool.TryParse(rawValue, out var boolVal)) return boolVal;
                    if (rawValue == "1" || rawValue.Equals("true", StringComparison.OrdinalIgnoreCase) || rawValue == "是") return true;
                    if (rawValue == "0" || rawValue.Equals("false", StringComparison.OrdinalIgnoreCase) || rawValue == "否") return false;
                    return null;
                }

                // DateTime
                if (underlyingType == typeof(DateTime))
                    return DateTime.TryParse(rawValue, out var dt) ? dt : null;

                // DateOnly
                if (underlyingType == typeof(DateOnly))
                    return DateOnly.TryParse(rawValue, out var d2) ? d2 : null;

                // TimeOnly
                if (underlyingType == typeof(TimeOnly))
                    return TimeOnly.TryParse(rawValue, out var t) ? t : null;

                // Guid
                if (underlyingType == typeof(Guid))
                    return Guid.TryParse(rawValue, out var g) ? g : null;

                // Enum
                if (underlyingType.IsEnum)
                {
                    // 嘗試以名稱解析
                    if (Enum.TryParse(underlyingType, rawValue, ignoreCase: true, out var enumVal))
                        return enumVal;

                    // 嘗試以數值解析
                    if (int.TryParse(rawValue, out var enumInt) && Enum.IsDefined(underlyingType, enumInt))
                        return Enum.ToObject(underlyingType, enumInt);

                    return null;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 嘗試轉換值並返回結果（供預覽用，不拋例外）
        /// </summary>
        private (bool Success, string DisplayValue, string? ErrorMessage) TryConvertValue(string rawValue, Type targetType)
        {
            var converted = ConvertToTargetType(rawValue, targetType);
            if (converted != null)
            {
                return (true, converted.ToString() ?? rawValue, null);
            }

            // 空字串對 string 型別是合法的
            if (string.IsNullOrEmpty(rawValue) && (targetType == typeof(string) || Nullable.GetUnderlyingType(targetType) != null))
            {
                return (true, "(空)", null);
            }

            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            var hint = GetTypeFormatHint(underlyingType);
            var errorMsg = string.IsNullOrEmpty(hint)
                ? $"無法將 \"{rawValue}\" 轉換為 {GetSimpleTypeName(underlyingType)}"
                : $"無法將 \"{rawValue}\" 轉換為 {GetSimpleTypeName(underlyingType)}（期望格式：{hint}）";
            return (false, $"⚠ {rawValue}", errorMsg);
        }

        /// <summary>
        /// 取得型別的格式提示（供錯誤訊息使用）
        /// </summary>
        private static string? GetTypeFormatHint(Type type)
        {
            if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
                return "整數，如 123、-5";
            if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
                return "數值，如 123.45";
            if (type == typeof(bool))
                return "true/false、1/0、是/否";
            if (type == typeof(DateTime))
                return "日期時間，如 2026-03-01 或 2026/03/01 14:30";
            if (type == typeof(DateOnly))
                return "日期，如 2026-03-01";
            if (type == typeof(TimeOnly))
                return "時間，如 14:30 或 14:30:00";
            if (type == typeof(Guid))
                return "GUID，如 xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
            if (type.IsEnum)
            {
                var names = Enum.GetNames(type);
                var preview = names.Length <= 5
                    ? string.Join("、", names)
                    : string.Join("、", names.Take(5)) + "…";
                return $"列舉值：{preview}";
            }
            return null;
        }

        #endregion
    }
}
