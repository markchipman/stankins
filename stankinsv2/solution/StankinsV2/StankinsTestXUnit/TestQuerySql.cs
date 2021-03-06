﻿using System;
using System.Data.SqlClient;
using System.IO;
using FluentAssertions;
using Stankins.Interfaces;
using Stankins.SimpleRecipes;
using Stankins.SqlServer;
using StankinsReceiverDB;
using Xbehave;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace StankinsTestXUnit
{
    [Trait("ExternalDependency", "SqlServer")]
    public class TestQuerySql
    {
        public static IEnumerable<object[]> ExportTableToExcelSqlData()
        {

            return new List<object[]>
            {
                new object[] { TestReceiveDatabasesSql.SqlConnection, "sys.tables", "a.xlsx" }
            };
        }
        
        public static IEnumerable<object[]> SelectFromDbData()
        {

            return new List<object[]>
            {
                new object[] { TestReceiveDatabasesSql.SqlConnection, "select 234", 1 }
            };
        }
        [Scenario]
        [Trait("ReceiveQueryFromDatabaseSql", "")]
        [MemberData(nameof(SelectFromDbData))]
        public void SelectFromDb(string connectionString, string select, int nrRows)
        {
            IReceive status = null;

            IDataToSent data = null;
            $"Assume Sql Server instance {connectionString} exists , if not see docker folder".w(() => {

            });
            $"When I create the ReceiveQueryFromDatabaseSql ".w(() => status = new ReceiveQueryFromDatabaseSql(connectionString, select));
            $"and receive data".w(async () =>
            {
                data = await status.TransformData(null);
            });
            $"the data should have a table".w(() =>
            {
                data.DataToBeSentFurther.Count.Should().Be(1);
            });


            $"should be {nrRows} rows".w(() => { data.DataToBeSentFurther[0].Rows.Count.Should().Be(nrRows); });



        }
         [Scenario]
        [Trait("DBReceiverStatement", "")]
        //[Example("Server=(local);Database=msdb;User Id=SA;Password = <YourStrong!Passw0rd>;", "select 234", 1)]
        [MemberData(nameof(SelectFromDbData))]
        public void SelectFromDbReceiver(string connectionString, string select, int nrRows)
        {
            IReceive status = null;
            string connectionType=typeof(SqlConnection).AssemblyQualifiedName;
            
            IDataToSent data = null;
            $"Assume Sql Server instance {connectionString} exists , if not see docker folder".w(() => {

            });
            $"When I create the DBReceiverStatement ".w(() => status = new DBReceiverStatement(connectionString,connectionType, select));
            $"and receive data".w(async () =>
            {
                data = await status.TransformData(null);
            });
            $"the data should have a table".w(() =>
            {
                data.DataToBeSentFurther.Count.Should().Be(1);
            });


            $"should be {nrRows} rows".w(() => { data.DataToBeSentFurther[0].Rows.Count.Should().Be(nrRows); });



        }

        [Scenario]
        [Trait("ExportTableToExcelSql", "")]
        [MemberData(nameof(ExportTableToExcelSqlData))]
        //[Example("Server=(local);Database=msdb;User Id=SA;Password = <YourStrong!Passw0rd>;", "sys.tables", "a.xlsx")]
        public void ExportTableToExcelSql(string connectionString, string tableName, string fileName)
        {
            IReceive status = null;

            IDataToSent data = null;
            fileName = Guid.NewGuid().ToString("N") + fileName;

            
            $"the Excel {fileName} should not exists".w(() => File.Exists(fileName).Should().BeFalse());
            $"Assume Sql Server instance {connectionString} exists , if not see docker folder".w(() => { });

            $"When I create the ExportTableToExcelSql ".w(() =>
                status = new ExportTableToExcelSql(connectionString, tableName, fileName));
            $"and receive data".w(async () => { data = await status.TransformData(null); });
            $"the data should have a table".w(() =>
            {
                data.DataToBeSentFurther.Count.Should().Be(3);
                //table + 2 outputs
            });


            
            $"the Excel {fileName} should  exists".w(() => File.Exists(fileName).Should().BeTrue());



        }
    }
}