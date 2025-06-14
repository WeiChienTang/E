using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Services.GenericManagementService;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Services;

public class ColorService : GenericManagementService<Color>, IColorService
{
    public ColorService(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// 覆寫取得所有資料方法，加入排序
    /// </summary>
    public override async Task<List<Color>> GetAllAsync()
    {
        return await _dbSet
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <summary>
    /// 覆寫搜尋方法，實作顏色特定的搜尋邏輯
    /// </summary>
    public override async Task<List<Color>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        return await _dbSet
            .Where(c => !c.IsDeleted &&
                       (c.Name.Contains(searchTerm) ||
                        c.Code.Contains(searchTerm) ||
                        (c.Description != null && c.Description.Contains(searchTerm)) ||
                        (c.HexCode != null && c.HexCode.Contains(searchTerm))))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }    /// <summary>
    /// 覆寫驗證方法，添加顏色特定的驗證規則
    /// </summary>
    public override async Task<ServiceResult> ValidateAsync(Color entity)
    {
        // 基本驗證
        if (string.IsNullOrWhiteSpace(entity.Name))
            return ServiceResult.Failure("顏色名稱為必填");

        if (string.IsNullOrWhiteSpace(entity.Code))
            return ServiceResult.Failure("顏色代碼為必填");

        // 檢查代碼是否重複
        if (await IsCodeExistsAsync(entity.Code, entity.Id))
            return ServiceResult.Failure("顏色代碼已存在");

        // 檢查名稱是否重複
        if (await IsNameExistsAsync(entity.Name, entity.Id))
            return ServiceResult.Failure("顏色名稱已存在");

        // 檢查十六進位色碼是否重複（如果有提供）
        if (!string.IsNullOrWhiteSpace(entity.HexCode) && 
            await IsHexCodeExistsAsync(entity.HexCode, entity.Id))
            return ServiceResult.Failure("十六進位色碼已存在");

        return ServiceResult.Success();
    }

    /// <summary>
    /// 覆寫名稱存在檢查
    /// </summary>
    public override async Task<bool> IsNameExistsAsync(string name, int? excludeId = null)
    {
        var query = _dbSet.Where(c => c.Name == name && !c.IsDeleted);

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    /// <summary>
    /// 檢查顏色代碼是否已存在
    /// </summary>
    public async Task<bool> IsCodeExistsAsync(string code, int? excludeId = null)
    {
        var query = _dbSet.Where(c => c.Code == code && !c.IsDeleted);

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    /// <summary>
    /// 根據代碼取得顏色資料
    /// </summary>
    public async Task<Color?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .Where(c => c.Code == code && !c.IsDeleted)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// 根據十六進位色碼取得顏色資料
    /// </summary>
    public async Task<Color?> GetByHexCodeAsync(string hexCode)
    {
        return await _dbSet
            .Where(c => c.HexCode == hexCode && !c.IsDeleted)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// 檢查十六進位色碼是否已存在
    /// </summary>
    public async Task<bool> IsHexCodeExistsAsync(string hexCode, int? excludeId = null)
    {
        var query = _dbSet.Where(c => c.HexCode == hexCode && !c.IsDeleted);

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync();
    }
}
