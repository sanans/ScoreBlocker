using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Timers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Gma.UserActivityMonitor;
using System.Threading;



namespace Score_Blocker
{
	public class ScoreBlocker : Form
	{
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool IsWindow(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32", SetLastError = true, CharSet = CharSet.Auto)]
		private extern static int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		static extern bool SetCursorPos(int x, int y);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

		[DllImport("user32.dll")]
		public static extern bool GetCursorPos(out POINT lpPoint);

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern bool SetForegroundWindow(IntPtr hwnd);

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int X;
			public int Y;

			public static implicit operator Point(POINT point)
			{
				return new Point(point.X, point.Y);
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;        // x position of upper-left corner
			public int Top;         // y position of upper-left corner
			public int Right;       // x position of lower-right corner
			public int Bottom;      // y position of lower-right corner
		}

		private enum EWidnowFoundType
		{
			NONE, CHROME, ADOBE, ESPN3
		}

		delegate void VoidBlankDelegate();
		delegate void SetBoundsDelegate(int i_x, int i_y, int i_width, int i_height);

		private System.Timers.Timer p_timer;
		private EWidnowFoundType p_windowType = EWidnowFoundType.NONE;
		IntPtr p_windowHandle = new IntPtr();
		private float p_espn3WidthPercent = 0.0f;

		public ScoreBlocker()
		{
			this.TopMost = true;
			this.StartPosition = FormStartPosition.Manual;
			BackColor = System.Drawing.Color.DarkBlue;

			FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;

			p_timer = new System.Timers.Timer(1000);
			p_timer.Elapsed += new ElapsedEventHandler(Tick);
			//p_timer.Enabled = true;

			this.Location = new Point(10000, 0);

			HookManager.KeyDown += new KeyEventHandler(SelectWindow);
		}

		private const int MOUSEEVENTF_LEFTDOWN = 0x02;
		private const int MOUSEEVENTF_LEFTUP = 0x04;

		private void SelectWindow(object i_sender, KeyEventArgs i_args)
		{
			HookManager.KeyDown -= new KeyEventHandler(SelectWindow);
			Keys l_key = i_args.KeyCode;

			//Console.WriteLine("key " + l_key + " control = " + (Control.ModifierKeys == Keys.Control));
			if (Control.ModifierKeys != Keys.Control)
			{
				HookManager.KeyDown += new KeyEventHandler(SelectWindow);
				return;
			}

			if ((l_key == Keys.NumPad4) || (l_key == Keys.NumPad5) || (l_key == Keys.NumPad6))
			{
				if (p_windowType == EWidnowFoundType.NONE)
				{
					HookManager.KeyDown += new KeyEventHandler(SelectWindow);
					return;
				}

				IntPtr l_currentMainWindow = GetForegroundWindow();
				bool l_switchBack = false;
				if (l_currentMainWindow != p_windowHandle)
				{
					l_switchBack = true;
					SetForegroundWindow(p_windowHandle);
					Thread.Sleep(200);
				}

				RECT l_rect;
				if (!GetWindowRect(new HandleRef(this, p_windowHandle), out l_rect))
				{
					HookManager.KeyDown += new KeyEventHandler(SelectWindow);
					return;
				}

				int l_videoWidth, l_videoHeight, l_videoHeightOffset;
				int l_windowX, l_windowY, l_windowWidth, l_windowHeight;
				l_windowX = l_rect.Left;
				l_windowY = l_rect.Top;
				l_windowWidth = l_rect.Right - l_rect.Left;
				l_windowHeight = l_rect.Bottom - l_rect.Top;

				if (p_windowType == EWidnowFoundType.CHROME)
				{

					l_windowY += 122;
					l_videoWidth = 960;
					l_videoHeight = 540;
					l_windowX += (int)((l_windowWidth - l_videoWidth) * 0.5);
				}
				else if ((l_windowHeight * 1.7777777777777) > l_windowWidth)
				{
					l_videoWidth = l_windowWidth;
					l_videoHeight = (int)(l_windowWidth / 1.7777777777777);
				}
				else
				{
					l_videoWidth = l_windowWidth;
					l_videoHeight = l_windowHeight;
				}

				l_videoHeightOffset = (l_windowHeight - l_videoHeight) / 2;
				if (p_windowType == EWidnowFoundType.CHROME)
					l_videoHeightOffset = 0;

				POINT l_currentMousePos;
				GetCursorPos(out l_currentMousePos);
				if(l_key == Keys.NumPad4)
					SetCursorPos(l_windowX + (int)(l_videoWidth * 0.044), l_windowY + (int)(l_videoHeight * 0.964) + l_videoHeightOffset);
				else
					SetCursorPos(l_windowX + (int)(l_videoWidth * 0.1), l_windowY + (int)(l_videoHeight * 0.964) + l_videoHeightOffset);

				mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
				Thread.Sleep(200);
				mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
				mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

				if (l_key == Keys.NumPad6)
				{
					Thread.Sleep(400);
					mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
					mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
					Thread.Sleep(400);
					mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
					mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
				}

				Thread.Sleep(1000);
				SetCursorPos(l_currentMousePos.X, l_currentMousePos.Y);

				if (l_switchBack)
					SetForegroundWindow(l_currentMainWindow);

				HookManager.KeyDown += new KeyEventHandler(SelectWindow);
				return;
			}

			if ((l_key == Keys.NumPad7) || (l_key == Keys.NumPad8))
			{
				if (p_windowType != EWidnowFoundType.ESPN3)
				{
					HookManager.KeyDown += new KeyEventHandler(SelectWindow);
					return;
				}
				if (l_key == Keys.NumPad7)
				{
					p_espn3WidthPercent += 0.02f;
					p_espn3WidthPercent = Math.Min(1.0f, p_espn3WidthPercent);
				}
				else
				{
					p_espn3WidthPercent -= 0.02f;
					p_espn3WidthPercent = Math.Max(0.01f, p_espn3WidthPercent);
				}
				SetSizePosition();
			}

			if ((l_key != Keys.NumPad1) && (l_key != Keys.NumPad2) && (l_key != Keys.NumPad3))
			{
				HookManager.KeyDown += new KeyEventHandler(SelectWindow);
				return;
			}

			StringBuilder l_stringBuilder = new StringBuilder(255);
			IntPtr l_selectedWindow = GetForegroundWindow();
			if (l_selectedWindow == IntPtr.Zero)
			{
				HookManager.KeyDown += new KeyEventHandler(SelectWindow);
				return;
			}

			EWidnowFoundType l_typeToBe;
			if (l_key == Keys.NumPad3)
				l_typeToBe = EWidnowFoundType.ESPN3;
			else
			{
				GetWindowText(l_selectedWindow, l_stringBuilder, 255);
				if (l_stringBuilder.ToString().Contains("Watch NFL Games Online"))
					l_typeToBe = EWidnowFoundType.CHROME;
				else
					l_typeToBe = EWidnowFoundType.ADOBE;
			}

			if ((p_windowHandle == l_selectedWindow) && (p_windowType == l_typeToBe))
			{
				p_timer.Enabled = false;
				p_windowHandle = IntPtr.Zero;
				p_windowType = EWidnowFoundType.NONE;
				Invoke(new VoidBlankDelegate(MakeInvisible));
				HookManager.KeyDown += new KeyEventHandler(SelectWindow);
				return;
			}

			p_windowHandle = l_selectedWindow;
			p_windowType = l_typeToBe;

			if (p_windowType == EWidnowFoundType.ADOBE)
				p_timer.Interval = 1000;
			else if (p_windowType == EWidnowFoundType.CHROME)
				p_timer.Interval = 100;
			else if (p_windowType == EWidnowFoundType.ESPN3)
			{
				p_timer.Interval = 1000;
				p_espn3WidthPercent = 1.0f;
			}


			p_timer.Enabled = true;
			if ((l_key == Keys.NumPad1) || (l_key == Keys.NumPad3))
				SetSizePosition();
			else
				MakeInvisible();

			HookManager.KeyDown += new KeyEventHandler(SelectWindow);
		}

		private void Tick(object source, ElapsedEventArgs e)
		{
			if (p_windowType == EWidnowFoundType.ADOBE)
			{
				if (!IsWindow(p_windowHandle))
				{
					p_windowHandle = IntPtr.Zero;
					p_windowType = EWidnowFoundType.NONE;
					p_timer.Enabled = false;
					Invoke(new VoidBlankDelegate(MakeInvisible));
					return;
				}
			}
			if (p_windowType == EWidnowFoundType.CHROME)
			{
				SetSizePosition();
			}

			Invoke(new VoidBlankDelegate(MakeTopMost));
		}

		private void SetSizePosition()
		{
			RECT l_rect;
			if (!GetWindowRect(new HandleRef(this, p_windowHandle), out l_rect))
			{
				p_windowType = EWidnowFoundType.NONE;
				p_timer.Interval = 1000;
				Invoke(new VoidBlankDelegate(MakeInvisible));
				//Console.WriteLine("get rect failed");
				return;
			}

			int l_videoWidth, l_videoHeight, l_videoHeightOffset;
			int l_windowX, l_windowY, l_windowWidth, l_windowHeight;
			l_windowX = l_rect.Left;
			l_windowY = l_rect.Top;
			l_windowWidth = l_rect.Right - l_rect.Left;
			l_windowHeight = l_rect.Bottom - l_rect.Top;

			if (p_windowType == EWidnowFoundType.CHROME)
			{

				l_windowY += 122;
				l_videoWidth = 960;
				l_videoHeight = 540;
				l_windowX += (int)((l_windowWidth - l_videoWidth) * 0.5);
			}
			else if ((l_windowHeight * 1.7777777777777) > l_windowWidth)
			{
				l_videoWidth = l_windowWidth;
				l_videoHeight = (int)(l_windowWidth / 1.7777777777777);
			}
			else
			{
				l_videoWidth = l_windowWidth;
				l_videoHeight = l_windowHeight;
			}

			l_videoHeightOffset = (l_windowHeight - l_videoHeight) / 2;
			if (p_windowType == EWidnowFoundType.CHROME)
				l_videoHeightOffset = 0;

			float l_barXPercent = 0.0f, l_barYPercent = 0.0f, l_barWidthPercent = 0.0f, l_barHeightPercent = 0.0f;

			if ((p_windowType == EWidnowFoundType.ADOBE) || (p_windowType == EWidnowFoundType.CHROME))
			{
				l_barXPercent = 0.15f;
				l_barYPercent = 0.904f;
				l_barWidthPercent = 0.666f;
				l_barHeightPercent = 0.0515f;
			}
			else if (p_windowType == EWidnowFoundType.ESPN3)
			{
				l_barXPercent = 1.0f - p_espn3WidthPercent;
				l_barYPercent = 0.904f;
				l_barWidthPercent = p_espn3WidthPercent;
				l_barHeightPercent = 1.0f - l_barYPercent;
			}

			Invoke(new SetBoundsDelegate(SetTheBounds),
				new object[]{
						l_windowX + (int)(l_videoWidth * l_barXPercent), 
						l_windowY + (int)(l_videoHeight * l_barYPercent) + l_videoHeightOffset, 
						(int)(l_videoWidth * l_barWidthPercent),
						(int)(l_videoHeight * l_barHeightPercent) + 1
					});
		}

		private void WorkMexicanWork()
		{

			bool l_adobeFoundThisTime = true;
			if (p_windowType == EWidnowFoundType.NONE)
			{
				Process[] l_processes = Process.GetProcesses();
				for (int i = 0; i < l_processes.Length; ++i)
				{
					if (l_processes[i].MainWindowTitle == "Adobe Flash Player")
					{
						p_windowType = EWidnowFoundType.ADOBE;
						p_windowHandle = l_processes[i].MainWindowHandle;
						l_adobeFoundThisTime = true;
						p_timer.Interval = 1000;
						//Console.WriteLine("found adobe from none");
						break;
					}
					else if (l_processes[i].MainWindowTitle == "Watch NFL Games Online | Stream Recaps with NFL Game Rewind - NFL.com - Google Chrome")
					{
						p_windowType = EWidnowFoundType.CHROME;
						p_windowHandle = l_processes[i].MainWindowHandle;
						p_timer.Interval = 100;
						//Console.WriteLine("found chrome");
					}
				}
			}
			else if (p_windowType == EWidnowFoundType.CHROME)
			{
				Process[] l_processes = Process.GetProcesses();
				for (int i = 0; i < l_processes.Length; ++i)
				{
					if (l_processes[i].MainWindowTitle == "Adobe Flash Player")
					{
						p_windowType = EWidnowFoundType.ADOBE;
						p_windowHandle = l_processes[i].MainWindowHandle;
						l_adobeFoundThisTime = true;
						p_timer.Interval = 1000;
						//Console.WriteLine("found adobe from chrome");
						break;
					}
				}
			}

			if ((p_windowType == EWidnowFoundType.CHROME) || l_adobeFoundThisTime) //if we need to set the window size
			{
				
			}
			else if (p_windowType == EWidnowFoundType.ADOBE)
			{
				if (!IsWindow(p_windowHandle))
				{
					p_windowType = EWidnowFoundType.NONE;
					p_timer.Interval = 1000;
					//Console.WriteLine("Adobe window GONE!!!!");
					Invoke(new VoidBlankDelegate(MakeInvisible));
					WorkMexicanWork();
					return;
				}
				Invoke(new VoidBlankDelegate(MakeTopMost));
				//Console.WriteLine("Adobe window stll here");
			}
			//if (p_windowType == EWidnowFoundType.NONE)
				//Console.WriteLine("No window found");
		}

		void SetTheBounds(int i_x, int i_y, int i_width, int i_height)
		{
			SetBounds(i_x, i_y, i_width, i_height);
		}

		void MakeInvisible()
		{
			Location = new Point(10000, 0);
		}

		void MakeTopMost()
		{
			TopMost = true;
		}

	}

	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new ScoreBlocker());
		}
	}
}
