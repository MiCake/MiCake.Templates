namespace MiCakeTemplate.Util.Time
{
    /// <summary>
    /// A class to represent a range of date time.
    /// </summary>
    public class DateTimeRange
    {
        private readonly DateTime _startTime;
        /// <summary>
        /// The start time of the current time range.
        /// </summary>
        public DateTime StartTime
        {
            get { return _startTime; }
        }

        /// <summary>
        /// Then end time of the current time range.
        /// </summary>
        private readonly DateTime _endTime;
        public DateTime EndTime
        {
            get { return _endTime; }
        }

        public static DateTimeRange? Min
        {
            get { return new DateTimeRange(DateTime.MinValue, DateTime.MinValue); }
        }

        public static DateTimeRange? Max
        {
            get { return new DateTimeRange(DateTime.MinValue, DateTime.MaxValue); }
        }

        /// <summary>
        /// The total days of the current time range.
        /// </summary>
        public double Days => (EndTime - StartTime).TotalDays;

        #region Ctor

        /// <summary>
        /// 时间段【保证输入的两个时间点大小可以构成正确的的时间段】
        /// </summary>
        /// <param name="startTime">时间段开始时间</param>
        /// <param name="endTime">时间段结束时间</param>
        /// <param name="isTransform">true:如果大小相反则自动交换  false:大小相反则抛出异常</param>
        public DateTimeRange(DateTime startTime, DateTime endTime, bool isTransform)
        {
            var tempStartTime = startTime;
            var tempEndTime = endTime;

            //是否顺序错误
            bool isOrderMiss = false;
            if (tempStartTime.CompareTo(tempEndTime) > 0)
            {
                isOrderMiss = true;
                tempStartTime = endTime;
                tempEndTime = startTime;
            }

            if (isOrderMiss && !isTransform)
                throw new Exception("DateTimeRange:StartTime is more than EndTime!");

            _startTime = tempStartTime;
            _endTime = tempEndTime;

        }

        /// <summary>
        /// 时间段
        /// </summary>
        /// <param name="startTime">时间段开始时间</param>
        /// <param name="endTime">时间段结束时间</param>
        public DateTimeRange(DateTime startTime, DateTime endTime)
            : this(startTime, endTime, true)
        {

        }

        #endregion

        /// <summary>
        /// Get a new <see cref="DateTimeRange"/> that is the overlap of two time ranges.
        /// </summary>
        /// <param name="timeRange">A time range which want to compare with current time range.</param>
        /// <param name="isIncludeCriticalPoint">Whether critical point is included</param>
        /// <returns></returns>
        public DateTimeRange? GetOverlapRange(DateTimeRange timeRange, bool isIncludeCriticalPoint = false)
        {
            DateTimeRange? reslut = null;

            DateTime bStartTime = _startTime;
            DateTime oEndTime = _endTime;
            DateTime sStartTime = timeRange.StartTime;
            DateTime eEndTime = timeRange.EndTime;

            bool isOverlap = isIncludeCriticalPoint ?
                            bStartTime <= eEndTime && oEndTime >= sStartTime :
                            bStartTime < eEndTime && oEndTime > sStartTime;

            if (isOverlap)
            {
                DateTime sTime = sStartTime >= bStartTime ? sStartTime : bStartTime;
                DateTime eTime = oEndTime >= eEndTime ? eEndTime : oEndTime;

                reslut = new DateTimeRange(sTime, eTime);
            }
            return reslut;
        }

        /// <summary>
        /// Get total hours of the current time range.
        /// </summary>
        public double GetRangeHours()
        {
            return (_endTime - _startTime).TotalHours;
        }

        /// <summary>
        /// Get total minutes of the current time range.
        /// </summary>
        public double GetRangeMinutes()
        {
            return (_endTime - _startTime).TotalMinutes;
        }

        /// <summary>
        /// Check whether the current time point is in this time range.
        /// </summary>
        public bool IsTimeInRange(DateTime iTime)
        {
            return _startTime.CompareTo(iTime) <= 0 && _endTime.CompareTo(iTime) >= 0;
        }

        /// <summary>
        ///  Check whether the time range is fully included by the current time range.
        /// </summary>
        /// <param name="compareRange"></param>
        /// <returns></returns>
        public bool IsCoverTimeRange(DateTimeRange compareRange)
        {
            return StartTime <= compareRange.StartTime && EndTime >= compareRange.EndTime;
        }
    }
}
