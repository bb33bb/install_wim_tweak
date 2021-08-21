using System;

namespace Microsoft.Win32.Security
{
	[Flags]
	public enum PrivilegeAttributes : uint
	{
		Disabled = 0x0u,
		EnabledByDefault = 0x1u,
		Enabled = 0x2u,
		UsedForAccess = 0x80000000u
	}
}
