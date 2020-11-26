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
            interpreator.Interpretate("set a 5\n" +
                                      "sub a 3\n" +
                                      "print a\n" +
                                      "set b 4\n" +
                                      "print b");
        }
    }

    class Interpretator
    {
        private Queue<IOperation> _queue = new Queue<IOperation>();
        private Dictionary<string, Int32> memory = new Dictionary<string, Int32>();

        public void Interpretate(string text)
        {
            var lines = text.Split("\n");

            foreach (var line in lines)
            {
                var operatorName = line.Substring(0, line.IndexOf(" ", StringComparison.Ordinal));
                switch (operatorName)
                {
                    case "set":
                    {
                        var cutted = CutString(line);
                        _queue.Enqueue(new SetOperator(cutted));
                        break;
                    }
                    case "sub":
                    {
                        var cutted = CutString(line);
                        _queue.Enqueue(new SubOperator(cutted));
                        break;
                    }
                    case "rem":
                    {
                        var cutted = CutString(line);
                        _queue.Enqueue(new RemOperator(cutted));
                        break;
                    }
                    case "print":
                    {
                        var cutted = CutString(line);
                        _queue.Enqueue(new PrintOperator(cutted));
                        break;
                    }
                }
            }

            while (_queue.Count > 0)
            {
                var op = _queue.Dequeue();
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