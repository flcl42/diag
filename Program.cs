using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Socket serv = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
IPEndPoint addr = IPEndPoint.Parse(args[0]);
serv.Bind(addr);
Console.WriteLine($"LIST addr={IPEndPoint.Parse(args[0])}");

_ = Task.Run(() =>
{
    try
    {
        byte[] buf = new byte[1280];
        while (true)
        {
            EndPoint e = new IPEndPoint(IPAddress.Any, 0);
            int received = serv.ReceiveFrom(buf, ref e);
            Console.WriteLine($"RECV {received} bytes from {e}: {Convert.ToHexStringLower(buf.AsSpan(0, received))}");
            IPEndPoint ee = (IPEndPoint)e;
            byte[] send = new byte[32];
            ee.Address.MapToIPv4().GetAddressBytes().CopyTo(send, 0);
            BinaryPrimitives.WriteUInt16BigEndian(send.AsSpan(4, 2), (ushort)ee.Port);
            serv.SendTo(send, e);
            Console.WriteLine($"SENT {received} bytes from {e}: {Convert.ToHexStringLower(send)}");

        }
    }
    catch (Exception e)
    {
        Console.WriteLine($"EROR {e}");
    }
});


if (args is { Length: 2 })
{
    byte[] data = Encoding.UTF8.GetBytes("hi");
    Console.WriteLine($"SEND {data} bytes to {args[1]}");

    serv.SendTo(Encoding.UTF8.GetBytes("hi"), IPEndPoint.Parse(args[1]));
}

Console.ReadLine();
Console.WriteLine("EXIT");

serv.Disconnect(false);
