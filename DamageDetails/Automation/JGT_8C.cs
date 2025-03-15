using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using HandyControl.Controls;

namespace DamageMaker.Automation;

public static class JGT_8C
{
	private static AutomationElement DeskTop;

	private static FlaUI.Core.AutomationElements.Window? fxWins;

	private static int count;

	static JGT_8C()
	{
		count = 0;
	}

	public static async Task<bool> IsRunning()
	{
		await Task.Run(delegate
		{
			Process process = Process.GetProcessesByName("RailTest").FirstOrDefault();
			if (process == null)
			{
				fxWins = null;
			}
			else
			{
				if (count++ == 0)
				{
					using UIA3Automation uIA3Automation = new UIA3Automation();
					DeskTop = uIA3Automation.GetDesktop();
				}
				fxWins = DeskTop.FindFirstChild((ConditionFactory cf) => cf.ByClassName("SunAwtFrame").And(cf.ByName("位置查找"))).AsWindow();
			}
		});
		return fxWins != null;
	}

	public static async Task LocationMileageAsync(string Mileage)
	{
		string Mileage2 = Mileage;
		if (fxWins != null || await IsRunning())
		{
			await Task.Run(delegate
			{
				string[] source = Mileage2.Split(new string[2] { "km", "KM" }, StringSplitOptions.RemoveEmptyEntries);
				uint[] array = source.Select((string p) => (uint)float.Parse(p.Replace("M", "").Replace("m", ""))).ToArray();
				if (array.Length == 0)
				{
					MessageBox.Warning("未找到里程数");
				}
				else if (array.Length > 2)
				{
					MessageBox.Warning("里程数解析出错");
				}
				else
				{
					fxWins.Patterns.Window.Pattern.SetWindowVisualState(WindowVisualState.Normal);
					Rectangle boundingRectangle = fxWins.BoundingRectangle;
					TypeEditBox(boundingRectangle, array);
				}
			});
		}
		else
		{
			MessageBox.Show("请打开位置查找窗口！");
		}
	}

	private static void TypeEditBox(Rectangle rect, uint[] content)
	{
		Point tb1 = new Point(rect.Right - (int)((double)rect.Width * 0.69), rect.Top + (int)((double)rect.Height * 0.1));
		Point tb2 = new Point(rect.Right - (int)((double)rect.Width * 0.49), rect.Top + (int)((double)rect.Height * 0.1));
		Mouse.DoubleClick(tb1);
		Thread.Sleep(100);
		Keyboard.Type(content[0].ToString());
		Keyboard.Press(VirtualKeyShort.TAB);
		Mouse.DoubleClick(tb2);
		if (content.Length == 2)
		{
			Thread.Sleep(100);
			Keyboard.Type(content[1].ToString());
		}
		Keyboard.Press(VirtualKeyShort.RETURN);
	}
}
