using System;

namespace Microsoft.Win32.Security
{
	public abstract class DisposableObject : IDisposable
	{
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		~DisposableObject()
		{
			Dispose(disposing: false);
		}

		protected abstract void Dispose(bool disposing);
	}
}
