﻿using Stankins.Interfaces;
using StankinsCommon;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace StankinsObjects 
{
    //TODO: make remove more columns at once
    public class FilterRemoveColumn : BaseObject, IFilter
    {
        public string NameColumn { get; }
        public FilterRemoveColumn(string nameColumn) : this(new CtorDictionary() {
            { nameof(nameColumn), nameColumn }
            }
              )
        {

        }
        public FilterRemoveColumn(CtorDictionary dataNeeded) : base(dataNeeded)
        {
            this.NameColumn = base.GetMyDataOrThrow<string>(nameof(NameColumn));
            this.Name = nameof(FilterRemoveColumn);
        }

        public async override Task<IDataToSent> TransformData(IDataToSent receiveData)
        {
            var tables = base.FindTableAfterColumnName(NameColumn, receiveData).ToArray();
            foreach (var tb in tables)
            {
                tb.Value.Columns.Remove(NameColumn);
            }
            //metadata
            var cols = receiveData.Metadata.Columns;
            for (int i = cols.Count - 1; i >= 0; i--)
            {
                if (!string.Equals(cols[i].Name, NameColumn))
                    continue;
                cols.RemoveAt(i);

            }
            return await Task.FromResult(receiveData) ;


        }

        public override Task<IMetadata> TryLoadMetadata()
        {
            throw new NotImplementedException();
        }
    }
}
