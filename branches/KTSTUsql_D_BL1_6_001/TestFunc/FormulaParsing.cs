using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using ELW;
using ELW.Library.Math;
using ELW.Library.Math.Exceptions;
using ELW.Library.Math.Expressions;
using ELW.Library.Math.Tools;

namespace TestFunc
{
    class ClassFormulaParsing
    {
        public ClassFormulaParsing(string formula, ELW.Library.Math.Tools.VariableValue[] args)
        {
            try
            {
                // Compiling an expression
                PreparedExpression preparedExpression = ToolsHelper.Parser.Parse(formula);
                CompiledExpression compiledExpression = ToolsHelper.Compiler.Compile(preparedExpression);
                // Optimizing an expression
                CompiledExpression optimizedExpression = ToolsHelper.Optimizer.Optimize(compiledExpression);
                // Creating list of variables specified
                List<VariableValue> variables = new List<VariableValue>();

                foreach (VariableValue arg in args)
                    variables.Add(arg);                

                // Do it !
                double res = ToolsHelper.Calculator.Calculate(compiledExpression, variables);
                // Show the result.
                 Console.WriteLine(String.Format("Result: {0}\nOptimized: {1}", res, ToolsHelper.Decompiler.Decompile(optimizedExpression)));
            }
            catch (CompilerSyntaxException ex)
            {
                Console.WriteLine(String.Format("Compiler syntax error: {0}", ex.Message));
            }
            catch (MathProcessorException ex)
            {
                Console.WriteLine(String.Format("Error: {0}", ex.Message));
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Error in input data.");
            }
            catch (Exception)
            {
                Console.WriteLine("Unexpected exception.");
                throw;
            }
        }
    }
}
