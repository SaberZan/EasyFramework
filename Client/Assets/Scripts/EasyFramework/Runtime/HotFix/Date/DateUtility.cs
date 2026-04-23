using System;
using System.Globalization;

namespace Easy
{
    public static class DateUtility
    {
        public const string LogTag = "DateUtility";
        public readonly static DateTime UnixStartDate = new(1970, 1, 1, 0, 0, 0);

        public const int OneDaySeconds = 86400;
        public const int OneHourSeconds = 3600;
        public const int OneMinuteSeconds = 60;


        public static Func<long> getCurTimeCallback;

        public static long GetCurTime()
        {
            return getCurTimeCallback?.Invoke()?? DateTimeOffset.Now.ToUnixTimeSeconds();
        }

        public static DateTimeOffset GetCurrentUtcTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds(GetCurTime());
        }

        public static long GetRemainTimeSec(long expireTimeUnixSec)
        {
            long currentTimeSec = GetCurTime();
            long remainTimeSec = expireTimeUnixSec - currentTimeSec;
            return remainTimeSec;
        }

        /// <summary>
        /// 去除日期的时、分、秒，只保留年、月、日（即当日0点）
        /// </summary>
        public static DateTime GetCurDayZeroTime(long nowSecondsUnix, long hourOffset = 0)
        {
            var nowDateUtc = DateTimeOffset.FromUnixTimeSeconds(nowSecondsUnix).UtcDateTime;
            var nowDateForRefresh = nowDateUtc.AddHours(hourOffset);
            var dateYMD = nowDateForRefresh.AddSeconds(-nowDateForRefresh.Second).AddMinutes(-nowDateForRefresh.Minute).AddHours(-nowDateForRefresh.Hour);
            return dateYMD;
        }

        /// <summary>
        /// 获取明天0点
        /// </summary>
        /// <param name="nowSecondsUnix">现在Unix时间戳</param>
        /// <param name="hourOffset">偏移 UTC 0, 北京 8, 日本 9</param>
        /// <returns>明天0点的时间戳</returns>
        public static long GetNextDayZeroTime(long nowSecondsUnix, long hourOffset = 0)
        {
            var nowDateUtc = DateTimeOffset.FromUnixTimeSeconds(nowSecondsUnix).UtcDateTime;

            var nowDateForRefresh = nowDateUtc.AddHours(hourOffset);

            // 明天0点
            var nextRefreshDate = nowDateForRefresh.AddSeconds(-nowDateForRefresh.Second).AddMinutes(-nowDateForRefresh.Minute)
                                    .AddHours(-nowDateForRefresh.Hour).AddDays(1);

            var nextRefreshDateUtc = nextRefreshDate.AddHours(-hourOffset);

            long nextRefreshTime = (long)(nextRefreshDateUtc - UnixStartDate).TotalSeconds;


            return nextRefreshTime;
        }

        /// <summary>
        /// 获取下个月1号0点
        /// </summary>
        /// <param name="nowSecondsUnix">现在Unix时间戳</param>
        /// <param name="hourOffset">偏移 UTC 0, 北京 8, 日本 9</param>
        /// <returns>下个月1号0点的时间戳</returns>
        public static long GetNextMonth1stDayZeroTime(long nowSecondsUnix, long hourOffset = 0)
        {
            var nowDateUtc = DateTimeOffset.FromUnixTimeSeconds(nowSecondsUnix).UtcDateTime;

            var nowDateForRefresh = nowDateUtc.AddHours(hourOffset);

            // 下个月1号0点
            var nextRefreshDate = nowDateForRefresh.AddSeconds(-nowDateForRefresh.Second).AddMinutes(-nowDateForRefresh.Minute)
                                    .AddHours(-nowDateForRefresh.Hour).AddDays(1 - nowDateForRefresh.Day).AddMonths(1);

            var nextRefreshDateUtc = nextRefreshDate.AddHours(-hourOffset);

            long nextRefreshTime = (long)(nextRefreshDateUtc - UnixStartDate).TotalSeconds;

            return nextRefreshTime;
        }

        /// <summary>
        /// 获取是周几
        /// </summary>
        public static int GetDayOfWeek(DateTime dateTime)
        {
            var day = dateTime.DayOfWeek;
            return day == DayOfWeek.Sunday ? 7 : (int)day;
        }
        
