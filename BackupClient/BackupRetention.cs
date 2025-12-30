using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackupClient
{
    internal class BackupRetention
    {
        public int Count { get; set; }
        public int Size { get; set; }

        public BackupRetention(int count, int size)
        {
            Count = count;
            Size = size;
        }
    }
}
