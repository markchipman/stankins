﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Stankins.Interfaces;
using StankinsCommon;
using StankinsObjects;

namespace Stankins.Alive
{
    public abstract class AliveStatus : BaseObject, IReceive
    {
        public AliveStatus(CtorDictionary dataNeeded) : base(dataNeeded)
        {

        }

        protected DataTable CreateTable(IDataToSent dataToSent)
        {

            string nameTable = nameof(AliveStatus);
            try{
                return dataToSent.FindAfterName(nameTable).Value;
            }
            catch
            {
                //do nothing, create table is following
            }
            var m = new DataTable();
            m.TableName = nameTable;
            m.Columns.Add("Process",typeof(string));
            m.Columns.Add("Arguments", typeof(string));
            m.Columns.Add("To", typeof(string));            
            m.Columns.Add("IsSuccess", typeof(bool));
            m.Columns.Add("Result", typeof(string));
            m.Columns.Add("Duration", typeof(long));
            m.Columns.Add("DetailedResult", typeof(string));
            m.Columns.Add("Exception", typeof(string));
            m.Columns.Add("StartedDate", typeof(DateTime));
            dataToSent.AddNewTable(m);
            dataToSent.Metadata.AddTable(m, dataToSent.Metadata.Tables.Count);

            return m;
        }

        public override Task<IMetadata> TryLoadMetadata()
        {
            throw new NotImplementedException();

        }
        
        public static IEnumerable<AliveResult> FromTable(DataTable result)
        {
            foreach(DataRow row in result.Rows)
            {
                var m = new AliveResult();
                m.StartedDate =(DateTime)row[nameof(m.StartedDate)] ;
                m.Arguments = row[nameof(m.Arguments)]?.ToString();
                m.DetailedResult = row[nameof(m.DetailedResult)]?.ToString();
                var duration = row[nameof(m.Duration)]?.ToString();
                try
                {
                    m.Duration = (string.IsNullOrWhiteSpace(duration )) ? (int?)null : int.Parse(duration);
                }
                catch(Exception ex)
                {
                    string s = ex.Message;
                    System.Console.WriteLine(s);
                }
                m.Exception = row[nameof(m.Exception)]?.ToString();
                m.IsSuccess =bool.Parse( row[nameof(m.IsSuccess)].ToString());
                m.Process = row[nameof(m.Process)]?.ToString();
                m.Result = row[nameof(m.Result)]?.ToString();
                m.To = row[nameof(m.To)]?.ToString();
                yield return m;
                
            }
        }
    }
}
