using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lemon.Automation.BrowserNativeHost
{
    internal class PipeProtocol
    {
        private Stream m_inputStream;

        private Stream m_outputStream;

        private byte[] m_inputBuffer = new byte[1024];

        public PipeProtocol(PipeStream ioStream)
            : this(ioStream, ioStream)
        {
        }

        public PipeProtocol(Stream inputStream, Stream outputStream)
        {
            m_inputStream = inputStream;
            m_outputStream = outputStream;
        }

        public ReadOnlySequence<byte> Read()
        {
            if (m_inputStream.Read(m_inputBuffer, 0, 4) == 0)
            {
                return ReadOnlySequence<byte>.Empty;
            }
            int num = BitConverter.ToInt32(m_inputBuffer, 0);
            if (num == 0)
            {
                return ReadOnlySequence<byte>.Empty;
            }
            if (num > m_inputBuffer.Length)
            {
                m_inputBuffer = new byte[num];
            }
            int num2 = 0;
            int num3 = num;
            while (num3 > 0)
            {
                int num4 = m_inputStream.Read(m_inputBuffer, num2, num3);
                num3 -= num4;
                num2 += num4;
            }
            return new ReadOnlySequence<byte>(m_inputBuffer, 0, num);
        }

        public async Task<ReadOnlySequence<byte>> ReadAsync(CancellationToken cancelToken)
        {
            if (await m_inputStream.ReadAsync(m_inputBuffer, 0, 4, cancelToken) == 0)
            {
                return ReadOnlySequence<byte>.Empty;
            }
            int msgSize = BitConverter.ToInt32(m_inputBuffer, 0);
            if (msgSize == 0)
            {
                return ReadOnlySequence<byte>.Empty;
            }
            if (msgSize > m_inputBuffer.Length)
            {
                m_inputBuffer = new byte[msgSize];
            }
            int readOffset = 0;
            int remainingBytes = msgSize;
            while (remainingBytes > 0)
            {
                int readBytes = await m_inputStream.ReadAsync(m_inputBuffer, readOffset, remainingBytes, cancelToken);
                remainingBytes -= readBytes;
                readOffset += readBytes;
            }
            return new ReadOnlySequence<byte>(m_inputBuffer, 0, msgSize);
        }

        public void Write(byte[] msgBuffer)
        {
            byte[] bytes = BitConverter.GetBytes(msgBuffer.Length);
            m_outputStream.Write(bytes, 0, bytes.Length);
            if (msgBuffer.Length != 0)
            {
                m_outputStream.Write(msgBuffer, 0, msgBuffer.Length);
            }
            m_outputStream.Flush();
        }

        public async Task WriteAsync(byte[] msgBuffer, CancellationToken cancelToken)
        {
            byte[] msgSizeBuffer = BitConverter.GetBytes(msgBuffer.Length);
            await m_outputStream.WriteAsync(msgSizeBuffer, 0, msgSizeBuffer.Length, cancelToken);
            if (msgBuffer.Length != 0)
            {
                await m_outputStream.WriteAsync(msgBuffer, 0, msgBuffer.Length, cancelToken);
            }
            await m_outputStream.FlushAsync(cancelToken);
        }
    }
}
