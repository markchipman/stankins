﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StankinsHelperCommands;
using StankinsObjects;

namespace Stankins.Interpreter
{
    public class ValidationResultWarning:ValidationResult
    {
        public ValidationResultWarning(ValidationResult vr):base(vr)
        {

        }
    }
    public class InterpretFromType:IInterpreter
    {
        List<ValidationResult> valid=new List<ValidationResult>();
        /// <summary>
        /// ReceiveTableDatabaseSql connectionString="Server=(local);Database=test;User Id=SA;Password = <YourStrong!Passw0rd>;" nameTable=department
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string[] SplitString(string data)
        {
            var result=new List<string>();
            var res= data.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for(int i = 0; i < res.Length; i++)
            {
                string item=res[i];
                if (item.Replace("\"","").Length == item.Length-2)
                {
                    item = item.Replace("\"","");
                }
                else
                {
                    if (res[i].Contains("\"") && (i + 1 < res.Length))
                    {
                        while ((i + 1 < res.Length) && (!res[i + 1].EndsWith("\"")))
                        {
                            item += ' ' + res[i + 1];
                            i++;
                        }
                        item += res[i + 1];
                        i++;
                        item = item.Replace("\"", "");

                    }
                }
                result.Add(item);
            }
            return result.ToArray();
        }
        public bool CanInterpretString(string data)
        {
            var all = FindAssembliesToExecute.AddReferences(new FindAssembliesToExecute(null).FromType(typeof(RecipeFromFilePath)));
            var instr = SplitString(data);
            var name = instr[0];
            ObjectType = all.FirstOrDefault(it => 
            string.Equals(it.Name, name, StringComparison.InvariantCultureIgnoreCase));
            if(ObjectType == null)
            {
                ObjectType = all.FirstOrDefault(it => 
                 string.Equals(it.Type.FullName,name,StringComparison.InvariantCultureIgnoreCase)
                 );
           
            }
            if (ObjectType == null)
            {
                valid.Add(new ValidationResult($"cannot find object {name}"));
                return false;
            }

           if(instr.Length -1 != ObjectType.ConstructorParam.Keys.Count)
           {                
               valid.Add(new ValidationResultWarning(new ValidationResult($"for {name} constructor items length different:{instr.Length -1} {ObjectType.ConstructorParam.Keys.Count}")));
           }
           var keys = ObjectType.ConstructorParam.Keys;
           for (int i = 1; i < instr.Length; i++)
           {
               var first = instr[i].IndexOf("=");
               var argumentName = instr[i].Substring(0, first );
               var argumentValue = instr[i].Substring(first + 1);
               var key = keys.FirstOrDefault(it =>
                   string.Equals(it, argumentName, StringComparison.InvariantCultureIgnoreCase));

               if(key==null)
               {
                   
                   valid.Add(new ValidationResult($"key not found:{argumentName}"));
               }
               else
               {
                   ObjectType.ConstructorParam[key] = argumentValue;
               }

           }

           var warning=typeof(ValidationResultWarning).FullName;
            
           return (valid.Count(it=>it.GetType().FullName!=warning) == 0);

        }

        public ResultTypeStankins ObjectType { get; private set; }

        public BaseObject ObjectToRun()
        {
            return ObjectType.Create(ObjectType.ConstructorParam.Values.Select(it => it).ToArray());
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return valid.ToArray();
        }
    }
}