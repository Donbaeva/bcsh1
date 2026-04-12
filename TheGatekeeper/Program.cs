using System;
using System.Windows.Forms;

namespace TheGatekeeper
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Запускаем только WelcomeForm
            Application.Run(new WelcomeForm());
        }
    }
}