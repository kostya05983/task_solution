using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var interpreator = new Interpretator();
            interpreator.Interpretate("def test\n" +
                                      "    set a 5\n" +
                                      "    sub a 3\n" +
                                      "    print b\n" +
                                      "set b 7\n" +
                                      "call test");
        }
    }

    class Interpretator
    {
        private Dictionary<String, Dictionary<int, IOperation>> _program =
            new Dictionary<string, Dictionary<int, IOperation>>();

        private Stack<Dictionary<int, IOperation>> _runtime = new Stack<Dictionary<int, IOperation>>();

        private Dictionary<string, Int32> memory = new Dictionary<string, Int32>();
        private Stack<String> funkStack = new Stack<string>();
        private List<int> breakPointLines = new List<int>();

        public void Interpretate(string text)
        {
            _program.Add("main", new Dictionary<int, IOperation>());
            var lines = text.Split("\n");

            var currentKey = "main";
            var currentLine = 0;
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                var operatorName = trimmed.Substring(0, trimmed.IndexOf(" ", StringComparison.Ordinal));
                if (!line.StartsWith("    ") && funkStack.Count > 0)
                {
                    currentKey = funkStack.Pop();
                }

                var cutted = CutString(trimmed);

                switch (operatorName)
                {
                    case "def":
                        funkStack.Push(currentKey);
                        currentKey = cutted;

                        _program.Add(cutted, new Dictionary<int, IOperation>());
                        break;
                    case "set":
                    {
                        _program[currentKey][currentLine] = new SetOperator(cutted);
                        break;
                    }
                    case "sub":
                    {
                        _program[currentKey][currentLine] = new SubOperator(cutted);
                        break;
                    }
                    case "rem":
                    {
                        _program[currentKey][currentLine] = new RemOperator(cutted);
                        break;
                    }
                    case "print":
                    {
                        _program[currentKey][currentLine] = new PrintOperator(cutted);
                        break;
                    }
                    case "call":
                    {
                        _program[currentKey][currentLine] = new CallOperator(cutted, _program, currentLine);
                        break;
                    }
                    case "add":
                    {
                        var breakpointLine = int.Parse(cutted);
                        breakPointLines.Add(breakpointLine);
                        break;
                    }
                    case "run":
                    {
                        RunProgram(currentLine);
                        break;
                    }
                }
            }
        }

        private void RunProgram(int currentLine)
        {
            if (_runtime.Count == 0)
            {
                var queue = _program["main"];
                _runtime.Push(queue);
            }

            while (_runtime.Count > 0)
            {
                if (breakPointLines.Contains(currentLine))
                {
                    return;
                }

                var frame = _runtime.Peek();
                if (!frame.ContainsKey(currentLine))
                {
                    _runtime.Pop();
                    continue;
                }

                frame[currentLine].Call(memory);
                currentLine++;
            }
        }

        private string CutString(string line)
        {
            return line.Substring(line.IndexOf(" ", StringComparison.Ordinal) + 1);
        }
    }

    interface IOperation
    {
        void Call(Dictionary<String, Int32> memory);
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

        public void Call(Dictionary<String, Int32> memory)
        {
            memory[_name] = _val;
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


        public void Call(Dictionary<String, Int32> memory)
        {
            memory[_name] -= _val;
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

        public void Call(Dictionary<String, Int32> memory)
        {
            memory.Remove(_name);
        }
    }

    class CallOperator : IOperation
    {
        private string _name;
        private int _currentLine;
        private Dictionary<String, Dictionary<int, IOperation>> _dictionary;

        public CallOperator(string init, Dictionary<String, Dictionary<int, IOperation>> dictionary, int currentLine)
        {
            this._currentLine = currentLine;
            this._name = init;
            this._dictionary = dictionary;
        }

        public void Call(Dictionary<string, int> memory)
        {
            var queue = _dictionary[_name];
            while (queue.Count > 0)
            {
                queue[_currentLine].Call(memory);
                _currentLine++;
            }
        }
    }

    class PrintOperator : IOperation
    {
        private string _name;

        public PrintOperator(string init)
        {
            _name = init;
        }

        public void Call(Dictionary<String, Int32> memory)
        {
            var val = memory[_name];
            Console.WriteLine(val);
        }
    }
}