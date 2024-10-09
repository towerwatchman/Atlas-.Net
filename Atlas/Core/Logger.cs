using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.Core
{
    public static class Logging
    {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    }
}
