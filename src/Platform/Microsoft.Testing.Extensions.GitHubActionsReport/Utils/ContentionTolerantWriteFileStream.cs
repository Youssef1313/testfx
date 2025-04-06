// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Testing.Extensions.GitHubActionsReport;

internal sealed class ContentionTolerantWriteFileStream(string filePath, FileMode fileMode) : Stream
{
    private readonly List<byte> _buffer = new(1024);

    [ExcludeFromCodeCoverage]
    public override bool CanRead => false;

    [ExcludeFromCodeCoverage]
    public override bool CanSeek => false;

    [ExcludeFromCodeCoverage]
    public override bool CanWrite => true;

    [ExcludeFromCodeCoverage]
    public override long Length => _buffer.Count;

    [ExcludeFromCodeCoverage]
    public override long Position { get; set; }

    // Backoff and retry if the file is locked
    private FileStream CreateInnerStream()
    {
        for (int retriesRemaining = 10; ; retriesRemaining--)
        {
            try
            {
                return new FileStream(filePath, fileMode);
            }
            catch (IOException) when (retriesRemaining > 0)
            {
                // Variance in delay to avoid overlapping back-offs
                Thread.Sleep(RandomEx.Shared.Next(200, 1000));
            }
        }
    }

    public override void Write(byte[] buffer, int offset, int count) =>
        _buffer.AddRange(buffer.Skip(offset).Take(count));

    public override void Flush()
    {
        using FileStream stream = CreateInnerStream();
        stream.Write(_buffer.ToArray(), 0, _buffer.Count);
    }

    [ExcludeFromCodeCoverage]
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _buffer.Clear();
    }

    [ExcludeFromCodeCoverage]
    public override int Read(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();

    [ExcludeFromCodeCoverage]
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    [ExcludeFromCodeCoverage]
    public override void SetLength(long value) => throw new NotSupportedException();
}
