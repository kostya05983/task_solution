using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var _interpreter = new Interpretator();

            _interpreter.Interpretate("set code");
            _interpreter.Interpretate("def test\n" +
                                      "    set a 4\n" +
                                      "set t 5\n" +
                                      "call test\n" +
                                      "sub a 3\n" +
                                      "call test\n" +
                                      "print a");
            _interpreter.Interpretate("end set code");
            _interpreter.Interpretate("add break 1");
            _interpreter.Interpretate("run");
            _interpreter.Interpretate("print trace");
            _interpreter.Interpretate("run");
            _interpreter.Interpretate("run");
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

        public Dictionary<int, IOperation> Operations { get; }

        public int CurrentRuntimeLine { get; set; }

        public Frame(string name, Dictionary<int, IOperation> operations, int calledLine)
        {
            Name = name;
            Operations = operations;
            CalledLine = calledLine;
            CurrentRuntimeLine = Operations.Count != 0 ? Operations.Keys.Min() : 0;
        }

        public bool ExecuteFrameLine(Dictionary<string, Variable> memory)
        {
            var op = Operations[CurrentRuntimeLine];
            op.Call(memory, CurrentRuntimeLine);
            CurrentRuntimeLine++;
            return op is CallOperator;
        }

        public bool isOver()
        {
            return !Operations.ContainsKey(CurrentRuntimeLine);
        }

        public void Reset()
        {
            CurrentRuntimeLine = Operations.Count != 0 ? Operations.Keys.Min() : 0;
        }
    }

    class Interpretator
    {
        private Dictionary<String, Frame> program = new Dictionary<string, Frame>();
        private Stack<Frame> runtime = new Stack<Frame>();
        private Dictionary<string, Variable> memory = new Dictionary<string, Variable>();
        private List<int> breakPointLines = new List<int>();
        private int? lastBreakLine;

        public Interpretator()
        {
            program.Add("main", new Frame("main", new Dictionary<int, IOperation>(), 0));
        }

        public void Interpretate(string text)
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
                    var frame = runtime.Peek();
                    if (frame.ExecuteFrameLine(memory))
                    {
                        var newFrame = runtime.Peek();
                        RunFrame(newFrame, true);
                        runtime.Pop();
                    }

                    break;
                }
                default:
                {
                    ParseProgram(text);
                    break;
                }
            }
        }

        private void ParseProgram(string text)
        {
            var lines = text.Split("\n");
            var currentLine = 0;
            var currentFrame = "main";
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

                        program.Add(cutted, new Frame(cutted, new Dictionary<int, IOperation>(), currentLine));
                        break;
                    case "set":
                    {
                        program[currentFrame].Operations[currentLine] = new SetOperator(cutted);
                        break;
                    }
                    case "sub":
                    {
                        program[currentFrame].Operations[currentLine] = new SubOperator(cutted);
                        break;
                    }
                    case "rem":
                    {
                        program[currentFrame].Operations[currentLine] = new RemOperator(cutted);
                        break;
                    }
                    case "print":
                    {
                        program[currentFrame].Operations[currentLine] = new PrintOperator(cutted);
                        break;
                    }
                    case "call":
                    {
                        program[currentFrame].Operations[currentLine] = new CallOperator(cutted, program, runtime);
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
            foreach (var trace in runtime.Where(trace => trace.Name != "main"))
            {
                Console.WriteLine(trace.CalledLine + " " + trace.Name);
            }
        }

        private void RunProgram()
        {
            if (runtime.Count == 0)
            {
                var queue = program["main"];
                queue.Reset();
                runtime.Push(queue);
            }

            while (runtime.Count > 0)
            {
                var frame = runtime.Peek();
                var result = RunFrame(frame, false);
                switch (result)
                {
                    case 0:
                        runtime.Pop();
                        break;
                    case -1:
                        return;
                }
            }
        }

        private int RunFrame(Frame frame, bool isIgnoreBreak)
        {
            while (!frame.isOver())
            {
                if (breakPointLines.Contains(frame.CurrentRuntimeLine) && !isIgnoreBreak &&
                    frame.CurrentRuntimeLine != lastBreakLine)
                {
                    lastBreakLine = frame.CurrentRuntimeLine;
                    return -1;
                }

                lastBreakLine = null;
                if (frame.ExecuteFrameLine(memory))
                {
                    return 1;
                }
            }

            return 0;
        }

        private string CutString(string line)
        {
            return line.Substring(line.IndexOf(" ", StringComparison.Ordinal) + 1);
        }
    }

    interface IOperation
    {
        void Call(Dictionary<string, Variable> memory, int currentLine);
    }

    class SetOperator : IOperation
    {
        private string name;
        private Int32 val;

        public SetOperator(string init)
        {
            name = init.Substring(0, init.IndexOf(" ", StringComparison.Ordinal));
            val = Int32.Parse(init.Substring(init.IndexOf(" ", StringComparison.Ordinal) + 1));
        }

        public void Call(Dictionary<string, Variable> memory, int currentLine)
        {
            memory[name] = new Variable(val, currentLine);
        }
    }

    class SubOperator : IOperation
    {
        private string name;
        private Int32 val;

        public SubOperator(string init)
        {
            name = init.Substring(0, init.IndexOf(" ", StringComparison.Ordinal));
            val = Int32.Parse(init.Substring(init.IndexOf(" ", StringComparison.Ordinal) + 1));
        }


        public void Call(Dictionary<String, Variable> memory, int currentLine)
        {
            memory[name] = new Variable(
                memory[name].Value - val, currentLine);
        }
    }

    class RemOperator : IOperation
    {
        private string name;

        public RemOperator(string init)
        {
            name = init.Substring(0, init.IndexOf(" ", StringComparison.Ordinal));
        }

        public void Call(Dictionary<String, Variable> memory, int currentLine)
        {
            memory.Remove(name);
        }
    }

    class CallOperator : IOperation
    {
        private string name;
        private Dictionary<String, Frame> program;
        private Stack<Frame> runtime;

        public CallOperator(
            string init,
            Dictionary<String, Frame> program,
            Stack<Frame> _runtime
        )
        {
            this.name = init;
            this.program = program;
            this.runtime = _runtime;
        }

        public void Call(Dictionary<string, Variable> memory, int currentLine)
        {
            var frame = program[name];
            runtime.Push(new Frame(frame.Name, frame.Operations, currentLine));
        }
    }

    class PrintOperator : IOperation
    {
        private string name;

        public PrintOperator(string init)
        {
            name = init;
        }

        public void Call(Dictionary<String, Variable> memory, int currentLine)
        {
            var val = memory[name];
            Console.WriteLine(val.Value);
        }
    }
}