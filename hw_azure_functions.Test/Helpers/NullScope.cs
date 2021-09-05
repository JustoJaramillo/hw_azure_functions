using System;
using System.Collections.Generic;
using System.Text;

namespace hw_azure_functions.Test.Helpers
{
    class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();
        public void Dispose(){}
        private NullScope (){}
    }
}
