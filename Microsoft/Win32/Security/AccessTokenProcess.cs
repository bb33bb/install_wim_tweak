using System;

namespace Microsoft.Win32.Security
{
	public class AccessTokenProcess : AccessToken
	{
		public AccessTokenProcess(int pid, TokenAccessType desiredAccess)
			: base(OpenProcessToken(pid, desiredAccess))
		{
		}

		private static IntPtr TryOpenProcessToken(int pid, TokenAccessType desiredAccess)
		{
			IntPtr intPtr = Win32.OpenProcess(ProcessAccessType.PROCESS_QUERY_INFORMATION, 0, (uint)pid);
			if (intPtr == IntPtr.Zero)
			{
				return IntPtr.Zero;
			}
			Win32.CheckCall(intPtr);
			try
			{
				if (Win32.OpenProcessToken(intPtr, desiredAccess, out var hToken) == 0)
				{
					return IntPtr.Zero;
				}
				return hToken;
			}
			finally
			{
				Win32.CloseHandle(intPtr);
			}
		}

		private static IntPtr OpenProcessToken(int pid, TokenAccessType desiredAccess)
		{
			IntPtr intPtr = TryOpenProcessToken(pid, desiredAccess);
			if (intPtr == IntPtr.Zero)
			{
				Win32.ThrowLastError();
			}
			return intPtr;
		}
	}
}
