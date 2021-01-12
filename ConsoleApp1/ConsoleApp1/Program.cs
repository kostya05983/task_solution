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

    class Frame
    {
        public string Name { get; }

        public int CalledLine { get; }

        public LinkedList<IOperation> Operations { get; }

        public LinkedListNode<IOperation> CurrentRuntimeOperator { get; set; }

        public int CurrentLine
        {
            get => CurrentRuntimeOperator.Value.ParsedLine;
        }

        public Frame(string name, LinkedList<IOperation> operations, int calledLine)
        {
            Name = name;
            Operations = operations;
            CalledLine = calledLine;
            CurrentRuntimeOperator = operations.First;
        }

        public bool ExecuteFrameLine(Dictionary<string, Variable> memory)
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
        private Dictionary<String, Frame> program = new Dictionary<string, Frame>();
        private Stack<Frame> runtime = new Stack<Frame>();
        private Dictionary<string, Variable> memory = new Dictionary<string, Variable>();
        private List<int> breakPointLines = new List<int>();
        private int? lastBreakLine;

        private const int NEW_CALL = 1;
        private const int INTERRUPT = -1;
        private const int OVER = 0;
        private const string MAIN = "main";

        public Interpretator()
        {
            program.Add(MAIN, new Frame(MAIN, new LinkedList<IOperation>(), 0));
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
                    var frame = runtime.Peek();
                    frame.ExecuteFrameLine(memory);
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
            var frame = runtime.Peek();
            if (!frame.ExecuteFrameLine(memory)) return;

            var newFrame = runtime.Peek();
            var previousCount = runtime.Count - 1;
            while (runtime.Count != previousCount)
            {
                newFrame = runtime.Peek();
                var result = RunFrame(newFrame, false);
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
            var currentFrame = MAIN;
            var funkStack = new Stack<String>();
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                var operatorName = trimmed.Substring(0, trimmed.IndexOf(" ", StringComparison.Ordinal));
                if (!line.StartsWith("    ") && funkStack.Count > 0)
                {
                    currentFrame = funkStack.Pop();
                }

                var cutted = CutString(trimmed);

                switch (operatorName)
                {
                    case "def":
                        funkStack.Push(currentFrame);
                        currentFrame = cutted;

                        program.Add(cutted, new Frame(cutted, new LinkedList<IOperation>(), currentLine));
                        break;
                    case "set":
                    {
                        program[currentFrame].Operations.AddLast(new SetOperator(cutted, currentLine));
                        break;
                    }
                    case "sub":
                    {
                        program[currentFrame].Operations.AddLast(new SubOperator(cutted, currentLine));
                        break;
                    }
                    case "rem":
                    {
                        program[currentFrame].Operations.AddLast(new RemOperator(cutted, currentLine));
                        break;
                    }
                    case "print":
                    {
                        program[currentFrame].Operations.AddLast(new PrintOperator(cutted, currentLine));
                        break;
                    }
                    case "call":
                    {
                        program[currentFrame].Operations
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

            while (runtime.Count > 0)
            {
                var frame = runtime.Peek();
                var result = RunFrame(frame, false);
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

        private int RunFrame(Frame frame, bool isIgnoreBreak)
        {
            while (!frame.IsOver())
            {
                if (breakPointLines.Contains(frame.CurrentLine) && !isIgnoreBreak &&
                    frame.CurrentLine != lastBreakLine)
                {
                    lastBreakLine = frame.CurrentLine;
                    return INTERRUPT;
                }

                lastBreakLine = null;
                if (frame.ExecuteFrameLine(memory))
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
        private Dictionary<String, Frame> program;
        private Stack<Frame> runtime;
        public override int ParsedLine { get; }

        public CallOperator(
            string init,
            int parsedLine,
            Dictionary<String, Frame> program,
            Stack<Frame> _runtime
        )
        {
            this.name = init;
            this.program = program;
            this.runtime = _runtime;
            this.ParsedLine = parsedLine;
        }

        public override void Call(Dictionary<string, Variable> memory)
        {
            var frame = program[name];
            frame.Reset();
            runtime.Push(new Frame(name, frame.Operations, ParsedLine));
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