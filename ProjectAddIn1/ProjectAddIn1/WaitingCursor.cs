using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MS_Project_Import_Export
{
    public class WaitingCursor : IDisposable
    {
        Cursor current;
        public WaitingCursor()
        {
            current = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
        }

        public void Dispose()
        {
            Cursor.Current = current;
        }
    }
}
