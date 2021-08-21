#define DEBUG
using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.Security.Win32Structs;

namespace Microsoft.Win32.Security
{
	public class TokenPrivileges : CollectionBase
	{
		public TokenPrivilege this[int index] => (TokenPrivilege)base.InnerList[index];

		public void Add(TokenPrivilege privilege)
		{
			base.InnerList.Add(privilege);
		}

		public unsafe byte[] GetNativeTokenPrivileges()
		{
			Debug.Assert(Marshal.SizeOf(typeof(TOKEN_PRIVILEGES)) == 4);
			TOKEN_PRIVILEGES structure = default(TOKEN_PRIVILEGES);
			structure.PrivilegeCount = (uint)base.Count;
			byte[] array = new byte[Marshal.SizeOf(typeof(TOKEN_PRIVILEGES)) + Marshal.SizeOf(typeof(LUID_AND_ATTRIBUTES)) * base.Count];
			fixed (byte* ptr = array)
			{
				Marshal.StructureToPtr(structure, (IntPtr)ptr, fDeleteOld: false);
			}
			int num = Marshal.SizeOf(typeof(TOKEN_PRIVILEGES));
			for (int i = 0; i < base.Count; i++)
			{
				byte[] nativeLUID_AND_ATTRIBUTES = this[i].GetNativeLUID_AND_ATTRIBUTES();
				Array.Copy(nativeLUID_AND_ATTRIBUTES, 0, array, num, nativeLUID_AND_ATTRIBUTES.Length);
				num += nativeLUID_AND_ATTRIBUTES.Length;
			}
			return array;
		}
	}
}
