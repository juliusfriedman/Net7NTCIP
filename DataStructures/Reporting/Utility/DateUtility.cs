using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStructures.Reporting;

namespace ASTITransportation.DataStructures
{
    public sealed class DateUtility
    {
        #region Constants

        public const int HoursInDay = 24;
        public const int MinutesInHour = 60;
        public const int SecondsInMinute = 60;

        #endregion

        #region Constructor

        private DateUtility()
        {
        }

        #endregion

        #region Month Methods

        /// <summary>
        /// Finds the Number of the month given
        /// </summary>
        /// <param name="month">The string of the full or abbreviated month name</param>
        /// <returns>The month number or -1 if not valid</returns>
        public static int MonthNumber(string month)
        {
            if (string.IsNullOrEmpty(month)) return -1;
            month = month.Trim();
            month = month.ToLowerInvariant();
            month = month.Replace(".", string.Empty);
            switch (month)
            {
                case "january":
                case "jan":
                    return 1;
                case "february":
                case "feb":
                    return 2;
                case "march":
                case "mar":
                    return 3;
                case "april":
                case "apr":
                    return 4;
                case "may":
                    return 5;
                case "june":
                case "jun":
                    return 6;
                case "july":
                case "jul":
                    return 7;
                case "august":
                case "aug":
                    return 8;
                case "september":
                case "sep":
                case "sept":
                    return 9;
                case "october":
                case "oct":
                    return 10;
                case "november":
                case "nov":
                    return 11;
                case "december":
                case "dec":
                    return 12;
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Find the Name of the Month from the number given
        /// </summary>
        /// <param name="month">The month to find a name for</param>
        /// <returns>The Full Name of the Month</returns>
        public static string MonthName(int month)
        {
            if (month < 1) throw new ArgumentOutOfRangeException("month", "Cannot be less than 1");
            if (month > 12) throw new ArgumentOutOfRangeException("month", "Cannot be greater than 12");
            switch (month)
            {
                default:
                case 1:
                    return "January";
                case 2:
                    return "February";
                case 3:
                    return "March";
                case 4:
                    return "April";
                case 5:
                    return "May";
                case 6:
                    return "June";
                case 7:
                    return "July";
                case 8:
                    return "August";
                case 9:
                    return "September";
                case 10:
                    return "October";
                case 11:
                    return "November";
                case 12:
                    return "December";
            }
        }

        /// <summary>
        /// Find the abbreviation of the Month from the month string given
        /// </summary>
        /// <param name="month">The month to find a abbreviation for</param>
        /// <returns>The month abbreviation</returns>
        public static string MonthAbbreviation(string month)
        {
            return MonthAbbreviation(MonthNumber(month));
        }

        /// <summary>
        /// Find the abbreviation of the Month from the month
        /// </summary>
        /// <param name="month">The month number to find a abbreviation for</param>
        /// <returns>The month abbreviation</returns>
        public static string MonthAbbreviation(int month)
        {
            if (month < 1) throw new ArgumentOutOfRangeException("month", "Cannot be less than 1");
            if (month > 12) throw new ArgumentOutOfRangeException("month", "Cannot be greater than 12");
            switch (month)
            {
                default:
                case 1:
                    return "Jan";
                case 2:
                    return "Feb";
                case 3:
                    return "Mar";
                case 4:
                    return "Apr";
                case 5:
                    return "May";
                case 6:
                    return "Jun";
                case 7:
                    return "Jul";
                case 8:
                    return "Aug";
                case 9:
                    return "Sept";
                case 10:
                    return "Oct";
                case 11:
                    return "Nov";
                case 12:
                    return "Dec";
            }
        }

        #endregion

        #region DaysInMonth

        /// <summary>
        /// Returns the days in the month and year given
        /// </summary>
        /// <param name="month">The month</param>
        /// <param name="year">The year</param>
        /// <returns></returns>
        public static int DaysInMonth(int month, int year)
        {
            return DateTime.DaysInMonth(year, month);
        }

        /// <summary>
        /// Returns the days in the month and year given
        /// </summary>
        /// <param name="month">The month</param>
        /// <param name="year">The year</param>
        /// <returns></returns>
        public static int DaysInMonth(string month, int year)
        {
            return DateTime.DaysInMonth(year, MonthNumber(month));
        }

        /// <summary>
        /// Returns the days in the month given and the Current year
        /// </summary>
        /// <param name="month">The month</param>
        /// <returns></returns>
        public static int DaysInMonth(int month)
        {
            return DateTime.DaysInMonth(DateTime.Now.Year, month);
        }
        
        /// <summary>
        /// Returns the days in the month given and the Current year
        /// </summary>
        /// <param name="month">The month</param>
        /// <returns></returns>
        public static int DaysInMonth(string month)
        {
            return DateTime.DaysInMonth(DateTime.Now.Year, MonthNumber(month));
        }
        
        /// <summary>
        /// Returns the days in the month of the date given
        /// </summary>
        /// <param name="date">The date</param>
        /// <returns></returns>
        public static int DaysInMonth(DateTime date)
        {
            return DateTime.DaysInMonth(date.Year, date.Month);
        }

        /// <summary>
        /// Returns the days in the month of the current year and month
        /// </summary>
        /// <returns></returns>
        public static int DaysInMonth()
        {
            return DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
        }

        #endregion

        #region FirstOfMonth

        public static DateTime FirstOfMonth(int month)
        {
            return FirstOfMonth(month, DateTime.Now.Year);
        }

        public static DateTime FirstOfMonth(string month)
        {
            return FirstOfMonth(MonthNumber(month), DateTime.Now.Year);
        }

        public static DateTime FirstOfMonth(string month, int year)
        {
            return FirstOfMonth(MonthNumber(month), year);
        }

        public static DateTime FirstOfMonth(int month, int year)
        {
            if (year < DateTime.MinValue.Year) throw new ArgumentOutOfRangeException("year", "Cannot be less than DateTime.MinValue.Year");
            if (year > DateTime.MaxValue.Year) throw new ArgumentOutOfRangeException("year", "Cannot be greater than DateTime.MaxValue.Year");
            if (month < 1) throw new ArgumentOutOfRangeException("month", "Was not a valid month");
            if (month > 12) throw new ArgumentOutOfRangeException("month", "Was not a valid month");

            return new DateTime(year, month, 1);
        }

        public static DateTime FirstOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1, 0, 0, 0, date.Kind);
        }

