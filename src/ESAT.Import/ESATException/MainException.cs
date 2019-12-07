using System;
using System.Collections.Generic;
using System.Text;

namespace ESAT.Import.ESATException
{
    public class MainException : System.Exception
    {
        public MainException(System.Exception ex, string className, string functionName, string msg) : base(msg, ex == null ? new System.Exception() : ex)
        {
            this.ClassName = className;
            this.FunctionName = functionName;
        }

        public string ClassName { get; set; }
        public string FunctionName { get; set; }

        public override string ToString()
        {
            return $"Class:{this.ClassName}\nFunction:{this.FunctionName}\nMessage:{this.Message}";
        }
    }
}
