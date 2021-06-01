using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;

namespace AutomatedInputHandler
{
    public partial class Form1 : Form
    {
        Task RunTask;

        CancellationTokenSource TokenSource;
        CancellationToken Token;
    }
}
