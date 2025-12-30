using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupClient
{
    public enum BackupMethod
    {
        full = 1,
        incremental = 2,
        differential = 3
    }
}
