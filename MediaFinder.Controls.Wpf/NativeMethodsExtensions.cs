using System.Buffers;
using System.Diagnostics;
using System.Numerics;

namespace Windows.Win32;

internal static partial class PInvoke
{
    internal static string StrFormatByteSize<T>(T number, int bufferSize = 32)
        where T : INumber<T>
        => StrFormatByteSize(Convert.ToInt64(number), bufferSize);

    internal unsafe static string StrFormatByteSize(long number, int bufferSize = 32)
    {
        var buffer = ArrayPool<char>.Shared.Rent(bufferSize);
        try
        {
            fixed (char* ptr = buffer)
            {
                Debug.Assert(OperatingSystem.IsWindowsVersionAtLeast(5));
                var pBuf = StrFormatByteSize(number, ptr, (uint)bufferSize);

                return pBuf.ToString();
            }
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }
}
