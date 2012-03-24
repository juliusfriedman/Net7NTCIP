using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASTITransportation.Traffic
{
    public class DataBin
    {
        #region Fields

        const string IdProperty = "Id";
        const string BinTypeProperty = "Type";
        const string BinValueProperty = "Value";
        const string DescriptionProperty = "Description";

        Dictionary<string, object> properties = new Dictionary<string, object>()
        {
            {IdProperty, Guid.NewGuid()},
            {BinTypeProperty, DataBinType.Unknown},
            {BinValueProperty, 0.0D},
            {DescriptionProperty, string.Empty}
        };

        #endregion

        #region Properties

        public Guid Id
        {
            get { return (Guid)this[IdProperty]; }
            set { this[IdProperty] = value; }
        }

        public string Description
        {
            get { return this[DescriptionProperty].ToString(); }
            set { this[DescriptionProperty] = value; }
        }

        public DataBinType BinType
        {
            get { return (DataBinType)this[BinTypeProperty]; }
            set { this[BinTypeProperty] = value; }
        }

        public double BinValue
        {
            get { return (double)this[BinValueProperty]; }
            set { this[BinValueProperty] = value; }
        }

        public Object this[string key]
        {
            get
            {
                object value = null;
                if(!string.IsNullOrEmpty(key)) properties.TryGetValue(key, out value);
                return value;
            }
            set
            {
                if (!string.IsNullOrEmpty(key)) return;
                lock (properties)
                {
                    if (properties.ContainsKey(key)) properties[key] = value;
                    else properties.Add(key, value);
                }
            }
        }

        #endregion

        #region Methods

        public void IncrementValue()
        {
            ++BinValue;
        }

        public void DecrementValue()
        {
            --BinValue;
        }

        #endregion

        #region Operators

        public static bool operator ==(DataBin a, DataBin b)
        {
            return a.BinType.Equals(b.BinType) && a.BinValue.Equals(b.BinValue);
        }

        public static bool operator !=(DataBin a, DataBin b)
        {
            return !(a == b);
        }

        public static bool operator !(DataBin a)
        {
            return !((bool)a);
        }

        public static bool operator false(DataBin a)
        {
            return a.BinValue < 0;
        }

        public static bool operator true(DataBin a)
        {
            return a.BinValue > 0;
        }

        public static DataBin operator +(DataBin a, DataBin b)
        {
            DataBin laneBin = new DataBin();
            if (!a.BinType.Equals(b.BinType)) return laneBin;
            laneBin.BinType = a.BinType;
            
            laneBin.BinValue = a.BinValue + b.BinValue;

            if (a.Description.Equals(b.Description)) laneBin.Description = a.Description;
            else laneBin.Description = string.Concat(a.Description, "+", b.Description);
            
            return laneBin;
        }

        public static DataBin operator -(DataBin a, DataBin b)
        {
            DataBin laneBin = new DataBin();
            if (!a.BinType.Equals(b.BinType)) return laneBin;
            
            laneBin.BinType = a.BinType;

            laneBin.BinValue = a.BinValue - b.BinValue;

            if (a.Description.Equals(b.Description)) laneBin.Description = a.Description;
            else laneBin.Description = string.Concat(a.Description, "-", b.Description);
            
            return laneBin;
        }

        public static DataBin operator *(DataBin a, DataBin b)
        {
            DataBin laneBin = new DataBin();
            if (!a.BinType.Equals(b.BinType)) return laneBin;

            laneBin.BinType = a.BinType;

            laneBin.BinValue = a.BinValue * b.BinValue;

            if (a.Description.Equals(b.Description)) laneBin.Description = a.Description;
            else laneBin.Description = string.Concat(a.Description, "*", b.Description);

            return laneBin;
        }

        public static DataBin operator /(DataBin a, DataBin b)
        {
            DataBin laneBin = new DataBin();
            if (!a.BinType.Equals(b.BinType)) return laneBin;

            laneBin.BinType = a.BinType;

            laneBin.BinValue = a.BinValue / b.BinValue;

            if (a.Description.Equals(b.Description)) laneBin.Description = a.Description;
            else laneBin.Description = string.Concat(a.Description, "/", b.Description);

            return laneBin;
        }

        public static implicit operator double(DataBin a)
        {
            return a.BinValue;
        }

        public static implicit operator float(DataBin a)
        {
            return (float)a.BinValue;
        }

        public static implicit operator bool(DataBin a)
        {
            return a.BinValue > 0;
        }

        public static implicit operator string(DataBin a)
        {
            return a.ToString();
        }

        public override bool Equals(object obj)
        {
            try
            {
                return ((bool) (this == (DataBin) obj));
            }
            catch
            {
                return false;
            }
        }

        public override string ToString()
        {
            return Description;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
