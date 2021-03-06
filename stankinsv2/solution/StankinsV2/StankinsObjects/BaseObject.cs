﻿using Stankins.Interfaces;
using StankinsCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StankinsObjects
{
    public abstract class BaseObjectSender : BaseObject, ISenderToOutput
    {



        protected BaseObjectSender(CtorDictionary dataNeeded) : base(dataNeeded)
        {
        }

        public string InputTemplate { get; set; }
        public DataTableString OutputString { get; set; }


        public DataTableByte OutputByte { get; set; }

        public void CreateOutputIfNotExists(IDataToSent receiveData)
        {
            try
            {
                OutputString = receiveData.FindAfterName("OutputString").Value as DataTableString;
            }
            catch
            {
                OutputString = new DataTableString
                {
                    TableName = "OutputString"
                };
                FastAddTable(receiveData, OutputString);
            }
            try
            {
                OutputByte = receiveData.FindAfterName("OutputByte").Value as DataTableByte;
            }
            catch
            {
                OutputByte = new DataTableByte
                {
                    TableName = "OutputByte"
                };
                FastAddTable(receiveData, OutputByte);
            }
        }

        protected string FullFileNameFromPath(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    return fileName;
                }

                string pathDll = Assembly.GetEntryAssembly().Location;
                string path = Path.GetDirectoryName(pathDll);
                string f = Path.Combine(path, fileName);
                if (File.Exists(f))
                {
                    return f;
                }
            }
            catch (Exception)
            {
                //TODO:log
            }
            return null;
        }
        protected string ReadFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                return File.ReadAllText(fileName);
            }

            string pathDll = Assembly.GetEntryAssembly().Location;
            string path = Path.GetDirectoryName(pathDll);
            string f = Path.Combine(path, fileName);
            return File.ReadAllText(f);
        }
    }
    public abstract class BaseObject : IBaseObject
    {

        //TODO: maybe reflection to get properties values from dataNeeded?
        public BaseObject(CtorDictionary dataNeeded)
        {
            Version = new Version(1, 0, 0, 0);
            //TODO: read this from somewhere
            StoringDataBetweenCalls = new Dictionary<string, object>();
            this.dataNeeded = dataNeeded;
            //this.Name = this.GetType().Name;
        }
        public readonly CtorDictionary dataNeeded;
        public string Name { get; set; }
        public IDictionary<string, object> StoringDataBetweenCalls { get; set; }
        protected int[] FastAddTables(IDataToSent receiveData, params DataTable[] items)
        {
            if (items?.Length < 0)
            {
                return null;
            }

            int[] res = new int[items.Length];
            int i = 0;
            foreach (DataTable dt in items)
            {

                res[i++] = FastAddTable(receiveData, dt);
            }

            return res;

        }
        protected int FastAddTable(IDataToSent receiveData, DataTable dt)
        {
            int id = receiveData.AddNewTable(dt);
            receiveData.Metadata.AddTable(dt, id);
            return id;

        }
        //todo : this should stay into IDataToSent
        public IEnumerable<KeyValuePair<int, DataTable>> FindTableAfterColumnName(string nameColumn, IDataToSent receiveData)
        {

            int[] cols = receiveData.Metadata.Columns
                .Where(it => string.Equals(nameColumn, it.Name))
                .Select(it => it.IDTable)
                .ToArray();
            Dictionary<int, DataTable> tables = receiveData.DataToBeSentFurther;
            foreach (int i in tables.Keys)
            {
                if (!cols.Contains(i))
                {
                    continue;
                }

                yield return new KeyValuePair<int, DataTable>(i, tables[i]);
            }

        }
        public Version Version { get; }

        protected T GetMyDataOrDefault<T>(string name, T def)
        {
            if (dataNeeded == null)
            {
                return def;
            }

            name = name?.ToLowerInvariant();

            if (!dataNeeded.ContainsKey(name))
            {
                return def;
            }

            T ret;
            try
            {
                ret = (T)dataNeeded[name];
            }
            catch (InvalidCastException)
            {
                ret = (T)Convert.ChangeType(dataNeeded[name], typeof(T));
            }
            if (typeof(T).IsClass && object.Equals(ret, default(T)))
            {

                return def;
            }

            return ret;



        }
        protected T GetMyDataOrThrow<T>(string name)
        {
            if (dataNeeded == null)
            {
                throw new ArgumentException($"{nameof(dataNeeded)} is null", nameof(dataNeeded));
            }

            name = name?.ToLowerInvariant();
            if (!dataNeeded.ContainsKey(name))
            {
                throw new ArgumentException($"{nameof(dataNeeded)} does not contain {name}", name);
            }

            return (T)dataNeeded[name];
        }

        public abstract Task<IDataToSent> TransformData(IDataToSent receiveData);
        public abstract Task<IMetadata> TryLoadMetadata();

    }
}
