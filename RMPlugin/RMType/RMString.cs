using System;
using System.Runtime.InteropServices;

namespace RMPlugin
{
    namespace RMType
    {
        public class RMString : IDisposable
        {
            ~RMString() => Dispose(false);

            private bool m_disposed = false;

            private IntPtr m_inner = IntPtr.Zero;

            public static implicit operator RMString(IntPtr value) => new RMString() { m_inner = value };

            public static implicit operator IntPtr(RMString value) => value.m_inner;

            public static implicit operator RMString(string value) => new RMString() { m_inner = Marshal.StringToHGlobalUni(value) };

            public static implicit operator string(RMString value) => value.m_inner != IntPtr.Zero ? Marshal.PtrToStringUni(value.m_inner) : string.Empty;

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!m_disposed)
                {
                    if (m_inner != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(m_inner);
                        m_inner = IntPtr.Zero;
                    }
                    m_disposed = true;
                }
            }
        }
    }
}
