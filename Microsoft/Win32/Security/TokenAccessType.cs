using System;

namespace Microsoft.Win32.Security
{
	[Flags]
	public enum TokenAccessType : uint
	{
		TOKEN_ASSIGN_PRIMARY = 0x1u,
		TOKEN_DUPLICATE = 0x2u,
		TOKEN_IMPERSONATE = 0x4u,
		TOKEN_QUERY = 0x8u,
		TOKEN_QUERY_SOURCE = 0x10u,
		TOKEN_ADJUST_PRIVILEGES = 0x20u,
		TOKEN_ADJUST_GROUPS = 0x40u,
		TOKEN_ADJUST_DEFAULT = 0x80u,
		TOKEN_ADJUST_SESSIONID = 0x100u,
		TOKEN_ALL_ACCESS = 0xF01FFu,
		TOKEN_READ = 0x20008u,
		TOKEN_WRITE = 0x200E0u,
		TOKEN_EXECUTE = 0x20000u
	}
}
