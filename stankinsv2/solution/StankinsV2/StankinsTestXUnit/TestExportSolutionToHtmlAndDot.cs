﻿using FluentAssertions;
using Stankins.FileOps;
using Stankins.HTML;
using Stankins.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Stankins.AnalyzeSolution;
using Xbehave;
using Xunit;
using static System.Environment;
using Stankins.SimpleRecipes;
using System.Diagnostics;

namespace StankinsTestXUnit
{
    [Trait("ReceiverFromSolution", "")]
    [Trait("ExternalDependency", "0")]
    public class TestExportSolutionToHtmlAndDot
    {
        [Scenario]
        [Example(@"Assets/TestSolutionAnalyzer/TestSolutionAnalyzer.sln", 7)]
        public void TestSimpleSln(string filepath,int numberTables)
        {

            IDataToSent data=null;
            string fileName= Guid.NewGuid().ToString("N");

            $"receiving the file {filepath} ".w(async () =>
            {
                data= await new ExportSolutionToHtmlAndDot(filepath,fileName).TransformData(null);
            });
            $"Then should be a data".w(() => data.Should().NotBeNull());
            $"With {numberTables} tables".w(() =>
            {
                data.DataToBeSentFurther.Should().NotBeNull();
                data.DataToBeSentFurther.Count.Should().Be(numberTables);
            });
            $"And the file should exists".w(() => File.Exists(fileName));
            //$"and run".w(() => Process.Start("notepad.exe", fileName));

        }
    }
}
