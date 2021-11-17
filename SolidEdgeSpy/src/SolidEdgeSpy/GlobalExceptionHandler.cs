using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SolidEdgeSpy
{
    static class GlobalExceptionHandler
    {
        public static void HandleException()
        {
#if DEBUG
            Debugger.Break();
#endif
        }

        public static void HandleException(System.Exception ex)
        {
#if DEBUG
            Debugger.Break();
#endif
        }
    }
}
