﻿using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Generic.LightDataTable.Helper;

namespace Generic.LightDataTable
{
    public class LightDataRowCollection : List<LightDataTableRow>
    {
        public LightDataRowCollection() { }

        public LightDataRowCollection DistinctByColumn(string columnName, Predicate<LightDataTableRow> func = null)
        {
            var r = new LightDataRowCollection(this.GroupBy(x => x[columnName]).Select(x => x.First()).ToList());
            if (r.Any() && func != null)
                r = new LightDataRowCollection(r.FindAll(func));

            return r;
        }
        [JsonConstructor]
        public LightDataRowCollection(List<LightDataTableRow> list)
        {
            if (list != null)
                this.AddRange(list);
        }

        public LightDataRowCollection(IOrderedEnumerable<LightDataTableRow> list)
        {
            if (list != null)
                this.AddRange(list);
        }

        public LightDataRowCollection(IEnumerable<LightDataTableRow> enumerable)
        {
            if (enumerable?.Any() ?? false)
                this.AddRange(enumerable.ToList());
        }

        public LightDataTable ToTable()
        {
            return new LightDataTable(this);
        }

        public LightDataRowCollection Copy(ColumnsCollections<string> cols, ColumnsCollections<int> colsIndex)
        {
            return new LightDataRowCollection(this.Select(x => new LightDataTableRow(x, cols, colsIndex)).ToList());
        }

        public IList ToObject(Type type)
        {
            var tType = MethodHelper.GetActualType(type);
            var baseListType = typeof(List<>);
            var listType = baseListType.MakeGenericType(tType);
            var iList = Activator.CreateInstance(listType) as IList;
            foreach (var item in this)
            {
                iList?.Add(item.ToObject(tType));
            }
            return iList;
        }

        public List<T> ToObject<T>()
        {
            var tType = MethodHelper.GetActualType(typeof(T));
            var baseListType = typeof(List<>);
            var listType = baseListType.MakeGenericType(tType);
            var iList = Activator.CreateInstance(listType) as IList;
            foreach (var item in this)
            {
                iList?.Add(item.ToObject(tType));
            }
            return (List<T>)iList;
        }

    }
}
