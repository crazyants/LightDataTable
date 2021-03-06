﻿using Newtonsoft.Json;
using System;
using System.Data.SqlTypes;
using System.Globalization;

namespace Generic.LightDataTable
{
    public class LightDataTableShared
    {
        public CultureInfo Culture = new CultureInfo("en");
        [JsonProperty(Order = 1)]
        public RoundingSettings RoundingSettings { get; set; }
        [JsonProperty(Order = 2)]
        public ColumnsCollections<string> Columns { get; protected set; }
        [JsonProperty(Order = 3)]
        public ColumnsCollections<int> ColumnsWithIndexKey { get; protected set; }
        private int _columnLength = 0;// to speed up things
        [JsonProperty(Order = 0)]
        public int ColumnLength
        {
            get { return _columnLength; }
            set { _columnLength = value; }
        }

        public bool IgnoreTypeValidation;

        public LightDataTableShared(CultureInfo cultureInfo)
        {
            Columns = new ColumnsCollections<string>();
            ColumnsWithIndexKey = new ColumnsCollections<int>();
            RoundingSettings = new RoundingSettings();
            ValidateCulture();
            Culture = cultureInfo ?? new CultureInfo("en");
        }

        public LightDataTableShared()
        {
            Columns = new ColumnsCollections<string>();
            ColumnsWithIndexKey = new ColumnsCollections<int>();
            RoundingSettings = new RoundingSettings();
            ValidateCulture();
            Culture = new CultureInfo("en");
        }


        protected object ValueByType(Type propertyType, object defaultValue = null)
        {
            if (defaultValue != null)
            {
                var typeOne = propertyType;
                var typeTwo = defaultValue.GetType();
                if (Nullable.GetUnderlyingType(typeOne) != null)
                    typeOne = Nullable.GetUnderlyingType(typeOne);
                if (Nullable.GetUnderlyingType(typeTwo) != null)
                    typeTwo = Nullable.GetUnderlyingType(typeTwo);

                if (typeOne == typeTwo)
                    return defaultValue;
            }
            if (propertyType == typeof(int?))
                return new int?();
            if (propertyType == typeof(int))
                return 0;

            if (propertyType == typeof(Int64?))
                return new Int64?();
            if (propertyType == typeof(Int64))
                return 0;

            if (propertyType == typeof(decimal?))
                return new decimal?();

            if (propertyType == typeof(decimal))
                return new decimal();

            if (propertyType == typeof(double?))
                return new double?();

            if (propertyType == typeof(double))
                return new double();


            if (propertyType == typeof(DateTime?))
                return new DateTime?();

            if (propertyType == typeof(DateTime))
                return SqlDateTime.MinValue.Value;

            if (propertyType == typeof(bool?))
                return new bool?();

            if (propertyType == typeof(bool))
                return false;

            if (propertyType == typeof(TimeSpan?))
                return new TimeSpan?();

            if (propertyType == typeof(TimeSpan))
                return new TimeSpan();

            if (propertyType == typeof(byte[]))
                return new byte[0];

            if (propertyType == typeof(string))
                return string.Empty;
            return null;

        }

        internal void ValidateCulture()
        {
            try
            {
                if (System.Threading.Thread.CurrentThread.CurrentCulture.Name != Culture.Name) // vi behöver sätta det första gången bara. detta snabbar upp applikationen ta inte bort detta.
                    System.Threading.Thread.CurrentThread.CurrentCulture = Culture;
            }
            catch
            {
                //Ignore
            }
        }

        protected void TypeValidation(ref object value, Type dataType, bool loadDefaultOnError, object defaultValue = null)
        {
            try
            {
                ValidateCulture();
                if (value == null || value.GetType() == typeof(System.DBNull))
                {
                    value = ValueByType(dataType, defaultValue);
                    return;
                }

                if (IgnoreTypeValidation)
                    return;

                if (dataType == typeof(int?) || dataType == typeof(int))
                {
                    if (double.TryParse(CleanValue(dataType, value).ToString(), NumberStyles.Float, Culture, out var douTal))
                        value = Convert.ToInt32(douTal);
                    else
                        if (loadDefaultOnError)
                        value = ValueByType(dataType, defaultValue);

                    return;
                }

                if (dataType == typeof(Int64?) || dataType == typeof(Int64))
                {
                    if (double.TryParse(CleanValue(dataType, value).ToString(), NumberStyles.Float, Culture, out var douTal))
                        value = Convert.ToInt64(douTal);
                    else
                    if (loadDefaultOnError)
                        value = ValueByType(dataType, defaultValue);

                    return;
                }

                if (dataType == typeof(decimal?) || dataType == typeof(decimal))
                {
                    if (decimal.TryParse(CleanValue(dataType, value).ToString(), NumberStyles.Float, Culture, out var decTal))
                        value = RoundingSettings.Round(decTal);
                    else
                    if (loadDefaultOnError)
                        value = ValueByType(dataType, defaultValue);
                    return;

                }

                if (dataType == typeof(double?) || dataType == typeof(double))
                {
                    if (double.TryParse(CleanValue(dataType, value).ToString(), NumberStyles.Float, Culture, out var douTal))
                        value = RoundingSettings.Round(douTal);
                    else
                    if (loadDefaultOnError)
                        value = ValueByType(dataType, defaultValue);
                    return;

                }

                if (dataType == typeof(DateTime?) || dataType == typeof(DateTime))
                {
                    if (DateTime.TryParse(value.ToString(), Culture, DateTimeStyles.None, out var dateValue))
                        value = dateValue;
                    else
                    if (loadDefaultOnError)
                        value = ValueByType(dataType, defaultValue);
                    return;

                }

                if (dataType == typeof(bool?) || dataType == typeof(bool))
                {
                    if (bool.TryParse(value.ToString(), out var boolValue))
                        value = boolValue;
                    else
                    if (loadDefaultOnError)
                        value = ValueByType(dataType, defaultValue);
                    return;
                }

                if (dataType == typeof(TimeSpan?) || dataType == typeof(TimeSpan))
                {
                    if (TimeSpan.TryParse(value.ToString(), Culture, out var timeSpanValue))
                        value = timeSpanValue;
                    else
                    if (loadDefaultOnError)
                        value = ValueByType(dataType, defaultValue);
                    return;


                }

                if (dataType.IsEnum)
                {
                    if (value.GetType() == typeof(int))
                    {
                        Enum.IsDefined(dataType, value);
                    }
                    else if (Enum.IsDefined(dataType, value))
                        value = Enum.Parse(dataType, value.ToString(), true);
                }
                else if (dataType == typeof(string))
                {
                    value = value.ToString();

                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error: InvalidType. ColumnType is " + dataType.FullName + " and the given value is of type " + value.GetType().FullName + " Orginal Exception " + ex.Message);

            }
        }


        private object CleanValue(Type valueType, object value)
        {
            if ((valueType == typeof(decimal) || valueType == typeof(double)) || (valueType == typeof(decimal?) || valueType == typeof(double?)))
            {
                value = Culture.NumberFormat.NumberDecimalSeparator == "." ? value.ToString().Replace(",", ".") : value.ToString().Replace(".", ",");
                value = System.Text.RegularExpressions.Regex.Replace(value.ToString(), @"\s", "");
                return value;

            }
            return value;
        }

        private T ConvertValueToType<T>(object value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }

        public LightDataTableRow NewRow(CultureInfo cultureInfo = null)
        {
            var row = new LightDataTableRow(ColumnLength, Columns, ColumnsWithIndexKey, cultureInfo ?? Culture)
            {
                RoundingSettings = this.RoundingSettings
            };
            return row;
        }

    }
}
