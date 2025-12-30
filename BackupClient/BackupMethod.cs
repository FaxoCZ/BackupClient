using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupClient
{
    public enum BackupMethod
    {
        Full = 1,
        Incremental = 2,
        Differential = 3
    }
}