        /// <summary>
        /// 获取本周日最后一刻的秒级时间戳
        /// </summary>
        public static long GetWeekEndSecondTimestampOptimized()
        {
            long currentSeconds = GetCurTime();
        
            // 计算当前时间在周中的位置
            DateTime currentTime = DateTimeOffset.FromUnixTimeSeconds(currentSeconds).UtcDateTime;
            int daysUntilSunday = 7 - GetDayOfWeek(currentTime);
        
            // 获取本周日的日期
            DateTime sundayDate = currentTime.AddDays(daysUntilSunday).Date;
        
            // 计算本周日23:59:59的时间戳
            long sundayEndSeconds = new DateTimeOffset(sundayDate, TimeSpan.Zero).ToUnixTimeSeconds() + 
                                    (23 * OneHourSeconds) + 
                                    (59 * OneMinuteSeconds) + 
                                    59;
        
            return sundayEndSeconds;
        }
        
        /// <summary>
        /// 获取本周开始秒级时间戳
        /// </summary>
        public static long GetWeekStartTimestampOptimized()
        {
            long mondayTimestamp = new DateTimeOffset(
                DateTime.Today.AddDays(-(((int)DateTime.Today.DayOfWeek - 1 + 7) % 7))
            ).ToUnixTimeSeconds();
        
            return mondayTimestamp;
        }

        /// <summary>
        /// 结束时间UnixSec转剩余时间Text
        /// </summary>
        /// <param name="expireTimeSec"></param>
        /// <returns></returns>
        public static string GetRemainTimeTextByEndUnixSec(long expireTimeSec)
        {
            if (expireTimeSec < 0)
            {
                return "ERR_TIME_SEC_BELOW_ZERO";
            }
            return GetRemainTimeTextByRemainUnixSec(GetRemainTimeSec(expireTimeSec));
        }

        /// <summary>
        /// 剩余时间Sec转剩余时间Text
        /// </summary>
        /// <param name="remainTimeSec"></param>
        /// <returns></returns>
        public static string GetRemainTimeTextByRemainUnixSec(long remainTimeSec)
        {
            if (remainTimeSec < 0)
            {
                return "ERR_TIME_SEC_BELOW_ZERO";
            }
            TimeSpan timeSpan = TimeSpan.FromSeconds(remainTimeSec);
            if (timeSpan.TotalDays >= 1)
            {
                return $"{timeSpan.Days.ToString("D2")}d{timeSpan.Hours.ToString("D2")}h";
            }
            else if (timeSpan.TotalHours >= 1)
            {
                return $"{timeSpan.Hours.ToString("D2")}h{timeSpan.Minutes.ToString("D2")}m";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                return $"{timeSpan.Minutes.ToString("D2")}m{timeSpan.Seconds.ToString("D2")}s";
            }
            else if (timeSpan.Seconds >= 1)
            {
                return $"{timeSpan.Seconds.ToString("D2")}s";
            }
            else
            {
                return "0S";    //0秒
            }
        }

        public static string ConvertSeconds2StrHHMMSS(long seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            return string.Format("{0:D2}:{1:D2}:{2:D2}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
        }

        public static DateTimeOffset ConvertUnixSecToDateTimeOffset(long unixSec)
        {
            var nowDateUtc = DateTimeOffset.FromUnixTimeSeconds(unixSec);
            return nowDateUtc;
        }

        public static DateTime ConvertUnixSecToUtcDateTime(long unixSec)
        {
            var dateTimeOffset = ConvertUnixSecToDateTimeOffset(unixSec);
            return dateTimeOffset.UtcDateTime;
        }

        /// <summary>
        /// 是否同一天
        /// </summary>
        /// <param name="unixSec1"></param>
        /// <param name="unixSec2"></param>
        /// <returns></returns>
        public static bool IsSameDay(long unixSec1, long unixSec2)
        {
            var utcDateTime1 = ConvertUnixSecToUtcDateTime(unixSec1);
            var utcDateTime2 = ConvertUnixSecToUtcDateTime(unixSec2);
            return utcDateTime1.Date == utcDateTime2.Date;
        }

        /// <summary>
        /// 获取时间对应时间显示
        /// </summary>
        /// <returns></returns>
        public static string GetUnixTimeString(long unixSecondsTimestamp, bool includeTimestamp = false)
        {
            var unixDateTime = ConvertUnixSecToDateTimeOffset(unixSecondsTimestamp);
            return GetUnixTimeString(unixDateTime, includeTimestamp);
        }

        /// <summary>
        /// 获取UTC时间戳对应时间显示
        /// </summary>
        public static string GetUnixTimeString(DateTimeOffset dateTimeOffset, bool includeTimestamp = false)
        {
            if (includeTimestamp)
                return $"{dateTimeOffset:yyyy-MM-dd HH:mm:ss}({dateTimeOffset.ToUnixTimeSeconds()})";
            return $"{dateTimeOffset:yyyy-MM-dd HH:mm:ss}";
        }
    }
}