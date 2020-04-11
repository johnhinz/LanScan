using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace lanscan
{
    class Program
    {

        [DllImport("iphlpapi.dll", ExactSpelling=true)]
        public static extern int SendARP( int destIp, int srcIP, byte[] macAddr, ref uint physicalAddrLen );
        static void Main(string[] args)
        {


            Object obj = new object();

            RangeFinder range = new RangeFinder();
            IPAddress startIP = IPAddress.Parse("192.168.0.1");
            IPAddress endIP = IPAddress.Parse("192.168.0.254");
            IEnumerable<string> ipAddresses = range.GetIPRange(startIP, endIP);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            CancellationTokenSource cts = new CancellationTokenSource();
            //cts.CancelAfter(300);
          
            ParallelOptions options = new ParallelOptions();
            options.CancellationToken = cts.Token;



            Parallel.ForEach(ipAddresses, options, (ipAddress)=>
            {

                IPAddress dst = IPAddress.Parse(ipAddress); // the destination IP address

                // Ping ping = new Ping();
                // PingReply pingReply = ping.Send(dst, 300);
                // if (pingReply.Status == IPStatus.Success)
                // {

                    byte[] macAddr = new byte[6];
                    uint macAddrLen = (uint)macAddr.Length;

                    lock (obj)
                    {
                        if (SendARP(BitConverter.ToInt32(dst.GetAddressBytes(), 0), 0, macAddr, ref macAddrLen) == 0)
                        {

                            string[] str = new string[(int)macAddrLen];
                            for (int i=0; i<macAddrLen; i++)
                                str[i] = macAddr[i].ToString("x2");

                            Console.WriteLine($"{ipAddress}  {string.Join(":", str)}");
                        }
                    }

                    options.CancellationToken.ThrowIfCancellationRequested();
                //}
            });

            sw.Stop();
            System.Console.WriteLine(sw.Elapsed);
        }
    }
}
