using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundProcessor.Logic
{
    using System.Diagnostics;

    public class TimedWord
    {
        public string Word { get; set; }

        public long MilliSecond { get; set; }

        public string Result { get; set; }
    }
}
