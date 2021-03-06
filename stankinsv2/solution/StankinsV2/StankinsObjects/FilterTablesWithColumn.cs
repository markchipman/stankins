﻿using StankinsCommon;
using Stankins.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StankinsObjects 
{

    public class FilterTablesWithColumn : BaseObject, IFilter
    {
        public FilterTablesWithColumn(string nameColumnToKeep) : this(new CtorDictionary() {
            { nameof(nameColumnToKeep ), nameColumnToKeep }
            }
            )
        {

        }
        public FilterTablesWithColumn(CtorDictionary dataNeeded) : base(dataNeeded)
        {
            this.NameColumnToKeep = base.GetMyDataOrThrow<string>(nameof(NameColumnToKeep));
            this.Name = nameof(FilterTablesWithColumn);
        }

        public string NameColumnToKeep { get; }
        public override async Task<IDataToSent> TransformData(IDataToSent receiveData)
        {
            
            var IdTablesWithColumn = receiveData
                .Metadata
                .Columns
                .Where(it => string.Equals(it.Name, NameColumnToKeep, StringComparison.CurrentCultureIgnoreCase))
                .Select(it => it.IDTable)
                .Distinct()
                .ToArray();


            var idsTablesToDelete = receiveData.DataToBeSentFurther
                .Where(it => !IdTablesWithColumn.Contains(it.Key))
                .Select(it => it.Key)
                .ToList();
            idsTablesToDelete.ForEach(it => receiveData.DataToBeSentFurther.Remove(it));
            for (int i = receiveData.Metadata.Tables.Count - 1; i >= 0; i--)
            {
                var t = receiveData.Metadata.Tables[i];
                if (idsTablesToDelete.Contains(t.Id))
                    receiveData.Metadata.Tables.Remove(t);
            }

            

            for (int i = receiveData.Metadata.Columns.Count - 1; i >= 0; i--)
            {
                if (!IdTablesWithColumn.Contains(receiveData.Metadata.Columns[i].IDTable))
                    receiveData.Metadata.Columns.RemoveAt(i);
            }
            return await Task.FromResult(receiveData) ;       
        }

        public override Task<IMetadata> TryLoadMetadata()
        {
            throw new NotImplementedException();
        }
    }
}
