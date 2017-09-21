using System.Collections.Generic;
using FastDeepCloner;
using Generic.LightDataTable.Attributes;
using Generic.LightDataTable.Helper;

namespace Generic.LightDataTable.InterFace
{
    public interface IDbEntity
    {
        [PrimaryKey]
        long Id { get; set; }

        Dictionary<string, object> PropertyChanges { get; }
        // Merge tow objects, only unupdated data will be merged
        void Merge(InterFace.IDbEntity data);

        ItemState State { get; set; }
        //Clear all the changed property and begin new validations
        InterFace.IDbEntity ClearPropertChanges();

        /// <summary>
        /// Clone the whole object
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        IDbEntity Clone(FieldType fieldType = FieldType.PropertyInfo);
        // This method is added incase we want JsonConverter to serilize only new data, 
        // be sure to ClearPropertChanges before begning to change the data
        string GetJsonForPropertyChangesOnly();


    }
}
