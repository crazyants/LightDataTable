using System;
using System.Globalization;
using System.Linq.Expressions;

namespace Generic.LightDataTable.Interface
{
    public interface ILightDataTable
    {
        string TablePrimaryKey { get; set; }

        int TotalPages { get; }

        ColumnsCollections<string> Columns { get; }

        ColumnsCollections<int> ColumnsWithIndexKey { get; }

        ILightDataTable AddColumn(string columnName, string displayName, Type dataType = null, object defaultValue = null);

        ILightDataTable AddColumn(string columnName, Type dataType = null, object defaultValue = null);

        T FindByPrimaryKey<T>(object primaryKeyValue);

        LightDataTableRow AddRow(object[] values);

        LightDataTableRow AddRow(LightDataTableRow row);

        ILightDataTable RemoveColumn<T, P>(Expression<Func<T, P>> action) where T : class;

        void MergeByPrimaryKey(LightDataTableRow rowToBeMerged);

        LightDataTableRow NewRow(CultureInfo cultureInfo = null);

        void RemoveColumn(string columnName);

        void RemoveColumn(int columnIndex);

        ILightDataTable AssignValueToColumn(string columnName, object value);

        ILightDataTable MergeColumns(LightDataTable data2);

        ILightDataTable Merge(LightDataTable data2, bool mergeColumnAndRow = true);

        ILightDataTable OrderBy<T, P>(Expression<Func<T, P>> action, bool position) where T : class;

        ILightDataTable OrderBy(string columnName, bool position);

        LightDataRowCollection Rows { get; }

        /// <summary>
        /// T could be bool For Any
        /// T could be LightDataTableRow for the first found Item
        /// T could be List of LightDataTableRow found items
        /// T could be LightDataTable a Table result 
        /// T could also be LightDataRowCollection of found items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        T SelectMany<T>(Predicate<LightDataTableRow> func);
    }
}
