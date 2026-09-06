using System;
using System.Collections.Generic;

namespace Starstate.Core
{
    /// <summary>日历与时间工具：日期/星期/节假日/周与月边界（纯 C#，System.DateTime）。</summary>
    public static class GameClock
    {
        // 序章周一（入职前最后一周）；入职日 2026-09-01（周二）
        public static readonly DateTime GameStart = new DateTime(2026, 8, 24);
        public static readonly DateTime ProbationStart = new DateTime(2026, 9, 1);
        public const string ProbationStartIso = "2026-09-01";

        private static readonly string[] WeekdayCn = { "周一", "周二", "周三", "周四", "周五", "周六", "周日" };

        // 节假日口径：传统六节＋开国纪念日＋宪法日（世界观冻结）。
        // 2026-2027 为真实历法；2028 年起的具体日期为【占位推演】（docs/OPEN_QUESTIONS.md Q2-12）。
        private static readonly Dictionary<string, string> Holidays = new Dictionary<string, string>
        {
            // 2026
            { "2026-09-25", "中秋节" },
            { "2026-10-08", "开国纪念日" },   // 【占位】
            { "2026-10-18", "重阳节" },
            { "2027-01-01", "元旦" },         // 【占位】
            // 2027
            { "2027-02-05", "除夕" },
            { "2027-02-06", "春节" },
            { "2027-02-07", "春节" },
            { "2027-03-11", "宪法日" },       // 【占位】1986年3月宪法颁布纪念日
            { "2027-04-05", "清明节" },
            { "2027-06-09", "端午节" },       // 【占位】
            { "2027-09-15", "中秋节" },       // 【占位】
            { "2027-10-08", "开国纪念日" },
            // 2028 【占位】
            { "2028-01-01", "元旦" },
            { "2028-01-25", "除夕" },
            { "2028-01-26", "春节" },
            { "2028-01-27", "春节" },
            { "2028-03-11", "宪法日" },
            { "2028-04-04", "清明节" },
            { "2028-06-22", "端午节" },
            { "2028-09-22", "中秋节" },
            { "2028-10-08", "开国纪念日" },
            // 2029 【占位】
            { "2029-01-01", "元旦" },
            { "2029-02-12", "除夕" },
            { "2029-02-13", "春节" },
            { "2029-02-14", "春节" },
            { "2029-03-11", "宪法日" },
            { "2029-04-04", "清明节" },
            { "2029-06-10", "端午节" },
            { "2029-09-21", "中秋节" },
            { "2029-10-08", "开国纪念日" },
            // 2030 【占位】
            { "2030-01-01", "元旦" },
            { "2030-02-02", "除夕" },
            { "2030-02-03", "春节" },
            { "2030-02-04", "春节" },
            { "2030-03-11", "宪法日" },
            { "2030-04-05", "清明节" },
            { "2030-06-15", "端午节" },
            { "2030-09-11", "中秋节" },
            { "2030-10-08", "开国纪念日" },
            // 2031 【占位】
            { "2031-01-01", "元旦" },
            { "2031-01-22", "除夕" },
            { "2031-01-23", "春节" },
            { "2031-01-24", "春节" },
            { "2031-03-11", "宪法日" },
            { "2031-04-05", "清明节" },
            { "2031-06-04", "端午节" },
            { "2031-09-30", "中秋节" },
            { "2031-10-08", "开国纪念日" },
            // 2032 【占位】
            { "2032-01-01", "元旦" },
            { "2032-02-10", "除夕" },
            { "2032-02-11", "春节" },
            { "2032-02-12", "春节" },
            { "2032-03-11", "宪法日" },
            { "2032-04-04", "清明节" },
            { "2032-06-23", "端午节" },
            { "2032-09-19", "中秋节" },
            { "2032-10-08", "开国纪念日" },
            // 2033 【占位】
            { "2033-01-01", "元旦" },
            { "2033-01-30", "除夕" },
            { "2033-01-31", "春节" },
            { "2033-02-01", "春节" },
            { "2033-03-11", "宪法日" },
            { "2033-04-04", "清明节" },
            { "2033-06-12", "端午节" },
            { "2033-09-08", "中秋节" },
            { "2033-10-08", "开国纪念日" },
            // 2034 【占位】
            { "2034-01-01", "元旦" },
            { "2034-02-18", "除夕" },
            { "2034-02-19", "春节" },
            { "2034-02-20", "春节" },
            { "2034-03-11", "宪法日" },
            { "2034-04-05", "清明节" },
            { "2034-06-01", "端午节" },
            { "2034-09-26", "中秋节" },
            { "2034-10-08", "开国纪念日" },
            // 2035 【占位】
            { "2035-01-01", "元旦" },
            { "2035-02-07", "除夕" },
            { "2035-02-08", "春节" },
            { "2035-02-09", "春节" },
            { "2035-03-11", "宪法日" },
            { "2035-04-05", "清明节" },
            { "2035-05-21", "端午节" },
            { "2035-09-16", "中秋节" },
            { "2035-10-08", "开国纪念日" },
            // 2036 【占位】
            { "2036-01-01", "元旦" },
            { "2036-01-27", "除夕" },
            { "2036-01-28", "春节" },
            { "2036-01-29", "春节" },
            { "2036-03-11", "宪法日" },
            { "2036-04-04", "清明节" },
            { "2036-06-09", "端午节" },
            { "2036-10-08", "开国纪念日" },
        };

        public static DateTime Parse(string iso) => DateTime.ParseExact(iso, "yyyy-MM-dd", null);
        public static string Iso(DateTime d) => d.ToString("yyyy-MM-dd");
        public static DateTime AddDays(DateTime d, int n) => d.AddDays(n);

        /// <summary>该日期所在周的周一。</summary>
        public static DateTime MondayOf(DateTime d) => AddDays(d.Date, -((int)d.DayOfWeek == 0 ? 6 : (int)d.DayOfWeek - 1));

        public static string Weekday(DateTime d) => WeekdayCn[(int)d.DayOfWeek == 0 ? 6 : (int)d.DayOfWeek - 1];

        public static string Fmt(DateTime d) => $"{d.Year}年{d.Month}月{d.Day}日";
        public static string FmtFull(DateTime d) => $"{Fmt(d)} {Weekday(d)}";

        public static string HolidayName(DateTime d)
        {
            string s;
            return Holidays.TryGetValue(Iso(d), out s) ? s : null;
        }

        public static bool IsWeekend(DateTime d) => d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday;

        public static bool IsWorkday(DateTime d) => !IsWeekend(d) && !Holidays.ContainsKey(Iso(d));

        public static DateTime NextWorkday(DateTime d)
        {
            var x = AddDays(d, 1);
            while (!IsWorkday(x)) x = AddDays(x, 1);
            return x;
        }

        public static bool IsLastDayOfMonth(DateTime d)
        {
            var nxt = AddDays(d, 1);
            return nxt.Month != d.Month;
        }

        /// <summary>自序章周起第几周，从 1 开始（以周一为周首）。</summary>
        public static int WeekIndex(DateTime d)
        {
            return (int)((MondayOf(d) - MondayOf(GameStart)).TotalDays / 7) + 1;
        }

        public static string MonthKey(DateTime d) => $"{d.Year}-{d.Month:D2}";
    }
}
