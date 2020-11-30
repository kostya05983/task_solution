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
        private Dictionary<String, Queue<IOperation>> _program = new Dictionary<string, Queue<IOperation>>();
        private Dictionary<string, Int32> memory = new Dictionary<string, Int32>();
        private Stack<String> funkStack = new Stack<string>();

        public void Interpretate(string text)
        {
            _program.Add("main", new Queue<IOperation>());
            var lines = text.Split("\n");

            var currentKey = "main";
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                var operatorName = trimmed.Substring(0, trimmed.IndexOf(" ", StringComparison.Ordinal));
                if (!line.StartsWith("    ") && funkStack.Count > 0)
                {
                    currentKey = funkStack.Pop();
                }

                switch (operatorName)
                {
                    case "def":
                        var name = CutString(trimmed);
                        funkStack.Push(currentKey);
                        currentKey = name;

                        _program.Add(name, new Queue<IOperation>());
                        break;
                    case "set":
                    {
                        var cutted = CutString(trimmed);
                        _program[currentKey].Enqueue(new SetOperator(cutted));
                        break;
                    }
                    case "sub":
                    {
                        var cutted = CutString(trimmed);
                        _program[currentKey].Enqueue(new SubOperator(cutted));
                        break;
                    }
                    case "rem":
                    {
                        var cutted = CutString(trimmed);
                        _program[currentKey].Enqueue(new RemOperator(cutted));
                        break;
                    }
                    case "print":
                    {
                        var cutted = CutString(trimmed);
                        _program[currentKey].Enqueue(new PrintOperator(cutted));
                        break;
                    }
                    case "call":
                    {
                        var cutted = CutString(trimmed);
                        _program[currentKey].Enqueue(new CallOperator(cutted, _program));
                        break;
                    }
                }
            }

            var queue = _program["main"];
            while (queue.Count > 0)
            {
                var op = queue.Dequeue();
                op.Call(memory);
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
        private Dictionary<String, Queue<IOperation>> _dictionary;

        public CallOperator(string init, Dictionary<String, Queue<IOperation>> dictionary)
        {
            this._name = init;
            this._dictionary = dictionary;
        }

        public void Call(Dictionary<string, int> memory)
        {
            var queue = _dictionary[_name];
            while (queue.Count > 0)
            {
                var op = queue.Dequeue();
                op.Call(memory);
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