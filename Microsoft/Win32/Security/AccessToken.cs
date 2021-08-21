using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.Security
{
	public abstract class AccessToken : DisposableObject
	{
		private IntPtr _handle;

		protected internal AccessToken(IntPtr handle)
		{
			_handle = handle;
		}

		protected override void Dispose(bool disposing)
		{
			if (_handle != IntPtr.Zero && Win32.CloseHandle(_handle) != 0)
			{
				_handle = IntPtr.Zero;
			}
		}

		public void EnablePrivilege(TokenPrivilege privilege)
		{
			TokenPrivileges privileges = new TokenPrivileges { privilege };
			EnableDisablePrivileges(privileges);
		}

		private void EnableDisablePrivileges(TokenPrivileges privileges)
		{
			UnsafeEnableDisablePrivileges(privileges);
		}

		private unsafe void UnsafeEnableDisablePrivileges(TokenPrivileges privileges)
		{
			fixed (byte* ptr = privileges.GetNativeTokenPrivileges())
			{
				Win32.SetLastError(0u);
				Win32.CheckCall(Win32.AdjustTokenPrivileges(_handle, 0, (IntPtr)ptr, 0u, IntPtr.Zero, out var _));
				if (Marshal.GetLastWin32Error() == 1300)
				{
					Win32.ThrowLastError();
				}
			}
		}
	}
}
