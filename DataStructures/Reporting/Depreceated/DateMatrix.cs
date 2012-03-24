using System;
using System.Web.Script.Serialization; 
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ASTITransportation.DataStructures
{
    public class DateMatrix
    {
        /// <summary>
        /// Used for adding data to the dataDictionary in a threaded manner
        /// </summary>
        internal struct AddThreaded
        {
            public int start;
            public int count;
            public DataSet dataSet;
            public string dateTimeField;
            public string dataField;
            public double[, , ,] data;
            public AddThreaded(int start, int count, ref DataSet dataSet, ref string dateTimeField, ref string dataField, ref double [, , ,] data)
            {
                this.start = start;
                this.count = count;
                this.dataSet = dataSet;
                this.dateTimeField = dateTimeField;
                this.dataField = dataField;
                this.data = data;
            }
        }        

        #region Constructor

        /// <summary>
        /// Constructs a DateMatrix for the Current Year and Current Month
        /// </summary>
        public DateMatrix()
        {
            yearInReview = DateTime.Now.Year;
            monthInReview = DateTime.Now.Month;            
        }

        /// <summary>
        /// Constructs a DateMatrix for the given Year and Month
        /// </summary>
        /// <param name="year">The year the matrix represents</param>
        /// <param name="month">The month the matrix represents</param>
        public DateMatrix(int year, int month)
        {
            if (year < DateTime.MinValue.Year) throw new ArgumentOutOfRangeException("year", "Cannot be less than DateTime.MinValue.Year");
            if (year > DateTime.MaxValue.Year) throw new ArgumentOutOfRangeException("year", "Cannot be greater than DateTime.MaxValue.Year");
            if (month < 1) throw new ArgumentOutOfRangeException("month", "Was not a valid month");
            if (month > 12) throw new ArgumentOutOfRangeException("month", "Was not a valid month");
            this.yearInReview = year;
            this.monthInReview = month;
        }

        /// <summary>
        /// Constructs a DateMatrix for the current year
        /// </summary>
        /// <param name="month">The month in review of this DateMatrix</param>
        public DateMatrix(int month) : this(DateTime.Now.Year, month) { }

        #endregion

        #region Fields

        /// <summary>
        /// The year the matrix is representing
        /// </summary>
        int yearInReview;

        /// <summary>
        /// The month the matrix is representing
        /// </summary>
        int monthInReview;

        /// <summary>
        /// The dataDictionary where 4 dimensional data is stored
        /// </summary>
        Dictionary<string, double[, , ,]> dataDictionary = new Dictionary<string, double[, , ,]>();

        /// <summary>
        /// The JavaScriptSerializer of the DateMatrix
        /// </summary>
        static JavaScriptSerializer jsonSerializer = new JavaScriptSerializer()
        {
            MaxJsonLength = int.MaxValue           
        };

        #endregion

        #region Properties

        ///May need an Indexer

        /// <summary>
        /// The year the matrix is representing
        /// </summary>
        public int YearInReview { get { return yearInReview; } }

        /// <summary>
        /// The month the matrix is representing
        /// </summary>
        public int MonthInReview { get { return monthInReview; } }

        /// <summary>
        /// The dataDictionary where 4 dimensional data is stored
        /// </summary>
        public Dictionary<string, double[, , ,]> DataDictionary { get { return dataDictionary; } }

        /// <summary>
        /// Provides a SynchLock around the dataDictionary
        /// </summary>
        Object SyncRoot { get { return (((System.Collections.IDictionary)dataDictionary).SyncRoot); } }

        #endregion

        #region Data Methods

        /// <summary>
        /// Adds a 4 dimensional dataSet to the DateMatrix
        /// </summary>
        /// <param name="key">The key the dataSet should be stored under</param>
        /// <param name="value">The 4 dimensional dataSet to store</param>
        public void AddDataSet(string key, double[, , ,] value)
        {
            lock (SyncRoot)
            {
                dataDictionary.Add(key, value);
            }            
        }

        /// <summary>
        /// Adds a 4 dimensional dataSet to the DateMatrix by disceminating the data from the given DataSet
        /// </summary>
        /// <param name="key">The key they dataSet should be stored under</param>
        /// <param name="dataSet">The System.DataSet to disceminate</param>
        /// <param name="dateTimeField">The field in the DataSet which is of type DateTime</param>
        /// <param name="dataField">The field in the DataSet which is to be used in the 4 dimensional dataSet for values</param>
        public void AddDataSet(string key, DataSet dataSet, string dateTimeField, string dataField)
        {
            //make a place to hold the data we are sharing amount threads
            double[, , ,] newData = new double[31, 24, 60, 60];
            //determine the record count
            int count = dataSet.Tables[0].Rows.Count - 1;//T
            //set the amount of records each thread will process
            int threadRecords = 5000;//Mx
            //detmine the amount of threads it will take
            int threadCount = count / threadRecords;//Tx
            //add one for the difference in the last thread
            threadCount++;
            
            //allocate threads
            Thread[] threads = new Thread[threadCount];
            
            //start at 0
            int start = 0;

            int lastThread = threadCount - 1;

            for (int threadIndex = 0; threadIndex < threadCount; threadIndex++)
            {
                //If this is the thread which takes care of the difference then use count - name which is start
                if (threadIndex == lastThread)
                {
                    threads[threadIndex] = new Thread(new ParameterizedThreadStart(AddDataSetThreaded));
                    threads[threadIndex].Priority = ThreadPriority.AboveNormal;
                    threads[threadIndex].SetApartmentState(ApartmentState.MTA);
                    threads[threadIndex].Start(new AddThreaded(start,  Math.Abs((count - start)), ref dataSet, ref dateTimeField, ref dataField, ref newData));
                }
                else
                {
                    //If this is a thread which does not take care of diffrence then use name which is start
                    threads[threadIndex] = new Thread(new ParameterizedThreadStart(AddDataSetThreaded));
                    threads[threadIndex].Priority = ThreadPriority.AboveNormal;
                    threads[threadIndex].SetApartmentState(ApartmentState.MTA);
                    threads[threadIndex].Start(new AddThreaded(start, threadRecords, ref dataSet, ref dateTimeField, ref dataField, ref newData));
                }
                start += threadRecords;
            }

            ///move execution time away from this thread
            Thread.CurrentThread.Priority = ThreadPriority.Lowest;           

            //while any thread is working
            while (threads.Any(thread => thread.IsAlive == true))
            {
                //sleep indefinitely for it to complete 
                Thread.Sleep(0);
            }

            Thread.CurrentThread.Priority = ThreadPriority.Normal;

            //add the dataset to the dataDictionary.
            AddDataSet(key, newData);
        }

        /// <summary>
        /// Adds a 4 dimensional dataSet to the DateMatrix, 
        /// Allows a dataLoading function to be used on the dataSet to perform algorithms while loading the data into the matrix
        /// </summary>
        /// <param name="key">The key the dataSet should be stored under</param>
        /// <param name="dataSet">The DataSet which contains the data to be loaded into the dateMatrix</param>
        /// <param name="dataLoader">A Function which takes the DataSet and returns a 4 dimensional double[] with data of the DataSet</param>
        public void AddDataSet(string key, DataSet dataSet, Func<DataSet, double[, , ,]> dataLoader)
        {
            AddDataSet(key, dataLoader.Invoke(dataSet));
        }

        /// <summary>
        /// Adds a DataSet from a Add Threaded
        /// </summary>
        /// <param name="cadd"></param>
        internal void AddDataSetThreaded(object addThreaded)
        {
            if (null == addThreaded) return;
            AddThreaded add = (AddThreaded)addThreaded;

            DataSet dataSet = add.dataSet;

            DataTable dataTable = dataSet.Tables[0];

            DataRow dataRow;// = dataTable.Rows[i];  

            DataColumnCollection dataColumns = dataTable.Columns;

            DataColumn dataColumn = null;

            string dataFieldName = add.dataField.ToLowerInvariant();

            foreach (DataColumn tempCol in dataColumns)
            {
                if (tempCol.ColumnName.ToLowerInvariant() == dataFieldName)
                {
                    dataColumn = tempCol;
                    break;
                }
                else continue;
            }

            if (null == dataColumn) return;

            Type columnType = dataColumn.DataType;

            bool convertDouble = columnType.IsAssignableFrom(typeof(double));

            bool convertInt = columnType.IsAssignableFrom(typeof(int));

            bool convertString = columnType.IsAssignableFrom(typeof(string));

            try
            {
                for (int index = add.start, end = add.count; index < end; index++)
                {

                    dataRow = dataTable.Rows[index];

                    DateTime recordDate = DateTime.SpecifyKind(dataRow.Field<DateTime>(add.dateTimeField), DateTimeKind.Utc);
                    double recordData = 0;
                    
                    //create a boxed version of the data
                    object tempData = dataRow.ItemArray[dataColumn.Ordinal];

                    //if it is a supported type
                    if (convertDouble)
                    {
                        recordData = (double)tempData;
                    }
                    else if (convertInt)
                    {
                        recordData = Convert.ToDouble((int)tempData);
                    }
                    else if (convertString)
                    {
                        //this is a stringType
                        //Double Parse the value
                        try
                        {
                            recordData = Double.Parse(tempData.ToString());
                        }
                        catch
                        {
                            //this is not a double equatable value
                            continue;
                        }
                    }
                    else//skip unsupported types
                    {
                        continue;
                    }

                    //add the data
                    lock (add.data)
                    {
                        add.data[recordDate.Day - 1, recordDate.Hour, recordDate.Minute, recordDate.Second] += recordData;
                    }

                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Removes a 4 dimensional dataSet from the dataDictionary of this DateMatrix
        /// </summary>
        /// <param name="key">The key of the dataSet to remove the the dataDictionary</param>
        public void RemoveDataSet(string key)
        {
            lock (SyncRoot)
            {
                dataDictionary.Remove(key);
            }
        }        

        /// <summary>
        /// Returns a list of all keys in the dataDictionary of this DateMatrix
        /// </summary>
        /// <returns></returns>
        public List<string> GetDataKeys()
        {
            lock (SyncRoot)
            {
                return dataDictionary.Keys.ToList();
            }
        }

        /// <summary>
        /// Returns a 4 dimensional dataSet from the dataDictionary of this DateMatrix 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public double[, , ,] GetDataSet(string key)
        {
            lock (SyncRoot)
            {
                return dataDictionary[key];
            }
        }

        /// <summary>
        /// Returns an inverted matrix by finding values which are equal to 0 and setting them to 1
        /// and values greater then 0 and setting them to 0
        /// </summary>
        /// <param name="key">The key to retrieve from the dataDictionary and invert</param>
        /// <returns></returns>
        public double[, , ,] GetMissingDataFromDataSet(string key)
        {
            return GetMissingDataFromDataSet(GetDataSet(key));
        }

        /// <summary>
        /// Returns an inverted matrix by finding values which are equal to 0 and setting them to 1
        /// and values greater then 0 and setting them to 0
        /// </summary>
        /// <param name="dataSet">The 4 dimensional dataSet to invert</param>
        /// <returns></returns>
        public double[, , ,] GetMissingDataFromDataSet(double[, , ,] dataSet)
        {
            int dayEnd = dataSet.GetUpperBound(0) + 1,
                monthEnd = DateUtility.DaysInMonth(monthInReview, yearInReview),
                hourEnd = dataSet.GetUpperBound(1) + 1,
                minuteEnd = dataSet.GetUpperBound(2) + 1,
                secondEnd = dataSet.GetUpperBound(3) + 1;

            double[, , ,] data = new double[dayEnd, hourEnd, minuteEnd, secondEnd];

            for (int day = 0; (day < dayEnd && day < monthEnd); day++)
            {
                for (int hour = 0; hour < hourEnd; hour++)
                {
                    for (int minute = 0; minute < minuteEnd; minute++)
                    {
                        for (int second = 0; second < secondEnd; second++)
                        {
                            if (dataSet[day, hour, minute, second] == 0) data[day, hour, minute, second] = 1;
                            else data[day, hour, minute, second] = 0;
                        }
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// Converts a 4 dimensional dataSet to a 3 dimensional dataSet
        /// </summary>
        /// <param name="dataSet">The dataSet to fold</param>
        /// <returns></returns>
        public double[, ,] ConvertTo3D(double[, , ,] dataSet)
        {            
            //A 3d matrix of only days and hours and minutes
            double[, ,] dMatrix = new double[7, 24, 60];
            double MinuteTotal = 0;
            for (int Day = 0, dayEnd = dataSet.GetUpperBound(0) +1, dayInMonth = DateUtility.DaysInMonth(monthInReview, yearInReview); (Day < dayEnd && Day < dayInMonth); Day++)
            {
                for (int Hour = 0, hourEnd = dataSet.GetUpperBound(1) + 1; Hour < hourEnd; Hour++)
                {
                    for (int Minute = 0, minuteEnd = dataSet.GetUpperBound(2) + 1; Minute < minuteEnd; Minute++)
                    {
                        MinuteTotal = 0;
                        for (int Second = 0, secondEnd = dataSet.GetUpperBound(3) + 1; Second < secondEnd; Second++)
                        {
                            MinuteTotal += dataSet[Day, Hour, Minute, Second];
                        }
                        DateTime checker = new DateTime(yearInReview, monthInReview, Day + 1, Hour, Minute, 0);                        
                        MinuteTotal /= 60; //seconds in a minute
                        dMatrix[(int)checker.DayOfWeek, Hour, Minute] = MinuteTotal;
                    }//Minutes                
                }//Hours
            }//Days           
            return dMatrix;
        }

        /// <summary>
        /// Converts a 4 dimensional dataSet to a 3 dimensional dataSet
        /// </summary>
        /// <param name="dataSet">The dataSet to fold</param>
        /// <param name="day">The day to focus</param>
        /// <returns></returns>
        public double[, ,] ConvertTo3D(double[, , ,] dataSet, DayOfWeek day)
        {
            DateTime refrencePoint = DateTime.MinValue;
            int ZeroData = 0;
            //A 3d matrix of only days and hours and minutes
            double[, ,] dMatrix = new double[1, 24, 60];
            double MinuteTotal = 0;
            int dayBound = dataSet.GetUpperBound(0) + 1;
            for (int Day = 0; Day < dayBound; Day++)
            {
                try
                {
                    refrencePoint = new DateTime(yearInReview, monthInReview, Day + 1);
                }
                catch//if this is invalid date
                {
                    break;//stop the loop
                }
                if (refrencePoint.DayOfWeek != day) continue;
                for (int Hour = 0; Hour < 24; Hour++)
                {
                    for (int Minute = 0; Minute < 60; Minute++)
                    {
                        ZeroData = 0;
                        MinuteTotal = 0;
                        for (int Second = 0; Second < 60; Second++)
                        {
                            if (dataSet[Day, Hour, Minute, Second] == 0)
                                ZeroData++;
                            MinuteTotal += dataSet[Day, Hour, Minute, Second];
                        }
                        int Correction = 60 - ZeroData;
                        int DayOccurs = DateUtility.OccurancesOfDayInMonth(day, monthInReview, yearInReview);                        

                        MinuteTotal /= 60; //seconds in a minute

                        if (Correction < DayOccurs * 2 && Correction != 0)
                            dMatrix[0, Hour, Minute] = MinuteTotal + (MinuteTotal / Correction) * ((DayOccurs * 2 - Correction));
                        else
                            dMatrix[0, Hour, Minute] = MinuteTotal;                       
                    }//Minutes                
                }//Hours 
            }
            return dMatrix;
        }

        /// <summary>
        /// Converts a 4 dimensional dataSet to a 3 dimensional dataSet
        /// </summary>
        /// <param name="dataSet">The dataSet to fold</param>
        /// <param name="day">The day to focus</param>
        /// <param name="hour">The hour to focus</param>
        /// <returns></returns>
        public double[, ,] ConvertTo3D(double[, , ,] dataSet, DayOfWeek day, int hour)
        {
            int ZeroData = 0;
            //A 3d matrix of only days and hours and minutes
            double[, ,] dMatrix = new double[1, 1, 60];
            double MinuteTotal = 0;
            if (hour < 0) throw new ArgumentOutOfRangeException("hour", "Cannot be less than 0");
            if (hour > 24) throw new ArgumentOutOfRangeException("hour", "Cannot be greater than 24");
            DateTime checker = DateTime.MinValue;
            int dayBound = dataSet.GetUpperBound(0);
            for (int Day = 0; Day < dayBound; Day++)
            {
                checker = new DateTime(yearInReview, monthInReview, Day + 1);
                if (checker.DayOfWeek != day) continue;
                for (int Minute = 0; Minute < 60; Minute++)
                {
                    ZeroData = 0;
                    MinuteTotal = 0;
                    for (int Second = 0; Second < 60; Second++)
                    {
                        if (dataSet[(int)day, hour, Minute, Second] == 0) ZeroData++;                            
                        MinuteTotal += dataSet[Day, hour, Minute, Second];
                    }

                    int Correction = 60 - ZeroData;

                    int DayOccurs = DateUtility.OccurancesOfDayInMonth(day, monthInReview, yearInReview);

                    if (MinuteTotal == 0)
                        MinuteTotal = 1;

                    MinuteTotal /= 60; //seconds in a minute

                    if (Correction != 0 && Correction < (DayOccurs * 2))
                        dMatrix[0, 0, Minute] = MinuteTotal + (MinuteTotal / Correction) * ((DayOccurs * 2 - Correction));
                    else
                        dMatrix[0, 0, Minute] = MinuteTotal;

                }//Minutes   
            }
                                        
            return dMatrix;
        }

        /// <summary>
        /// Converts a 4 dimensional dataSet to a 3 dimensional dataSet
        /// </summary>
        /// <param name="dataSet">The dataSet to fold</param>
        /// <param name="day">The day to focus</param>
        /// <returns></returns>
        public double[, ,] ConvertTo3D(double[, , ,] dataSet, int dayofmonth)
        {
            if (dayofmonth < 0 || dayofmonth > DateUtility.DaysInMonth(monthInReview, yearInReview)) throw new ArgumentOutOfRangeException("dayofmonth", "Cannot be less than 0 or greater than " + DateUtility.DaysInMonth(monthInReview, yearInReview));
            if (dayofmonth > 0) dayofmonth--;
            //A 3d matrix of only days and hours and minutes
            double[, ,] dMatrix = new double[1, 24, 60];
            double MinuteTotal = 0;
            for (int Hour = 0; Hour < 24; Hour++)
            {
                for (int Minute = 0; Minute < 60; Minute++)
                {
                    MinuteTotal = 0;
                    for (int Second = 0; Second < 60; Second++)
                    {
                        MinuteTotal += dataSet[dayofmonth, Hour, Minute, Second];
                    }
                    dMatrix[0, Hour, Minute] = MinuteTotal / 60;//seconds in a minute
                }//Minutes                
            }//Hours            
            return dMatrix;
        }

        /// <summary>
        /// Converts a 4 dimensional dataSet to a 3 dimensional dataSet
        /// </summary>
        /// <param name="dataSet">The dataSet to fold</param>
        /// <param name="day">The day to focus</param>
        /// <param name="hour">The hour to focus</param>
        /// <returns></returns>
        public double[, ,] ConvertTo3D(double[, , ,] dataSet, int dayofmonth, int hour)
        {
            if (dayofmonth < 0 || dayofmonth > DateUtility.DaysInMonth(monthInReview, yearInReview)) throw new ArgumentOutOfRangeException("dayofmonth", "Cannot be less than 0 or greater than " + DateUtility.DaysInMonth(monthInReview, yearInReview));
            if (dayofmonth > 0) dayofmonth--;
            //A 3d matrix of only days and hours and minutes
            double[, ,] dMatrix = new double[1, 1, 60];
            double MinuteTotal = 0;
            if (hour < 0) throw new ArgumentOutOfRangeException("hour", "Cannot be less than 0");
            if (hour > 24) throw new ArgumentOutOfRangeException("hour", "Cannot be greater than 24");
            for (int Minute = 0; Minute < 60; Minute++)
            {
                MinuteTotal = 0;
                for (int Second = 0; Second < 60; Second++)
                {                    
                    MinuteTotal += dataSet[dayofmonth, hour, Minute, Second];
                }
                dMatrix[0, 0, Minute] = MinuteTotal / 60;//seconds in a minute
            }//Minutes                               
            return dMatrix;
        }

        /// <summary>
        /// Converts a 3 dimensional dataSet to a 2 dimensional dataSet
        /// </summary>
        /// <param name="dataSet">The dataSet to fold</param>
        /// <returns></returns>
        public double[,] ConvertTo2D(double[, ,] dataSet)
        {
            //A 2d matrix of only days and hours
            int dayEnd = dataSet.GetUpperBound(0) + 1;
            int hourEnd = dataSet.GetUpperBound(1) + 1;
            double[,] dMatrix = new double[dayEnd, hourEnd];
            double HoursTotal = 0;
            for (int Day = 0; Day < dayEnd; Day++)
            {
                for (int Hour = 0; Hour < hourEnd; Hour++)
                {
                    HoursTotal = 0;
                    for (int Minute = 0, minuteEnd = dataSet.GetUpperBound(2) + 1; Minute < minuteEnd; Minute++)
                    {
                        HoursTotal += dataSet[Day, Hour, Minute] ;/// DateUtility.OccurancesOfDayInMonth((DayOfWeek)Day, monthInReview, yearInReview);
                    }//Minutes
                    HoursTotal /= 60;//minutes in a hour
                    dMatrix[Day, Hour] = HoursTotal;
                }//Hours
            }//Days    
            return dMatrix;
        }

        /// <summary>
        /// Converts a 3 dimensional dataSet to a 2 dimensional dataSet
        /// </summary>
        /// <param name="dataSet">The dataSet to fold</param>
        /// <param name="day">The day to focus</param>
        /// <returns></returns>
        public double[,] ConvertTo2D(double[, ,] dataSet, DayOfWeek day)
        {
            //A 2d matrix of only days and hours
            int hourEnd = dataSet.GetUpperBound(1) + 1;
            double[,] dMatrix = new double[1, hourEnd];
            double HoursTotal = 0;
            for (int Hour = 0; Hour < hourEnd; Hour++)
            {
                HoursTotal = 0;
                for (int Minute = 0; Minute < 60; Minute++)
                {
                    HoursTotal += dataSet[(int)day, Hour, Minute];// / DateUtility.OccurancesOfDayInMonth(day, monthInReview, yearInReview);
                }//Minutes
                HoursTotal /= 60;//minutes in a hour  
                dMatrix[0, Hour] = HoursTotal;
            }//Hours
            return dMatrix;
        }

        /// <summary>
        /// Converts a 3 dimensional dataSet to a 2 dimensional dataSet
        /// </summary>
        /// <param name="dataSet">The dataSet to fold</param>
        /// <param name="day">The day to focus</param>
        /// <param name="hour">The hour to focus</param>
        /// <returns></returns>
        public double[,] ConvertTo2D(double[, ,] dataSet, DayOfWeek day, int hour)
        {
            if (hour < 0) throw new ArgumentOutOfRangeException("hour", "Cannot be less than 0");
            if (hour > 24) throw new ArgumentOutOfRangeException("hour", "Cannot be greater than 24");
            //A 2d matrix of only days and hours
            double[,] dMatrix = new double[1, 1];
            double HoursTotal = 0;            
            for (int Minute = 0; Minute < 60; Minute++)
            {
                HoursTotal += dataSet[0, hour, Minute];// / DateUtility.OccurancesOfDayInMonth(day, monthInReview, yearInReview);
            }//Minutes
            HoursTotal /= 60; //minuts in a hour
            dMatrix[0, 0] = HoursTotal;
            return dMatrix;
        }

        /// <summary>
        /// Converts a 4 dimensional data set to a JSON Literal containing the Month, year and specificed key from the DataDictionary
        /// </summary>
        /// <param name="key">The key to use in the JSON Literal</param>
        /// <param name="data">The 4 dimensional data to serialize</param>
        /// <returns></returns>
        public string JsonEncode(string key, double[, , ,] data)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("{");
            output.AppendLine("'Year'" + ":" + yearInReview + ",");
            output.AppendLine("'Month'" + ":" + monthInReview + ",");
            output.AppendLine(" '" + key + "' " + ": ");

            int dayBound = data.GetUpperBound(0) + 1;
            int hourBound = data.GetUpperBound(1) + 1;
            int minBound = data.GetUpperBound(2) + 1;
            int secBound = data.GetUpperBound(3) + 1;
            
            double[][][][] dataz = new double[dayBound][][][];
            
            for (int d = 0; d < dayBound; d++)
            {
                dataz[d] = new double[hourBound][][];
                for (int h = 0; h < hourBound; h++)
                {
                    dataz[d][h] = new double[minBound][];
                    for (int m = 0; m < minBound; m++)
                    {
                        dataz[d][h][m] = new double[secBound];
                        for (int s = 0; s < secBound; s++)
                        {
                            dataz[d][h][m][s] = data[d, h, m, s];
                        }
                    }
                }
            }

            output.Append(jsonSerializer.Serialize(dataz));
            
            output.AppendLine("}");

            return output.ToString();            
        }

        /// <summary>
        /// Converts a 3 dimensional data set to a JSON Literal containing the Month, year and specificed key from the DataDictionary
        /// </summary>
        /// <param name="key">The key to use in the JSON Literal</param>
        /// <param name="data">The 3 dimensional data to serialize</param>
        /// <returns></returns>
        public string JsonEncode(string key, double[, ,] data)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("{");
            output.AppendLine("'Year'" + ":" + yearInReview + ",");
            output.AppendLine("'Month'" + ":" + monthInReview + ",");
            output.AppendLine(" '" + key + "' " + ": ");

            int dayBound = data.GetUpperBound(0) + 1;
            int hourBound = data.GetUpperBound(1) + 1;
            int minBound = data.GetUpperBound(2) + 1;

            double[][][] dataz = new double[dayBound][][];

            for (int d = 0; d < dayBound; d++)
            {
                dataz[d] = new double[hourBound][];
                for (int h = 0; h < hourBound; h++)
                {
                    dataz[d][h] = new double[minBound];
                    for (int m = 0; m < minBound; m++)
                    {
                        dataz[d][h][m] = data[d, h, m];
                    }
                }
            }

            output.Append(jsonSerializer.Serialize(dataz));

            output.AppendLine("}");

            return output.ToString();
        }

        /// <summary>
        /// Converts a 2 dimensional data set to a JSON Literal containing the Month, year and specificed key from the DataDictionary
        /// </summary>
        /// <param name="key">The key to use in the JSON Literal</param>
        /// <param name="data">The 2 dimensional data to serialize</param>
        /// <returns></returns>
        public string JsonEncode(string key, double[,] data)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine("{");
            output.AppendLine("'Year'" + ":" + yearInReview + ",");
            output.AppendLine("'Month'" + ":" + monthInReview + ",");
            output.AppendLine(" '" + key + "' " + ": ");

            int dayBound = data.GetUpperBound(0) + 1;
            int hourBound = data.GetUpperBound(1) + 1;            

            double[][] dataz = new double[dayBound][];

            for (int d = 0; d < dayBound; d++)
            {
                dataz[d] = new double[hourBound];
                for (int h = 0; h < hourBound; h++)
                {
                    dataz[d][h] = data[d, h];
                }
            }

            output.Append(jsonSerializer.Serialize(dataz));

            output.AppendLine("}");

            return output.ToString();
        }

        #endregion

        #region Report Methods

        public string AverageWeeklyReport(string key)
        {
            return JsonEncode(key, ConvertTo3D(GetDataSet(key)));
        }
       
        public string AverageWeeklyReportMissingData(string key)
        {            
            return JsonEncode(key, ConvertTo3D(GetMissingDataFromDataSet(GetDataSet(key))));
        }

        public string AverageHourlyReport(string key)
        {
            return JsonEncode(key, ConvertTo2D(ConvertTo3D(GetDataSet(key))));
        }

        public string AverageHourlyReportMissingData(string key)
        {
            return JsonEncode(key, ConvertTo2D(ConvertTo3D(GetMissingDataFromDataSet(GetDataSet(key)))));
        }

        public string TotalMonthlyReport(string key)
        {
            return JsonEncode(key, GetDataSet(key));
        }

        public string TotalMonthlyReportMissingData(string key)
        {
            return JsonEncode(key, GetMissingDataFromDataSet(GetDataSet(key)));
        }

        public string DailyTotalReport(string key, int dayofmonth)
        {
            return JsonEncode(key + " - Total " + dayofmonth.ToString(), ConvertTo3D(GetDataSet(key), dayofmonth));
        }

        public string DailyTotalReport(string key, int dayofmonth, int hour)
        {
            return JsonEncode(key + " - Total " + dayofmonth.ToString() + " @ " + hour.ToString(), ConvertTo3D(GetDataSet(key), dayofmonth, hour));
        }      

        public string DailyTotalReportMissingData(string key, int dayofmonth)
        {
            return JsonEncode(key + " - Daily Total Missing Data " + dayofmonth.ToString(), ConvertTo2D(ConvertTo3D(GetMissingDataFromDataSet(GetDataSet(key)), dayofmonth)));
        }

        public string DailyTotalReportMissingData(string key, int dayofmonth, int hour)
        {
            return JsonEncode(key + " - Daily Total Missing Data " + dayofmonth.ToString() + " @ " + hour.ToString(), ConvertTo2D(ConvertTo3D(GetMissingDataFromDataSet(GetDataSet(key)), dayofmonth, hour)));
        }       

        public string DailyAverageReport(string key, DayOfWeek day)
        {
            return JsonEncode(key + " - Average " + day.ToString(), ConvertTo3D(GetDataSet(key), day));
        }

        public string DailyAverageReport(string key, DayOfWeek day, int hour)
        {
            return JsonEncode(key + " - Average " + day.ToString() + " @ " + hour.ToString() , ConvertTo3D(GetDataSet(key), day, hour));
        }

        public string DailyAverageReportMissingData(string key, DayOfWeek day)
        {
            return JsonEncode(key + " - Average Missing Data " + day.ToString(), ConvertTo2D(ConvertTo3D(GetMissingDataFromDataSet(GetDataSet(key))), day));
        }

        public string DailyAverageReportMissingData(string key, DayOfWeek day, int hour)
        {
            return JsonEncode(key + " - Average Missing Data  " + day.ToString() + " @ " + hour.ToString(), ConvertTo2D(ConvertTo3D(GetMissingDataFromDataSet(GetDataSet(key))), day, hour));
        }

        #endregion
    }
}
