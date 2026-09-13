using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>
    /// 模板总表。**顺序即 seq 取模顺序**：新增模板只允许往后追加（或进 Y2 末位），
    /// 绝不插入/重排——否则同一 id 在不同版本会重建出不同卷宗，读档即错位。
    /// </summary>
    internal static class DossierTemplates
    {
        public static DossierTemplate[] All()
        {
            var list = new List<DossierTemplate>(DossierTemplatesY1.All);
            list.AddRange(DossierTemplatesY2.All);
            return list.ToArray();
        }

        /// <summary>抽查用：某模板的来文形态与选项数（内容审计/测试用）。</summary>
        public static int Count { get { return DossierTemplatesY1.All.Length + DossierTemplatesY2.All.Length; } }
    }
}
