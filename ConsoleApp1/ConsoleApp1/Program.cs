using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }

    class Interpretator
    {
        private Stack<IOperation> _stack = Stack();
        private Dictionary<string, IOperation> operators = new Dictionary<string, IOperation>();

        void Interpretate(string text)
        {
            String[] lines = text.Split("\n");

            foreach (var line in lines)
            {
                string operatorName = line.Substring(line.IndexOf(" ", StringComparison.Ordinal));
                switch (operatorName)
                {
                    case "set":
                    {
                        string cutted = line.Substring(line.IndexOf(" "));
                        operators.Add(new SetOperator(cutted));
                        break;
                    }
                    case "sub":
                    {
                        break;
                    }
                    case "rem":
                    {
                        break;
                    }
                    case "print":
                    {
                        break;
                    }
                }
            }
        }
    }

    interface IOperation
    {
        Dictionary<String, Int32> Dictionary { get; set; }

        void Call();
    }

    class SetOperator : IOperation
    {
        private string _name;
        private Int32 _val;

        public SetOperator(string init)
        {
            _name = init.Substring(init.IndexOf(" ", StringComparison.Ordinal));
            _val = Int32.Parse(init.Substring(init.IndexOf(" ", StringComparison.Ordinal), init.Length));
        }

        public Dictionary<string, int> Dictionary { get; set; }

        public void Call()
        {
            throw new NotImplementedException();
        }
    }
}