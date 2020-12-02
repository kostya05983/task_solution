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
    
    
//    [TestFixture]
    class Program
    {

        [Test]
        public void ShouldShowTrace()
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
        public int Value;
        public int LastChanged;

        public Variable(int value, int lastChanged)
        {
            Value = value;
            this.LastChanged = lastChanged;
        }
    }

    class Trace
    {
        public string Name;
        public int Line;

        public Trace(string name, int line)
        {
            this.Name = name;
            this.Line = line;
        }
    }

    class Frame
    {
        public string Name;
        public int CalledLine;
        public Dictionary<int, IOperation> Operations;

        public Frame(string name, Dictionary<int, IOperation> operations, int calledLine)
        {
            Name = name;
            Operations = operations;
            CalledLine = calledLine;
        }
    }

    class Interpretator
    {
        private Dictionary<String, Frame> _program =
            new Dictionary<string, Frame>();

        private Stack<Frame> _runtime = new Stack<Frame>();

        private Dictionary<string, Variable> memory = new Dictionary<string, Variable>();
        private Stack<Trace> funkStack = new Stack<Trace>();
        private List<int> breakPointLines = new List<int>();
        private int currentLine = 0;
        private Dictionary<string, int> framePosition = new Dictionary<string, int>();

        public Interpretator()
        {
            _program.Add("main", new Frame("main", new Dictionary<int, IOperation>(), 0));
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
                    var frame = _runtime.Peek();
                    var op = frame.Operations[currentLine];
                    op.Call(memory, currentLine);
                    currentLine++;
                    break;
                }
                case "step over":
                {
                    var frame = _runtime.Peek();
                    var op = frame.Operations[currentLine];
                    if (op is CallOperator)
                    {
                        var newFrame = _runtime.Peek();
                        RunFrame(newFrame, true);
                        _runtime.Pop();
                    }

                    currentLine++;
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
            var currentKey = "main";
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                var operatorName = trimmed.Substring(0, trimmed.IndexOf(" ", StringComparison.Ordinal));
                if (!line.StartsWith("    ") && funkStack.Count > 0)
                {
                    currentKey = funkStack.Pop().Name;
                }

                var cutted = CutString(trimmed);

                switch (operatorName)
                {
                    case "def":
                        funkStack.Push(new Trace(currentKey, currentLine));
                        currentKey = cutted;

                        _program.Add(cutted, new Frame(cutted, new Dictionary<int, IOperation>(), currentLine));
                        break;
                    case "set":
                    {
                        _program[currentKey].Operations[currentLine] = new SetOperator(cutted);
                        break;
                    }
                    case "sub":
                    {
                        _program[currentKey].Operations[currentLine] = new SubOperator(cutted);
                        break;
                    }
                    case "rem":
                    {
                        _program[currentKey].Operations[currentLine] = new RemOperator(cutted);
                        break;
                    }
                    case "print":
                    {
                        _program[currentKey].Operations[currentLine] = new PrintOperator(cutted);
                        break;
                    }
                    case "call":
                    {
                        _program[currentKey].Operations[currentLine] = new CallOperator(cutted, _program, _runtime);
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
            foreach (var trace in _runtime.Where(trace => trace.Name != "main"))
            {
                Console.WriteLine(trace.CalledLine + " " + trace.Name);
            }
        }

        private void RunProgram()
        {
            if (_runtime.Count == 0)
            {
                var queue = _program["main"];
                _runtime.Push(queue);
            }

            while (_runtime.Count > 0)
            {
                var frame = _runtime.Peek();
                Console.WriteLine("current frame", frame.Name);
                var result = RunFrame(frame, false);
                if (result == 0)
                {
                    _runtime.Pop();
                }
            }
        }

        private int RunFrame(Frame frame, bool isIgnoreBreak)
        {
            int currentFrameLine;
            if (!framePosition.ContainsKey(frame.Name))
            {
                currentFrameLine = frame.Operations.Keys.Min();
                framePosition[frame.Name] = currentFrameLine;
            }
            else
            {
                currentFrameLine = framePosition[frame.Name];
            }

            while (frame.Operations.ContainsKey(currentFrameLine))
            {
                if (breakPointLines.Contains(currentFrameLine) && !isIgnoreBreak)
                {
                    return 1;
                }

                var op = frame.Operations[currentFrameLine];
                op.Call(memory, currentFrameLine);
                currentFrameLine++;
                framePosition[frame.Name] = currentFrameLine;
                if (op is CallOperator)
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
        private string _name;
        private Int32 _val;

        public SetOperator(string init)
        {
            _name = init.Substring(0, init.IndexOf(" ", StringComparison.Ordinal));
            _val = Int32.Parse(init.Substring(init.IndexOf(" ", StringComparison.Ordinal) + 1));
        }

        public void Call(Dictionary<string, Variable> memory, int currentLine)
        {
            memory[_name] = new Variable(_val, currentLine);
        }
    }

    class SubOperator : IOperation
    {
        private string _name;
        private Int32 _val;

        public SubOperator(string init)
        {
            _name = init.Substring(0, init.IndexOf(" ", StringComparison.Ordinal));
            _val = Int32.Parse(init.Substring(init.IndexOf(" ", StringComparison.Ordinal) + 1));
        }


        public void Call(Dictionary<String, Variable> memory, int currentLine)
        {
            memory[_name] = new Variable(
                memory[_name].Value - _val, currentLine);
        }
    }

    class RemOperator : IOperation
    {
        private string _name;
        private Int32 _val;

        public RemOperator(string init)
        {
            _name = init.Substring(0, init.IndexOf(" ", StringComparison.Ordinal));
            _val = Int32.Parse(init.Substring(init.IndexOf(" ", StringComparison.Ordinal) + 1));
        }

        public void Call(Dictionary<String, Variable> memory, int currentLine)
        {
            memory.Remove(_name);
        }
    }

    class CallOperator : IOperation
    {
        private string _name;
        private Dictionary<String, Frame> _program;
        private Stack<Frame> _runtime;

        public CallOperator(string init, Dictionary<String, Frame> program,
            Stack<Frame> _runtime)
        {
            this._name = init;
            this._program = program;
            this._runtime = _runtime;
        }

        public void Call(Dictionary<string, Variable> memory, int currentLine)
        {
            var queue = _program[_name];
            _runtime.Push(queue);
        }
    }

    class PrintOperator : IOperation
    {
        private string _name;

        public PrintOperator(string init)
        {
            _name = init;
        }

        public void Call(Dictionary<String, Variable> memory, int currentLine)
        {
            var val = memory[_name];
            Console.WriteLine(val.Value);
        }
    }
}