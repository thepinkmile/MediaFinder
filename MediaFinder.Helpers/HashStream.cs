using System.Security.Cryptography;
using System.Text;

namespace MediaFinder.Helpers;
// Copyright 2018 Steve Streeting
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

/// <summary>
/// Passthrough stream which calculates a hash on all the bytes read or written.
/// This is a useful alternative to CryptoStream if you don't want the data to be
/// encrypted, but still want to calculate a hash on the data in a transparent way.
/// </summary>
public class HashStream : Stream
{

    protected Stream _target;
    protected byte[] _passphrase;
    protected Dictionary<string, HashAlgorithm?> _hashes;
    protected bool _finalized;

    /// <summary>
    /// Standard constructor
    /// </summary>
    /// <param name="targetStream">The stream to pass data to, or read data from</param>
    /// <param name="hashAlgorithms">The hash algorithms to use, e.g. SHA256Managed</param>
    public HashStream(Stream targetStream, params HashAlgorithmName[] hashAlgorithms)
        : this(targetStream, Array.Empty<byte>(), hashAlgorithms)
    {
    }

    public HashStream(Stream targetStream, string passphrase, params HashAlgorithmName[] hashAlgorithms)
        : this(targetStream, Encoding.UTF8.GetBytes(passphrase), hashAlgorithms)
    {
    }

    public HashStream(Stream targetStream, byte[] passphrase, params HashAlgorithmName[] hashAlgorithms)
    {
        _target = targetStream;
        _passphrase = passphrase;
        _hashes = hashAlgorithms
            .Where(x => x.Name is not null)
            .Distinct()
            .ToDictionary(x => x.Name!, x => CryptoConfig.CreateFromName(x.Name!) as HashAlgorithm);
        foreach (var hash in _hashes.Where(x => x.Value is null).ToList())
        {
            _hashes.Remove(hash.Key);
        }
    }

    /// <see cref="Stream"/>
    public override bool CanRead
    {
        get { return _target.CanRead; }
    }

    /// <see cref="Stream"/>
    public override bool CanSeek
    {
        get { return _target.CanSeek; }
    }

    /// <see cref="Stream"/>
    public override bool CanWrite
    {
        get { return _target.CanWrite; }
    }

    /// <see cref="Stream"/>
    public override long Length
    {
        get { return _target.Length; }
    }

    /// <see cref="Stream"/>
    public override long Position
    {
        get { return _target.Position; }
        set { _target.Position = value; }
    }

    /// <see cref="Stream"/>
    public override void Flush()
    {
        _target.Flush();
    }

    /// <see cref="Stream"/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        int ret = _target.Read(buffer, offset, count);
        foreach (var hash in _hashes.Values.Where(x => x is not null))
        {
            hash!.TransformBlock(buffer, offset, ret, buffer, offset);
        }
        return ret;
    }

    /// <see cref="Stream"/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        return _target.Seek(offset, origin);
    }

    /// <see cref="Stream"/>
    public override void SetLength(long value)
    {
        _target.SetLength(value);
    }

    /// <see cref="Stream"/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        _target.Write(buffer, offset, count);
        foreach (var hash in _hashes.Values.Where(x => x is not null))
        {
            hash!.TransformBlock(buffer, offset, count, buffer, offset);
        }
    }

    /// <summary>
    /// Calculate final hash for the content which has been written or read to
    /// the target stream so far.
    /// </summary>
    /// <param name="hashName">The hash to retrieve a value for.</param>
    /// <remarks>
    /// Consider using the overloaded method which takes a passphrase if you want
    /// an additional factor other than just the stream data.
    /// </remarks>
    /// <returns>The hash value</returns>
    /// <exception cref="ArgumentException">Thrown if the requested algorithm name is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the requested algorithm was not specified in the constructor.</exception>
    public byte[] Hash(HashAlgorithmName hashName)
    {
        if (hashName.Name is null)
        {
            throw new ArgumentNullException(nameof(hashName), "Invalid HashAlgorithmName");
        }
        if (!_hashes.TryGetValue(hashName.Name, out HashAlgorithm? value))
        {
            throw new InvalidOperationException("HashAlgorithm name not configured for generation");
        }

        if (!_finalized)
        {
            foreach (var hash in _hashes.Values.Where(x => x is not null))
            {
                hash!.TransformFinalBlock(_passphrase, 0, _passphrase.Length);
            }
        }

        return value?.Hash ?? [];
    }
}
