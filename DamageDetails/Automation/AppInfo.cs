using FlaUI.UIA3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WinRT;

namespace DamageMaker.Automation
{
   public static class AppInfo
    {
        public static  string GetFocusedApplicationName()
        {
            using (var automation = new UIA3Automation())
            {
           
              
                var focusedElement = automation.FocusedElement();
                if (focusedElement != null)
                {
                    var app = focusedElement.Properties.ProcessId;
                    var process = System.Diagnostics.Process.GetProcessById(app);
                    return process.MainModule.ModuleName;
                }
                return string.Empty;
            }
        }


    }
}
