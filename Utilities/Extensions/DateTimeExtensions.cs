using System;
using System.Globalization;
namespace ASTITransportation.Extensions
{
    /// <summary>
    /// 	Extension methods for the DateTimeOffset data type.
    /// </summary>
    public static class DateTimeExtensions
    {
        const int EveningEnds = 2;
        const int MorningEnds = 12;
        const int AfternoonEnds = 6;
        static readonly DateTime Date1970 = new DateTime(1970, 1, 1);

        ///<summary>
        ///	Return System UTC Offset
        ///</summary>
        public static double UtcOffset
        {
            get { return DateTime.Now.Subtract(DateTime.UtcNow).TotalHours; }
        }

        /// <summary>
        /// 	Calculates the age based on today.
        /// </summary>
        /// <param name = "dateOfBirth">The date of birth.</param>
        /// <returns>The calculated age.</returns>
        public static int CalculateAge(this DateTime dateOfBirth)
        {
            return CalculateAge(dateOfBirth, DateTime.Today);
        }

        /// <summary>
        /// 	Calculates the age based on a passed reference date.
        /// </summary>
        /// <param name = "dateOfBirth">The date of birth.</param>
        /// <param name = "referenceDate">The reference date to calculate on.</param>
        /// <returns>The calculated age.</returns>
        public static int CalculateAge(this DateTime dateOfBirth, DateTime referenceDate)
        {
            var years = referenceDate.Year - dateOfBirth.Year;
            if (referenceDate.Month < dateOfBirth.Month || (referenceDate.Month == dateOfBirth.Month && referenceDate.Day < dateOfBirth.Day))
                --years;
            return years;
        }

        /// <summary>
        /// 	Returns the number of days in the month of the provided date.
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <returns>The number of days.</returns>
        public static int GetCountDaysOfMonth(this DateTime date)
        {
            var nextMonth = date.AddMonths(1);
            return new DateTime(nextMonth.Year, nextMonth.Month, 1).AddDays(-1).Day;
        }

        /// <summary>
        /// 	Returns the first day of the month of the provided date.
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <returns>The first day of the month</returns>
        public static DateTime GetFirstDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        /// <summary>
        /// 	Returns the first day of the month of the provided date.
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <param name = "dayOfWeek">The desired day of week.</param>
        /// <returns>The first day of the month</returns>
        public static DateTime GetFirstDayOfMonth(this DateTime date, DayOfWeek dayOfWeek)
        {
            var dt = date.GetFirstDayOfMonth();
            while (dt.DayOfWeek != dayOfWeek)
                dt = dt.AddDays(1);
            return dt;
        }

