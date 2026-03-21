namespace ERPCore2.Models.Enums
{
    /// <summary>
    /// 潛在客戶開發階段
    /// </summary>
    public enum LeadStage
    {
        /// <summary>冷接觸（初步接觸，尚未確認意願）</summary>
        Cold = 1,
        /// <summary>有意願（確認有興趣，進入洽談）</summary>
        Interested = 2,
        /// <summary>報價中（已發送報價，等待決策）</summary>
        Quoting = 3,
        /// <summary>成交（已轉為正式客戶）</summary>
        Won = 4,
        /// <summary>流失（洽談終止，暫不繼續）</summary>
        Lost = 5
    }

    /// <summary>
    /// 潛在客戶來源
    /// </summary>
    public enum LeadSource
    {
        /// <summary>業務主動開發</summary>
        BusinessDevelopment = 1,
        /// <summary>客戶或合作夥伴推薦</summary>
        Referral = 2,
        /// <summary>展覽 / 展示會</summary>
        Exhibition = 3,
        /// <summary>網路 / 社群媒體</summary>
        Internet = 4,
        /// <summary>其他來源</summary>
        Other = 5
    }
}
