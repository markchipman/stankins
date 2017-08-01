﻿using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using StankinsInterfaces;
using StanskinsImplementation;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Transformers
{
    public class TransformRow
    {
        public string TheExpression { get; set; }
        public TransformRow(string expression)
        {
            TheExpression = expression;
        }
        public IRow[] valuesRead { get; set; }
        public IRow[] valuesTransformed { get; set; }

        public async Task Run()
        {
            var script = CSharpScript.Create<IRow>(TheExpression,
                globalsType: typeof(IRow),
                options:ScriptOptions.Default.AddReferences(
                    //todo: load at serialize time
                    typeof(RowRead).GetTypeInfo().Assembly
                    )
                );

            script.Compile();
            valuesTransformed = new IRow[valuesRead.Length];
            int i = 0;
            foreach (var item in valuesRead)
            {
                var state = await script.RunAsync(item);
                var returnValue = state.ReturnValue;
                valuesTransformed[i++] = returnValue;
            }
        }
    }
}