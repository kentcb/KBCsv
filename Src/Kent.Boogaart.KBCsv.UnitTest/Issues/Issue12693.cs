using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace Kent.Boogaart.KBCsv.UnitTest.Issues
{
    // This issue was caused by the following sequence of events:
    //  1. the StreamReader being used by the CsvParser notices it gets back less data than it asked for (mimicked in the fake server below by using a very small buffer)
    //  2. StreamReader therefore sets a flag indicating that it is blocked
    //  3. StreamReader.Peek therefore returns -1, indicating it has no data to peek at
    //  4. The old CsvParser therefore assumed there was no more data, and stopped parsing
    //
    // This was particularly problematic when passing a NetworkStream to the CsvReader. Since a NetworkStream does not support seeking, a StreamReader wrapped around it
    // will return -1 if it is waiting for data from the stream. This caused the CsvParser to think there was no more data.
    //
    // The new CsvParser does not use peeking at all, so it passes this test no problem.
    public sealed class Issue12693
    {
        [Fact]
        public void issue12693_repro()
        {
            var recordCount = 50;
            var random = new Random();
            var memoryStream = new MemoryStream();
            var csvWriter = new CsvWriter(memoryStream);

            // create some dummy data to work with
            for (var record = 0; record < recordCount; ++record)
            {
                var dataRecord = new DataRecord(null);

                for (var value = 0; value < random.Next(100, 200); ++value)
                {
                    dataRecord.Values.Add("value" + random.Next(0, 100000));
                }

                csvWriter.WriteDataRecord(dataRecord);
            }

            memoryStream.Position = 0;

            // create a fake server that will feed us our dummy data in dribs and drabs
            using (var fakeServer = new FakeServer(memoryStream))
            using (var connectionToServer = new TcpClient("localhost", FakeServer.Port))
            using (var csvReader = new CsvReader(connectionToServer.GetStream()))
            {
                while (csvReader.HasMoreRecords)
                {
                    csvReader.ReadDataRecordAsStrings();
                }

                // when the bug manifests itself, the parser may stop parsing prematurely so the record counts won't match
                // see below for an explanation of how the bug is triggered
                Assert.Equal(recordCount, csvReader.RecordNumber);
            }
        }

        private sealed class FakeServer : IDisposable
        {
            public const int Port = 9999;
            private readonly Socket socket;
            private readonly Stream dataSource;
            private Socket clientSocket;
            private NetworkStream clientStream;

            public FakeServer(Stream dataSource)
            {
                this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.dataSource = dataSource;

                var ipAddress = new IPAddress(new byte[] { 0, 0, 0, 0 });
                var endPoint = new IPEndPoint(ipAddress, Port);
                this.socket.Bind(endPoint);
                this.socket.Listen(int.MaxValue);
                this.socket.BeginAccept(this.OnSocketAccepted, null);
            }

            public void Dispose()
            {
                this.socket.Dispose();
            }

            private void OnSocketAccepted(IAsyncResult asyncResult)
            {
                this.clientSocket = this.socket.EndAccept(asyncResult);
                this.clientStream = new NetworkStream(this.clientSocket, false);

                // very small buffer, which will contain less data than what the client asks for
                var buffer = new byte[10];
                var read = 0;

                while ((read = this.dataSource.Read(buffer, 0, buffer.Length)) != 0)
                {
                    this.clientStream.Write(buffer, 0, read);
                }

                this.dataSource.CopyTo(this.clientStream);
                this.clientStream.Dispose();
                this.clientSocket.Dispose();
            }
        }
    }
}
