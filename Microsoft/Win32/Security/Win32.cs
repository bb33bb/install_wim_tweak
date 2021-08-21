using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.Security.Win32Structs;

namespace Microsoft.Win32.Security
{
	public class Win32
	{
		public const int FALSE = 0;

		public const int TRUE = 1;

		public const int SUCCESS = 0;

		public const int ERROR_SUCCESS = 0;

		public const int ERROR_INSUFFICIENT_BUFFER = 122;

		public const int ERROR_NOT_ALL_ASSIGNED = 1300;

		public const int ERROR_NONE_MAPPED = 1332;

		private const string Kernel32 = "kernel32.dll";

		private const string Advapi32 = "Advapi32.dll";

		public static uint GetLastError()
		{
			return (uint)Marshal.GetLastWin32Error();
		}

		public static void ThrowLastError()
		{
			Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
		}

		public static void CheckCall(bool funcResult)
		{
			if (!funcResult)
			{
				ThrowLastError();
			}
		}

		public static void CheckCall(int funcResult)
		{
			CheckCall(funcResult != 0);
		}

		public static void CheckCall(IntPtr funcResult)
		{
			CheckCall(!IsNullHandle(funcResult));
		}

		public static bool IsNullHandle(IntPtr ptr)
		{
			return ptr == IntPtr.Zero;
		}

		[DllImport("kernel32.dll")]
		public static extern void SetLastError(uint dwErrCode);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr OpenProcess(ProcessAccessType dwDesiredAccess, int bInheritHandle, uint dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern int CloseHandle(IntPtr handle);

		[DllImport("Advapi32.dll", SetLastError = true)]
		public static extern int OpenProcessToken(IntPtr hProcess, TokenAccessType dwDesiredAccess, out IntPtr hToken);

		[DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int LookupPrivilegeValue(string lpSystemName, string lpName, out LUID Luid);

		[DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int AdjustTokenPrivileges(IntPtr TokenHandle, int DisableAllPrivileges, IntPtr NewState, uint BufferLength, IntPtr PreviousState, out uint ReturnLength);
	}
}
