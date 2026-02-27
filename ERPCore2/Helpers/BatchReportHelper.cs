using ERPCore2.Models.Reports;
using ERPCore2.Services.Reports.Interfaces;
using ERPCore2.Data.Entities;
using Microsoft.Extensions.Logging;
using System.Runtime.Versioning;

namespace ERPCore2.Helpers;

/// <summary>
/// 批次報表產生 Helper
/// 提供通用的批次預覽圖片產生邏輯，避免各報表服務重複實作
/// </summary>
public static class BatchReportHelper
{
    /// <summary>
    /// 批次渲染報表為圖片（用於批次預覽）- 支援紙張設定
    /// </summary>
    /// <typeparam name="TEntity">實體類型</typeparam>
    /// <param name="entities">要處理的實體清單</param>
    /// <param name="getEntityId">取得實體 ID 的委派</param>
    /// <param name="generateReportAsync">產生單筆報表的委派（接受 entityId 和 PaperSetting）</param>
    /// <param name="formattedPrintService">格式化列印服務</param>
    /// <param name="entityDisplayName">實體顯示名稱（用於訊息，如「採購單」）</param>
    /// <param name="paperSetting">紙張設定（可選，null 時使用預設）</param>
    /// <param name="criteriaMessage">篩選條件訊息（查無資料時顯示）</param>
    /// <param name="logger">日誌記錄器（可選）</param>
    /// <returns>批次預覽結果</returns>
    [SupportedOSPlatform("windows6.1")]
    public static async Task<BatchPreviewResult> RenderBatchToImagesAsync<TEntity>(
        IEnumerable<TEntity> entities,
        Func<TEntity, int> getEntityId,
        Func<int, PaperSetting?, Task<FormattedDocument>> generateReportAsync,
        IFormattedPrintService formattedPrintService,
        string entityDisplayName,
        PaperSetting? paperSetting = null,
        string? criteriaMessage = null,
        ILogger? logger = null)
    {
        try
        {
            var entityList = entities.ToList();
            
            if (!entityList.Any())
            {
                var message = string.IsNullOrEmpty(criteriaMessage)
                    ? $"無符合條件的{entityDisplayName}"
                    : $"無符合條件的{entityDisplayName}\n篩選條件：{criteriaMessage}";
                return BatchPreviewResult.Failure(message);
            }

            var allImages = new List<byte[]>();
            FormattedDocument? mergedDocument = null;
            int successCount = 0;

            // 逐一產生報表並合併
            foreach (var entity in entityList)
            {
                var entityId = getEntityId(entity);
                try
                {
                    var document = await generateReportAsync(entityId, paperSetting);
                    
                    // 根據是否有紙張設定來選擇渲染方法
                    var images = paperSetting != null 
                        ? formattedPrintService.RenderToImages(document, paperSetting)
                        : formattedPrintService.RenderToImages(document);
                    allImages.AddRange(images);

                    // 合併 FormattedDocument（用於列印）
                    if (mergedDocument == null)
                    {
                        mergedDocument = document;
                    }
                    else
                    {
                        // 加入分頁標記並合併內容
                        mergedDocument.AddPageBreak();
                        mergedDocument.MergeFrom(document);
                    }
                    
                    successCount++;
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "產生{EntityName} {EntityId} 預覽時發生錯誤", entityDisplayName, entityId);
                    // 繼續處理下一筆，不中斷整個批次
                }
            }

            if (!allImages.Any())
            {
                return BatchPreviewResult.Failure($"所有{entityDisplayName}的預覽產生都失敗");
            }
            
            return BatchPreviewResult.Success(allImages, mergedDocument, successCount);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "批次產生預覽時發生錯誤");
            return BatchPreviewResult.Failure($"批次產生預覽時發生錯誤: {ex.Message}");
        }
    }

    /// <summary>
    /// 批次渲染報表為圖片（用於批次預覽）- 向下相容舊版本（不帶紙張設定）
    /// </summary>
    [SupportedOSPlatform("windows6.1")]
    public static Task<BatchPreviewResult> RenderBatchToImagesAsync<TEntity>(
        IEnumerable<TEntity> entities,
        Func<TEntity, int> getEntityId,
        Func<int, Task<FormattedDocument>> generateReportAsync,
        IFormattedPrintService formattedPrintService,
        string entityDisplayName,
        string? criteriaMessage = null,
        ILogger? logger = null)
    {
        // 包裝成新版本的委派格式
        return RenderBatchToImagesAsync(
            entities,
            getEntityId,
            (id, _) => generateReportAsync(id),
            formattedPrintService,
            entityDisplayName,
            null, // paperSetting
            criteriaMessage,
            logger);
    }

    /// <summary>
    /// 批次渲染報表為圖片（簡化版，實體須有 Id 屬性）- 支援紙張設定
    /// </summary>
    [SupportedOSPlatform("windows6.1")]
    public static Task<BatchPreviewResult> RenderBatchToImagesAsync<TEntity>(
        IEnumerable<TEntity> entities,
        Func<int, PaperSetting?, Task<FormattedDocument>> generateReportAsync,
        IFormattedPrintService formattedPrintService,
        string entityDisplayName,
        PaperSetting? paperSetting = null,
        string? criteriaMessage = null,
        ILogger? logger = null) where TEntity : Data.BaseEntity
    {
        return RenderBatchToImagesAsync(
            entities,
            e => e.Id,
            generateReportAsync,
            formattedPrintService,
            entityDisplayName,
            paperSetting,
            criteriaMessage,
            logger);
    }

    /// <summary>
    /// 批次渲染報表為圖片（簡化版，實體須有 Id 屬性）- 向下相容舊版本
    /// </summary>
    [SupportedOSPlatform("windows6.1")]
    public static Task<BatchPreviewResult> RenderBatchToImagesAsync<TEntity>(
        IEnumerable<TEntity> entities,
        Func<int, Task<FormattedDocument>> generateReportAsync,
        IFormattedPrintService formattedPrintService,
        string entityDisplayName,
        string? criteriaMessage = null,
        ILogger? logger = null) where TEntity : Data.BaseEntity
    {
        return RenderBatchToImagesAsync(
            entities,
            e => e.Id,
            (id, _) => generateReportAsync(id),
            formattedPrintService,
            entityDisplayName,
            null, // paperSetting
            criteriaMessage,
            logger);
    }
}