        /// <summary>
        /// 	Returns the last day of the month of the provided date.
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <returns>The last day of the month.</returns>
        public static DateTime GetLastDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, GetCountDaysOfMonth(date));
        }

        /// <summary>
        /// 	Returns the last day of the month of the provided date.
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <param name = "dayOfWeek">The desired day of week.</param>
        /// <returns>The date time</returns>
        public static DateTime GetLastDayOfMonth(this DateTime date, DayOfWeek dayOfWeek)
        {
            var dt = date.GetLastDayOfMonth();
            while (dt.DayOfWeek != dayOfWeek)
                dt = dt.AddDays(-1);
            return dt;
        }

        /// <summary>
        /// 	Indicates whether the date is today.
        /// </summary>
        /// <param name = "dt">The date.</param>
        /// <returns>
        /// 	<c>true</c> if the specified date is today; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsToday(this DateTime dt)
        {
            return (dt.Date == DateTime.Today);
        }

        /// <summary>
        /// 	Sets the time on the specified DateTime value.
        /// </summary>
        /// <param name = "date">The base date.</param>
        /// <param name = "hours">The hours to be set.</param>
        /// <param name = "minutes">The minutes to be set.</param>
        /// <param name = "seconds">The seconds to be set.</param>
        /// <returns>The DateTime including the new time value</returns>
        public static DateTime SetTime(this DateTime date, int hours, int minutes, int seconds)
        {
            return date.SetTime(new TimeSpan(hours, minutes, seconds));
        }

        /// <summary>
        /// 	Sets the time on the specified DateTime value.
        /// </summary>
        /// <param name = "date">The base date.</param>
        /// <param name = "time">The TimeSpan to be applied.</param>
        /// <returns>
        /// 	The DateTime including the new time value
        /// </returns>
        public static DateTime SetTime(this DateTime date, TimeSpan time)
        {
            return date.Date.Add(time);
        }

        /// <summary>
        /// 	Converts a DateTime into a DateTimeOffset using the local system time zone.
        /// </summary>
        /// <param name = "localDateTime">The local DateTime.</param>
        /// <returns>The converted DateTimeOffset</returns>
        public static DateTimeOffset ToDateTimeOffset(this DateTime localDateTime)
        {
            return localDateTime.ToDateTimeOffset(null);
        }

        /// <summary>
        /// 	Converts a DateTime into a DateTimeOffset using the specified time zone.
        /// </summary>
        /// <param name = "localDateTime">The local DateTime.</param>
        /// <param name = "localTimeZone">The local time zone.</param>
        /// <returns>The converted DateTimeOffset</returns>
        public static DateTimeOffset ToDateTimeOffset(this DateTime localDateTime, TimeZoneInfo localTimeZone)
        {
            localTimeZone = (localTimeZone ?? TimeZoneInfo.Local);

            if (localDateTime.Kind != DateTimeKind.Unspecified)
                localDateTime = new DateTime(localDateTime.Ticks, DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeToUtc(localDateTime, localTimeZone);
        }

        /// <summary>
        /// 	Gets the first day of the week using the current culture.
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <returns>The first day of the week</returns>
        public static DateTime GetFirstDayOfWeek(this DateTime date)
        {
            return date.GetFirstDayOfWeek(null);
        }

        /// <summary>
        /// 	Gets the first day of the week using the specified culture.
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <param name = "cultureInfo">The culture to determine the first weekday of a week.</param>
        /// <returns>The first day of the week</returns>
        public static DateTime GetFirstDayOfWeek(this DateTime date, CultureInfo cultureInfo)
        {
            cultureInfo = (cultureInfo ?? CultureInfo.CurrentCulture);

            var firstDayOfWeek = cultureInfo.DateTimeFormat.FirstDayOfWeek;
            while (date.DayOfWeek != firstDayOfWeek)
                date = date.AddDays(-1);

            return date;
        }

        /// <summary>
        /// 	Gets the last day of the week using the current culture.
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <returns>The first day of the week</returns>
        public static DateTime GetLastDayOfWeek(this DateTime date)
        {
            return date.GetLastDayOfWeek(null);
        }

        /// <summary>
        /// 	Gets the last day of the week using the specified culture.
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <param name = "cultureInfo">The culture to determine the first weekday of a week.</param>
        /// <returns>The first day of the week</returns>
        public static DateTime GetLastDayOfWeek(this DateTime date, CultureInfo cultureInfo)
        {
            return date.GetFirstDayOfWeek(cultureInfo).AddDays(6);
        }

        /// <summary>
        /// 	Gets the next occurence of the specified weekday within the current week using the current culture.
        /// </summary>
        /// <param name = "date">The base date.</param>
        /// <param name = "weekday">The desired weekday.</param>
        /// <returns>The calculated date.</returns>
        /// <example>
        /// 	<code>
        /// 		var thisWeeksMonday = DateTime.Now.GetWeekday(DayOfWeek.Monday);
        /// 	</code>
        /// </example>
        public static DateTime GetWeeksWeekday(this DateTime date, DayOfWeek weekday)
        {
            return date.GetWeeksWeekday(weekday, null);
        }

        /// <summary>
        /// 	Gets the next occurence of the specified weekday within the current week using the specified culture.
        /// </summary>
        /// <param name = "date">The base date.</param>
        /// <param name = "weekday">The desired weekday.</param>
        /// <param name = "cultureInfo">The culture to determine the first weekday of a week.</param>
        /// <returns>The calculated date.</returns>
        /// <example>
        /// 	<code>
        /// 		var thisWeeksMonday = DateTime.Now.GetWeekday(DayOfWeek.Monday);
        /// 	</code>
        /// </example>
        public static DateTime GetWeeksWeekday(this DateTime date, DayOfWeek weekday, CultureInfo cultureInfo)
        {
            var firstDayOfWeek = date.GetFirstDayOfWeek(cultureInfo);
            return firstDayOfWeek.GetNextWeekday(weekday);
        }

        /// <summary>
        /// 	Gets the next occurence of the specified weekday.
        /// </summary>
        /// <param name = "date">The base date.</param>
        /// <param name = "weekday">The desired weekday.</param>
        /// <returns>The calculated date.</returns>
        /// <example>
        /// 	<code>
        /// 		var lastMonday = DateTime.Now.GetNextWeekday(DayOfWeek.Monday);
        /// 	</code>
        /// </example>
        public static DateTime GetNextWeekday(this DateTime date, DayOfWeek weekday)
        {
            while (date.DayOfWeek != weekday)
                date = date.AddDays(1);
            return date;
        }

        /// <summary>
        /// 	Gets the previous occurence of the specified weekday.
        /// </summary>
        /// <param name = "date">The base date.</param>
        /// <param name = "weekday">The desired weekday.</param>
        /// <returns>The calculated date.</returns>
        /// <example>
        /// 	<code>
        /// 		var lastMonday = DateTime.Now.GetPreviousWeekday(DayOfWeek.Monday);
        /// 	</code>
        /// </example>
        public static DateTime GetPreviousWeekday(this DateTime date, DayOfWeek weekday)
        {
            while (date.DayOfWeek != weekday)
                date = date.AddDays(-1);
            return date;
        }

        /// <summary>
        /// 	Determines whether the date only part of twi DateTime values are equal.
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <param name = "dateToCompare">The date to compare with.</param>
        /// <returns>
        /// 	<c>true</c> if both date values are equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsDateEqual(this DateTime date, DateTime dateToCompare)
        {
            return (date.Date == dateToCompare.Date);
        }

        /// <summary>
        /// 	Determines whether the time only part of two DateTime values are equal.
        /// </summary>
        /// <param name = "time">The time.</param>
        /// <param name = "timeToCompare">The time to compare.</param>
        /// <returns>
        /// 	<c>true</c> if both time values are equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTimeEqual(this DateTime time, DateTime timeToCompare)
        {
            return (time.TimeOfDay == timeToCompare.TimeOfDay);
        }

        /// <summary>
        /// 	Get milliseconds of UNIX area. This is the milliseconds since 1/1/1970
        /// </summary>
        /// <param name = "datetime">Up to which time.</param>
        /// <returns>number of milliseconds.</returns>
        /// <remarks>
        /// 	Contributed by blaumeister, http://www.codeplex.com/site/users/view/blaumeiser
        /// </remarks>
        public static long GetMillisecondsSince1970(this DateTime datetime)
        {
            var ts = datetime.Subtract(Date1970);
            return (long)ts.TotalMilliseconds;
        }

        /// <summary>
        /// 	Indicates whether the specified date is a weekend (Saturday or Sunday).
        /// </summary>
        /// <param name = "date">The date.</param>
        /// <returns>
        /// 	<c>true</c> if the specified date is a weekend; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsWeekend(this DateTime date)
        {
            return date.DayOfWeek.EqualsAny(DayOfWeek.Saturday, DayOfWeek.Sunday);
        }

        /// <summary>
        /// 	Adds the specified amount of weeks (=7 days gregorian calendar) to the passed date value.
        /// </summary>
        /// <param name = "date">The origin date.</param>
        /// <param name = "value">The amount of weeks to be added.</param>
        /// <returns>The enw date value</returns>
        public static DateTime AddWeeks(this DateTime date, int value)
        {
            return date.AddDays(value * 7);
        }

        ///<summary>
        ///	Get the number of days within that year.
        ///</summary>
        ///<param name = "year">The year.</param>
        ///<returns>the number of days within that year</returns>
        /// <remarks>
        /// 	Contributed by Michael T, http://stackoverflow.com/users/190249/michael-t
        /// </remarks>
        public static int GetDays(int year)
        {
            var first = new DateTime(year, 1, 1);
            var last = new DateTime(year + 1, 1, 1);
            return GetDays(first, last);
        }

        ///<summary>
        ///	Get the number of days within that date year.
        ///</summary>
        ///<param name = "date">The date.</param>
        ///<returns>the number of days within that year</returns>
        /// <remarks>
        /// 	Contributed by Michael T, http://stackoverflow.com/users/190249/michael-t
        /// </remarks>
        public static int GetDays(this DateTime date)
        {
            return GetDays(date.Year);
        }

        ///<summary>
        ///	Get the number of days between two dates.
        ///</summary>
        ///<param name = "fromDate">The origin year.</param>
        ///<param name = "toDate">To year</param>
        ///<returns>The number of days between the two years</returns>
        /// <remarks>
        /// 	Contributed by Michael T, http://stackoverflow.com/users/190249/michael-t
        /// </remarks>
        public static int GetDays(this DateTime fromDate, DateTime toDate)
        {
            return Convert.ToInt32(toDate.Subtract(fromDate).TotalDays);
        }

        ///<summary>
        ///	Return a period "Morning", "Afternoon", or "Evening"
        ///</summary>
        ///<param name = "date">The date.</param>
        ///<returns>The period "morning", "afternoon", or "evening"</returns>
        /// <remarks>
        /// 	Contributed by Michael T, http://stackoverflow.com/users/190249/michael-t
        /// </remarks>
        public static string GetPeriodOfDay(this DateTime date)
        {
            var hour = date.Hour;
            if (hour < EveningEnds)
                return "evening";
            if (hour < MorningEnds)
                return "morning";
            return hour < AfternoonEnds ? "afternoon" : "evening";
        }

        /// <summary>
        /// Gets the week number for a provided date time value based on the current culture settings.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>The week number</returns>
        public static int GetWeekOfYear(this DateTime dateTime)
        {
            var culture = CultureInfo.CurrentUICulture;
            var calendar = culture.Calendar;
            var dateTimeFormat = culture.DateTimeFormat;

            return calendar.GetWeekOfYear(dateTime, dateTimeFormat.CalendarWeekRule, dateTimeFormat.FirstDayOfWeek);
        }
    }

    /// <summary>
    /// 	Extension methods for the DateTimeOffset data type.
    /// </summary>
    public static class DateTimeOffsetExtensions
    {
        /// <summary>
        /// 	Indicates whether the date is today.
        /// </summary>
        /// <param name = "dto">The date.</param>
        /// <returns>
        /// 	<c>true</c> if the specified date is today; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsToday(this DateTimeOffset dto)
        {
            return (dto.Date.IsToday());
        }

        /// <summary>
        /// 	Sets the time on the specified DateTimeOffset value using the local system time zone.
        /// </summary>
        /// <param name = "date">The base date.</param>
        /// <param name = "hours">The hours to be set.</param>
        /// <param name = "minutes">The minutes to be set.</param>
        /// <param name = "seconds">The seconds to be set.</param>
        /// <returns>The DateTimeOffset including the new time value</returns>
        public static DateTimeOffset SetTime(this DateTimeOffset date, int hours, int minutes, int seconds)
        {
            return date.SetTime(new TimeSpan(hours, minutes, seconds));
        }

        /// <summary>
        /// 	Sets the time on the specified DateTime value using the local system time zone.
        /// </summary>
        /// <param name = "date">The base date.</param>
        /// <param name = "time">The TimeSpan to be applied.</param>
        /// <returns>
        /// 	The DateTimeOffset including the new time value
        /// </returns>
        public static DateTimeOffset SetTime(this DateTimeOffset date, TimeSpan time)
        {
            return date.SetTime(time, null);
        }

        /// <summary>
        /// 	Sets the time on the specified DateTime value using the specified time zone.
        /// </summary>
        /// <param name = "date">The base date.</param>
        /// <param name = "time">The TimeSpan to be applied.</param>
        /// <param name = "localTimeZone">The local time zone.</param>
        /// <returns>/// The DateTimeOffset including the new time value/// </returns>
        public static DateTimeOffset SetTime(this DateTimeOffset date, TimeSpan time, TimeZoneInfo localTimeZone)
        {
            var localDate = date.ToLocalDateTime(localTimeZone);
            localDate.SetTime(time);
            return localDate.ToDateTimeOffset(localTimeZone);
        }

        /// <summary>
        /// 	Converts a DateTimeOffset into a DateTime using the local system time zone.
        /// </summary>
        /// <param name = "dateTimeUtc">The base DateTimeOffset.</param>
        /// <returns>The converted DateTime</returns>
        public static DateTime ToLocalDateTime(this DateTimeOffset dateTimeUtc)
        {
            return dateTimeUtc.ToLocalDateTime(null);
        }

        /// <summary>
        /// 	Converts a DateTimeOffset into a DateTime using the specified time zone.
        /// </summary>
        /// <param name = "dateTimeUtc">The base DateTimeOffset.</param>
        /// <param name = "localTimeZone">The time zone to be used for conversion.</param>
        /// <returns>The converted DateTime</returns>
        public static DateTime ToLocalDateTime(this DateTimeOffset dateTimeUtc, TimeZoneInfo localTimeZone)
        {
            localTimeZone = (localTimeZone ?? TimeZoneInfo.Local);

            return TimeZoneInfo.ConvertTime(dateTimeUtc, localTimeZone).DateTime;
        }
    }
}
