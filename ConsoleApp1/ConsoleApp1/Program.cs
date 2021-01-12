using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace ConsoleApp1
{
    [TestFixture]
    class Program
    {

        [Test]
        public void ShouldSubVariable()
        {
            var _interpreter = new Interpretator();
            _interpreter.ExecuteLine("set code");
            _interpreter.ExecuteLine("set a 5\n" +
                                     "sub a 2\n" +
                                     "print a");
            _interpreter.ExecuteLine("end set code");
            _interpreter.ExecuteLine("run");
        }
        
        [Test]
        public void ShouldCallFunctionAndPrint()
        {
            var _interpreter = new Interpretator();
            var _writer = new StringWriter();
            Console.SetOut(_writer);
            _interpreter.ExecuteLine("set code");
            _interpreter.ExecuteLine("def test\n" +
                                     "    print a\n" +
                                     "set a 5\n" +
                                     "call test");
            _interpreter.ExecuteLine("end set code");
            _interpreter.ExecuteLine("run");
            var output = _writer.ToString();
            var a = int.Parse(output);
            Assert.That(a, Is.EqualTo(5));
        }

        [Test]
        public void ShouldCallFunctionSubAndPrint()
        {
            var _interpreter = new Interpretator();
            var _writer = new StringWriter();
            Console.SetOut(_writer);
            _interpreter.ExecuteLine("set code");
            _interpreter.ExecuteLine("def test\n" +
                                     "    sub a 3\n" +
                                     "    print a\n" +
                                     "set a 5\n" +
                                     "call test");
            _interpreter.ExecuteLine("end set code");
            _interpreter.ExecuteLine("run");
            
            var output = _writer.ToString();
            var a = int.Parse(output);

            Assert.That(a, Is.EqualTo(2));
        }

        [Test]
        public void ShouldCallFunctionAndPrintGlobalVarAfter()
        {
            var _interpreter = new Interpretator();
            var _writer = new StringWriter();
            Console.SetOut(_writer);

            _interpreter.ExecuteLine("set code");
            _interpreter.ExecuteLine("def test\n" +
                                     "    set a 4\n" +
                                     "    sub a 3\n" +
                                     "call test\n" +
                                     "print a");
            _interpreter.ExecuteLine("end set code");
            _interpreter.ExecuteLine("run");

            var output = _writer.ToString();
            var a = int.Parse(output);

            Assert.That(a, Is.EqualTo(1));
        }

        [Test]
        public void ShouldCallFunctionTwice()
        {
            var _interpreter = new Interpretator();
            var _writer = new StringWriter();
            Console.SetOut(_writer);

            _interpreter.ExecuteLine("set code");
            _interpreter.ExecuteLine("def test\n" +
                                     "    sub a 3\n" +
                                     "set a 12\n" +
                                     "call test\n" +
                                     "sub a 3\n" +
                                     "call test\n" +
                                     "print a");
            _interpreter.ExecuteLine("end set code");
            _interpreter.ExecuteLine("run");

            var output = _writer.ToString();
            var a = int.Parse(output);

            Assert.That(a, Is.EqualTo(3));
        }

        [Test]
        public void ShouldShowMemoryVars()
        {
            var _interpreter = new Interpretator();
            var _writer = new StringWriter();
            Console.SetOut(_writer);

            _interpreter.ExecuteLine("set code");
            _interpreter.ExecuteLine("set a 5\n" +
                                     "set b 4\n" +
                                     "set c 3\n" +
                                     "set d 2\n" +
                                     "set e 1");
            _interpreter.ExecuteLine("end set code");
            _interpreter.ExecuteLine("add break 2");
            _interpreter.ExecuteLine("run");
            _interpreter.ExecuteLine("print mem");

            var output = _writer.ToString();
            Assert.That(output.Contains("a 5 0") && output.Contains("b 4 1"), Is.EqualTo(true));
        }

        [Test]
        public void ShouldShowTrace()
        {
            var _interpreter = new Interpretator();
            var _writer = new StringWriter();
            Console.SetOut(_writer);

            _interpreter.ExecuteLine("set code");
            _interpreter.ExecuteLine("def test\n" +
                                     "    set a 4\n" +
                                     "set t 5\n" +
                                     "call test\n" +
                                     "sub a 3\n" +
                                     "call test\n" +
                                     "print a");
            _interpreter.ExecuteLine("end set code");
            _interpreter.ExecuteLine("add break 1");
            _interpreter.ExecuteLine("run");
            _interpreter.ExecuteLine("print trace");
            _interpreter.ExecuteLine("run");
            _interpreter.ExecuteLine("run");

            var output = _writer.ToString();
            Assert.That(output.Contains("3 test") && output.Contains("4"),
                Is.EqualTo(true));
        }
        
           [Test]
        public void ShouldOverrideVariable()
        {
            var _interpreter = new Interpretator();
            var _writer = new StringWriter();
            Console.SetOut(_writer);

            _interpreter.ExecuteLine("set code");
            _interpreter.ExecuteLine("set a 5\n" +
                                     "set a 6\n" +
                                     "print a");
            _interpreter.ExecuteLine("end set code");
            _interpreter.ExecuteLine("run");

            var output = Int32.Parse(_writer.ToString());
            Assert.That(output, Is.EqualTo(6));
        }

        [Test]
        public void ShouldPostFuncDefinitionWorks()
        {
            var _interpreter = new Interpretator();
            var _writer = new StringWriter();
            Console.SetOut(_writer);

            _interpreter.ExecuteLine("set code");
            _interpreter.ExecuteLine("call test\n" +
                                     "print a\n" +
                                     "def test\n" +
                                     "    set a 5");
            _interpreter.ExecuteLine("end set code");
            _interpreter.ExecuteLine("run");

            var output = Int32.Parse(_writer.ToString());
            Assert.That(output, Is.EqualTo(5));
        }

        [Test]
        public void ShouldStepOverWork()
        {
            var _interpreter = new Interpretator();
            var _writer = new StringWriter();
            Console.SetOut(_writer);

            _interpreter.ExecuteLine("set code");
            _interpreter.ExecuteLine("def test\n" +
                                     "    set a 4\n" +
                                     "    set b 5\n" +
                                     "set t 5\n" +
                                     "call test\n" +
                                     "print a");
            _interpreter.ExecuteLine("end set code");
            _interpreter.ExecuteLine("add break 1");
            _interpreter.ExecuteLine("add break 4");
            _interpreter.ExecuteLine("run");
            _interpreter.ExecuteLine("step over");
            _interpreter.ExecuteLine("step");

            var output = Int32.Parse(_writer.ToString());
            Assert.That(output, Is.EqualTo(4));
        }
        
        static void Main(string[] args)
        {
        }
    }

    class Variable
    {
        public int Value { get; }

        public int LastChanged { get; }

        public Variable(int value, int lastChanged)
        {
            Value = value;
            LastChanged = lastChanged;
        }
    }

    class Function
    {
        public string Name { get; }

        public int CalledLine { get; }

        public LinkedList<IOperation> Operations { get; }

        public LinkedListNode<IOperation> CurrentRuntimeOperator { get; set; }

        public int CurrentLine
        {
            get => CurrentRuntimeOperator.Value.ParsedLine;
        }

        public Function(string name, LinkedList<IOperation> operations, int calledLine)
        {
            Name = name;
            Operations = operations;
            CalledLine = calledLine;
            CurrentRuntimeOperator = operations.First;
        }

        /**
         * set a 5
         * sub a 3
         */
        public bool ExecuteFunctionLine(Dictionary<string, Variable> memory)
        {
            var op = CurrentRuntimeOperator.Value;
            op.Call(memory);
            CurrentRuntimeOperator = CurrentRuntimeOperator.Next;
            return op is CallOperator;
        }
        
        public bool IsOver()
        {
            return CurrentRuntimeOperator == null;
        }

        public void Reset()
        {
            CurrentRuntimeOperator = Operations.First;
        }
    }

    class Interpretator
    {
        private Dictionary<String, Function> program = new Dictionary<string, Function>();
        private Stack<Function> runtime = new Stack<Function>();
        private Dictionary<string, Variable> memory = new Dictionary<string, Variable>();
        private List<int> breakPointLines = new List<int>();
        private int? lastBreakLine;

        private const int NEW_CALL = 1;
        private const int INTERRUPT = -1;
        private const int OVER = 0;
        private const string MAIN = "main";

        public Interpretator()
        {
            program.Add(MAIN, new Function(MAIN, new LinkedList<IOperation>(), 0));
        }

        public void ExecuteLine(string text)
        {
            switch (text)
            {
                case "set code":
                {
                    break;
                }
                case "end set code":
                {
                    break;
                }
                case "run":
                {
                    RunProgram();
                    break;
                }
                case "print mem":
                {
                    PrintMem();
                    break;
                }
                case "print trace":
                {
                    PrintTrace();
                    break;
                }
                case "step":
                {
                    var function = runtime.Peek();
                    function.ExecuteFunctionLine(memory);
                    break;
                }
                case "step over":
                {
                    StepOver();
                    break;
                }
                default:
                {
                    ParseProgram(text);
                    break;
                }
            }
        }

        private void StepOver()
        {
            var function = runtime.Peek();
            if (!function.ExecuteFunctionLine(memory)) return;

            var newFunction = runtime.Peek();
            var previousCount = runtime.Count - 1;
            while (runtime.Count != previousCount)
            {
                newFunction = runtime.Peek();
                var result = RunFunction(newFunction, false);
                switch (result)
                {
                    case OVER:
                        runtime.Pop();
                        break;
                }
            }
        }

        private void ParseProgram(string text)
        {
            var lines = text.Split("\n");
            var currentLine = 0;
            var currentFunction = MAIN;
            var funkStack = new Stack<String>();
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                var operatorName = trimmed.Substring(0, trimmed.IndexOf(" ", StringComparison.Ordinal));
                if (!line.StartsWith("    ") && funkStack.Count > 0)
                {
                    currentFunction = funkStack.Pop();
                }

                var cutted = CutString(trimmed);

                switch (operatorName)
                {
                    case "def":
                        funkStack.Push(currentFunction);
                        currentFunction = cutted;

                        program.Add(cutted, new Function(cutted, new LinkedList<IOperation>(), currentLine));
                        break;
                    case "set":
                    {
                        program[currentFunction].Operations.AddLast(new SetOperator(cutted, currentLine));
                        break;
                    }
                    case "sub":
                    {
                        program[currentFunction].Operations.AddLast(new SubOperator(cutted, currentLine));
                        break;
                    }
                    case "rem":
                    {
                        program[currentFunction].Operations.AddLast(new RemOperator(cutted, currentLine));
                        break;
                    }
                    case "print":
                    {
                        program[currentFunction].Operations.AddLast(new PrintOperator(cutted, currentLine));
                        break;
                    }
                    case "call":
                    {
                        program[currentFunction].Operations
                            .AddLast(new CallOperator(cutted, currentLine, program, runtime));
                        break;
                    }
                    case "add":
                    {
                        var breakpointLine = int.Parse(CutString(cutted));
                        breakPointLines.Add(breakpointLine);
                        break;
                    }
                }

                currentLine++;
            }
        }

        private void PrintMem()
        {
            foreach (var entry in memory)
            {
                Console.WriteLine(entry.Key + " " + entry.Value.Value + " " + entry.Value.LastChanged);
            }
        }

        private void PrintTrace()
        {
            foreach (var trace in runtime.Where(trace => trace.Name != MAIN))
            {
                Console.WriteLine(trace.CalledLine + " " + trace.Name);
            }
        }

        private void RunProgram()
        {
            if (runtime.Count == 0)
            {
                var queue = program[MAIN];
                queue.Reset();
                runtime.Push(queue);
            }

            /**
             * fun1
             * fun2
             *
             * fun0
             * fun1
             * fun2
             */
            while (runtime.Count > 0)
            {
                var function = runtime.Peek();
                var result = RunFunction(function, false);
                switch (result)
                {
                    case OVER:
                        runtime.Pop();
                        break;
                    case INTERRUPT:
                        return;
                }
            }
        }

        private int RunFunction(Function function, bool isIgnoreBreak)
        {
            while (!function.IsOver())
            {
                if (breakPointLines.Contains(function.CurrentLine) && !isIgnoreBreak &&
                    function.CurrentLine != lastBreakLine)
                {
                    lastBreakLine = function.CurrentLine;
                    return INTERRUPT;
                }

                lastBreakLine = null;
                if (function.ExecuteFunctionLine(memory))
                {
                    return NEW_CALL;
                }
            }

            return OVER;
        }

        private string CutString(string line)
        {
            return line.Substring(line.IndexOf(" ", StringComparison.Ordinal) + 1);
        }
    }

    abstract class IOperation
    {
        public abstract int ParsedLine { get; }

        public abstract void Call(Dictionary<string, Variable> memory);
    }

    class SetOperator : IOperation
    {
        private string name;
        private Int32 val;

        public override int ParsedLine { get; }

        public SetOperator(string init, int parsedLine)
        {
            name = init.Substring(0, init.IndexOf(" ", StringComparison.Ordinal));
            val = Int32.Parse(init.Substring(init.IndexOf(" ", StringComparison.Ordinal) + 1));
            this.ParsedLine = parsedLine;
        }

        public override void Call(Dictionary<string, Variable> memory)
        {
            memory[name] = new Variable(val, ParsedLine);
        }
    }

    class SubOperator : IOperation
    {
        private string name;
        private Int32 val;
        public override int ParsedLine { get; }

        public SubOperator(string init, int parsedLine)
        {
            name = init.Substring(0, init.IndexOf(" ", StringComparison.Ordinal));
            val = Int32.Parse(init.Substring(init.IndexOf(" ", StringComparison.Ordinal) + 1));
            this.ParsedLine = parsedLine;
        }


        public override void Call(Dictionary<string, Variable> memory)
        {
            memory[name] = new Variable(
                memory[name].Value - val, ParsedLine);
        }
    }

    class RemOperator : IOperation
    {
        private string name;
        public override int ParsedLine { get; }

        public RemOperator(string init, int parsedLine)
        {
            name = init.Substring(0, init.IndexOf(" ", StringComparison.Ordinal));
            this.ParsedLine = parsedLine;
        }

        public override void Call(Dictionary<string, Variable> memory)
        {
            memory.Remove(name);
        }
    }

    class CallOperator : IOperation
    {
        private string name;
        private Dictionary<String, Function> program;
        private Stack<Function> runtime;
        public override int ParsedLine { get; }

        public CallOperator(
            string init,
            int parsedLine,
            Dictionary<String, Function> program,
            Stack<Function> _runtime
        )
        {
            this.name = init;
            this.program = program;
            this.runtime = _runtime;
            this.ParsedLine = parsedLine;
        }

        public override void Call(Dictionary<string, Variable> memory)
        {
            var function = program[name];
            function.Reset();
            runtime.Push(new Function(name, function.Operations, ParsedLine));
        }
    }

    class PrintOperator : IOperation
    {
        private string name;
        public override int ParsedLine { get; }

        public PrintOperator(string init, int parsedLine)
        {
            name = init;
            this.ParsedLine = parsedLine;
        }

        public override void Call(Dictionary<string, Variable> memory)
        {
            var val = memory[name];
            Console.WriteLine(val.Value);
        }
    }
}