using System;
using System.Globalization;

namespace Easy
{
    public delegate void ChangeDay();

    [Update]
    [OrderIndex((int) NormalInitOrderIndexEnum.TimeManager)]
    public class TimeManager : Singleton<TimeManager>
    {
        public readonly DateTime UnixStartDate = new(1970, 1, 1, 0, 0, 0);

        public const int OneDaySeconds = 86400;
        public const int OneHourSeconds = 3600;
        public const int OneMinuteSeconds = 60;


        private long _syncTime;
        private long _recordTime = 0;
        private float _timeAdd = 0;
        private DateTimeOffset? _lastTime;

        private ChangeDay _changeDay;

        public override void BeforeRestart()
        {
            _timeAdd = 0;
            _lastTime = null;
            _syncTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            _recordTime = _syncTime;
            _changeDay = null;
        }

        public override void Init(InitCompleteCallback complete)
        {
            _timeAdd = 0;
            _syncTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            _recordTime = _syncTime;
            _changeDay = null;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            _timeAdd += deltaTime;
            if (_timeAdd > 1)
            {
                --_timeAdd;
                var curTime = DateTimeOffset.FromUnixTimeSeconds(GetCurSecTime());
                if (_lastTime == null)
                {
                    _lastTime = curTime;
                    return;
                }
                if (curTime.UtcDateTime.Date != _lastTime.Value.UtcDateTime.Date)
                {
                    _lastTime = curTime;
                    _changeDay?.Invoke();
                }
            }
        }

        public void SetSyncTime(long time)
        {
            _syncTime = time;
            _recordTime = _syncTime;
        }

        public long GetCurSecTime()
        {
            var nowTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            return _syncTime + nowTime - _recordTime;
        }

        public DateTimeOffset GetCurrentUtcTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds(GetCurSecTime());
        }

        public long GetRemainTimeSec(long expireTimeUnixSec)
        {
            long currentTimeSec = GetCurSecTime();
            long remainTimeSec = expireTimeUnixSec - currentTimeSec;
            return remainTimeSec;
        }

        /// <summary>
        /// 去除日期的时、分、秒，只保留年、月、日（即当日0点）
        /// </summary>
        public DateTime GetCurDayZeroTime(long nowSecondsUnix, long hourOffset = 0)
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
        public long GetNextDayZeroTime(long nowSecondsUnix, long hourOffset = 0)
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
        public long GetNextMonth1stDayZeroTime(long nowSecondsUnix, long hourOffset = 0)
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
        public int GetDayOfWeek(DateTime dateTime)
        {
            var day = dateTime.DayOfWeek;
            return day == DayOfWeek.Sunday ? 7 : (int)day;
        }
        
        /// <summary>
        /// 获取本周日最后一刻的秒级时间戳
        /// </summary>
        public long GetWeekEndSecondTimestampOptimized()
        {
            long currentSeconds = GetCurSecTime();
        
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
        public long GetWeekStartTimestampOptimized()
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
        public string GetRemainTimeTextByEndUnixSec(long expireTimeSec)
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
        public string GetRemainTimeTextByRemainUnixSec(long remainTimeSec)
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

        public string ConvertSeconds2StrHHMMSS(long seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            return string.Format("{0:D2}:{1:D2}:{2:D2}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
        }

        public DateTimeOffset ConvertUnixSecToDateTimeOffset(long unixSec)
        {
            var nowDateUtc = DateTimeOffset.FromUnixTimeSeconds(unixSec);
            return nowDateUtc;
        }

        public DateTime ConvertUnixSecToUtcDateTime(long unixSec)
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
        public bool IsSameDay(long unixSec1, long unixSec2)
        {
            var utcDateTime1 = ConvertUnixSecToUtcDateTime(unixSec1);
            var utcDateTime2 = ConvertUnixSecToUtcDateTime(unixSec2);
            return utcDateTime1.Date == utcDateTime2.Date;
        }

        /// <summary>
        /// 检查是否同一周
        /// </summary>
        public bool IsSameWeek(DateTime time1, DateTime time2)
        {
            // 使用ISO 8601标准：每周从周一开始
            var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
        
            int week1 = cal.GetWeekOfYear(time1, 
                System.Globalization.CalendarWeekRule.FirstFourDayWeek, 
                DayOfWeek.Monday);
            int week2 = cal.GetWeekOfYear(time2, 
                System.Globalization.CalendarWeekRule.FirstFourDayWeek, 
                DayOfWeek.Monday);
        
            return week1 == week2 && time1.Year == time2.Year;
        }
        
        /// <summary>
        /// 获取时间对应时间显示
        /// </summary>
        /// <returns></returns>
        public string GetUnixTimeString(long unixSecondsTimestamp, bool includeTimestamp = false)
        {
            var unixDateTime = ConvertUnixSecToDateTimeOffset(unixSecondsTimestamp);
            return GetUnixTimeString(unixDateTime, includeTimestamp);
        }

        /// <summary>
        /// 获取UTC时间戳对应时间显示
        /// </summary>
        public string GetUnixTimeString(DateTimeOffset dateTimeOffset, bool includeTimestamp = false)
        {
            if (includeTimestamp)
                return $"{dateTimeOffset:yyyy-MM-dd HH:mm:ss}({dateTimeOffset.ToUnixTimeSeconds()})";
            return $"{dateTimeOffset:yyyy-MM-dd HH:mm:ss}";
        }


        public void AddChangeDayListener(ChangeDay changeDay)
        {
            _changeDay += changeDay;
        }

        public void RemoveChangeDayListener(ChangeDay changeDay)
        {
            _changeDay -= changeDay;
        }
    }
}