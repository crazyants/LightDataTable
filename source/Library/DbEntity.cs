using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FastDeepCloner;
using Newtonsoft.Json;
using PropertyChanged;
using Generic.LightDataTable.Attributes;
using Generic.LightDataTable.Helper;
using Generic.LightDataTable.InterFace;
using System.Runtime.Serialization;

namespace Generic.LightDataTable.Library
{
    [AddINotifyPropertyChangedInterface]
    public class DbEntity : IDbEntity
    {
        [JsonIgnore]
        [ExcludeFromAbstract]
        [DoNotNotify]
        [FastDeepClonerIgnore]
        public Dictionary<string, object> PropertyChanges { get; private set; }

        [PrimaryKey]
        public virtual long Id { get; set; }

        [ExcludeFromAbstract]
        [DoNotNotify]
        [JsonIgnore]
        // This property should only be used from Internal or be user in IDbRuleTrigger Interface
        public virtual ItemState State { get; set; }

        public DbEntity() => ClearPropertChanges();

        /// <summary>
        /// Clear all changes
        /// </summary>
        /// <returns></returns>
        public IDbEntity ClearPropertChanges()
        {
            PropertyChanges = new Dictionary<string, object>();
            return this;
        }

        protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanges == null || PropertyChanges.ContainsKey(e.PropertyName)) return;
            PropertyChanges.Add(e.PropertyName, e.PropertyName);
        }

        // This Method is added in case we want JsonConverter to serialize only new data, 
        // be sure to Clear PropertyChanges before beginning to change the data
        public string GetJsonForPropertyChangesOnly()
        {
            return MethodHelper.CreateNewValuesFromObject(this);
        }

        public IDbEntity Clone(FieldType fieldType = FieldType.PropertyInfo)
        {
            return DeepCloner.Clone(this, new FastDeepClonerSettings()
            {
                FieldType = fieldType,
                OnCreateInstance = new Extensions.CreateInstance(FormatterServices.GetUninitializedObject)
            });
        }

        /// <summary>
        /// Merge only the Changes, that exist in PropertyChanges
        /// </summary>
        /// <param name="data">data to be merged</param>
        public virtual void Merge(InterFace.IDbEntity data)
        {
            foreach (var prop in DeepCloner.GetFastDeepClonerProperties(data.GetType()).Where(x => (!x.PropertyType.IsClass || x.PropertyType == typeof(string)) && !x.ContainAttribute<ExcludeFromAbstract>()))
            {
                var thisProp = this.GetType().GetProperty(prop.Name);
                if (thisProp == null || PropertyChanges.ContainsKey(thisProp.Name))
                    continue;
                thisProp.SetValue(this, prop.GetValue(data));
            }
        }
    }
}