        #endregion

        #region LastOfMonth

        public static DateTime LastOfMonth(int month)
        {
            return LastOfMonth(month, DateTime.Now.Year);
        }

        public static DateTime LastOfMonth(string month)
        {
            return LastOfMonth(MonthNumber(month), DateTime.Now.Year);
        }

        public static DateTime LastOfMonth(string month, int year)
        {
            return LastOfMonth(MonthNumber(month), year);
        }

        public static DateTime LastOfMonth(int month, int year)
        {
            if (year < DateTime.MinValue.Year) throw new ArgumentOutOfRangeException("year", "Cannot be less than DateTime.MinValue.Year");
            if (year > DateTime.MaxValue.Year) throw new ArgumentOutOfRangeException("year", "Cannot be greater than DateTime.MaxValue.Year");
            if (month < 1) throw new ArgumentOutOfRangeException("month", "Was not a valid month");
            if (month > 12) throw new ArgumentOutOfRangeException("month", "Was not a valid month");

            return new DateTime(year, month, DaysInMonth(month, year));

        }

        public static DateTime LastOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, DaysInMonth(date), 0, 0, 0, date.Kind);
        }

        #endregion

        #region OccurancesOfDayInMonth

        public static int OccurancesOfDayInMonth(DayOfWeek day, string month, int year)
        {
            return OccurancesOfDayInMonth(day, MonthNumber(month), year);
        }

        public static int OccurancesOfDayInMonth(DayOfWeek day, string month)
        {
            return OccurancesOfDayInMonth(day, MonthNumber(month), DateTime.Now.Year);
        }

        public static int OccurancesOfDayInMonth(DayOfWeek day, int month)
        {
            return OccurancesOfDayInMonth(day, month, DateTime.Now.Year);
        }

        public static int OccurancesOfDayInMonth(DayOfWeek day)
        {
            return OccurancesOfDayInMonth(day, DateTime.Now.Month, DateTime.Now.Year);
        }

        public static int OccurancesOfDayInMonth(DayOfWeek day, int month, int year)
        {
            if (year < DateTime.MinValue.Year) throw new ArgumentOutOfRangeException("year", "Cannot be less than DateTime.MinValue.Year");
            if (year > DateTime.MaxValue.Year) throw new ArgumentOutOfRangeException("year", "Cannot be greater than DateTime.MaxValue.Year");
            if (month < 1) throw new ArgumentOutOfRangeException("month", "Cannot be less than 1");
            if (month > 12) throw new ArgumentOutOfRangeException("month", "Cannot be greater than 12");
            if ((int)day < 0) throw new ArgumentOutOfRangeException("day", "Cannot be less than 0");
            if ((int)day > 6) throw new ArgumentOutOfRangeException("day", "Cannot be greater than 6");

            return (DaysInMonth(month, year) + (int)new DateTime(year, month, 7 - (int)day).DayOfWeek) / 7;
        }

        #endregion

        #region OccurancesOfDayInYear

        public static int OccurancesOfDayInYear(DayOfWeek day)
        {
            return OccurancesOfDayInYear(day, DateTime.Now.Year);
        }  
        
        public static int OccurancesOfDayInYear(DayOfWeek day, int year)
        {
            if (year < DateTime.MinValue.Year) throw new ArgumentOutOfRangeException("year", "Cannot be less than DateTime.MinValue.Year");
            if (year > DateTime.MaxValue.Year) throw new ArgumentOutOfRangeException("year", "Cannot be greater than DateTime.MaxValue.Year");
            if ((int)day < 0) throw new ArgumentOutOfRangeException("day", "Cannot be less than 0");
            if ((int)day > 6) throw new ArgumentOutOfRangeException("day", "Cannot be greater than 6");

            return (365 - 28 + DaysInMonth(2, year) + (int)new DateTime(year, 1, 7 - (int)day).DayOfWeek) / 7;
        }

        #endregion

        #region Quarters

        public enum Quarter
        {
            PreviousQuarter = -1,
            PresentQuarter = 0,
            NextQuarter = 1
        }

        public static DateTime QuarterEnddate(DateTime curDate, Quarter whichQtr)
        {
            int tQtr = (curDate.Month - 1) / 3 + 1 + (int)whichQtr;   
            return new DateTime(curDate.Year, (tQtr * 3) + 1, 1).AddDays(-1);
        }

        public static DateTime QuarterStartDate(DateTime curDate, Quarter whichQtr)
        {   
            return QuarterEnddate(curDate, whichQtr).AddDays(1).AddMonths(-3);
        }
        
        public static int QuarterOfDate(DateTime curDate){   
            //return (curDate.Month - 1) / 3 + (curDate.Month < 4 ? 4 : 0);
            return (curDate.Month + 2) / 3;
        }

        #endregion

        #region DateTime Validation

        static internal void ValidateDay(int day, int year, int month)
        {
            ValidateMonth(month);
            int daysInMonth = DateTime.DaysInMonth(year, month);
            if (!(day < daysInMonth)) throw new ArgumentException("Invalid day of month for given year, must be less then " + daysInMonth, "day");
        }

        static internal void ValidateMonth(int month)
        {
            if (month <= 0 || month >= 13) throw new ArgumentException("Invalid month, please use only 1 - 12", "month");
        }

        static internal void ValidateHour(int hour)
        {
            if (hour < 0 || hour >= 24) throw new ArgumentException("Invalid hour, please use only 0 - 24", "hour");
        }

        static internal void ValidateMinuteOrSecond(int minuteOrSecond)
        {
            if (minuteOrSecond < 0 || minuteOrSecond >= 60) throw new ArgumentException("Invalid minute, please use only 0 - 60", "minuteOrSecond");
        }

        static internal void ValidateYear(int year)
        {
            if (year < DateTime.MinValue.Year) throw new ArgumentException("Must be greater than DateTime.MinValue.Year", "year");
            if (year > DateTime.MaxValue.Year) throw new ArgumentException("Must be less than DateTime.MaxValue.Year", "year");
        }

        static internal void ValidateYear(BaseReport report, DateTime dateTime)
        {
            if (dateTime.Year > report.ReportInformation.EndDate.Year) throw new ArgumentException("Must be less than or equal to ReportInformation.EndDate.Year", "year");

            if (dateTime.Year < report.ReportInformation.StartDate.Year) throw new ArgumentException("Must be greater than or equal to ReportInformation.EndDate.Year", "year");
        }

        #endregion
    }
}
