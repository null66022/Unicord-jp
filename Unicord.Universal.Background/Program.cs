﻿using System;
using System.Threading;
using System.Windows.Forms;

namespace Unicord.Universal.Background
{
    class Program
    {
        private static Mutex _mutex;

        [STAThread]
        static void Main(string[] args)
        {
            _mutex = new Mutex(true, "{88FE061B-B4D8-41F4-99FE-15870E0F535B}", out var createdNew);
            if (!createdNew) return;

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new NotificationApplicationContext());
            }
            finally
            {
                _mutex.Dispose();
            }
        }
    }
}
