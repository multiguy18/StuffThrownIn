using System;
using System.Threading;
using Sanford.Multimedia.Midi;

namespace MidiFlood
{
    class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();

            Console.WriteLine("Number of devices: {0}", OutputDeviceBase.DeviceCount);
            Console.Write("Select the output device: ");

            int deviceNr;
            if(int.TryParse(Console.ReadLine(), out deviceNr))
            {
                if (deviceNr >= OutputDeviceBase.DeviceCount || deviceNr < 0)
                {
                    deviceNr = 0;
                }
            }


            using (OutputDevice device = new OutputDevice(deviceNr))
            {
                ChannelMessageBuilder builder = new ChannelMessageBuilder();

                do
                {
                    builder.MidiChannel = random.Next(16);
                    builder.Command = (ChannelCommand)Enum.GetValues(typeof(ChannelCommand)).PeekRandom();

                    //if (builder.Command == ChannelCommand.ProgramChange && random.Next(256) > 5)
                        //continue;

                    builder.Data1 = random.Next(127);
                    builder.Data2 = random.Next(127);

                    builder.Build();

                    device.Send(builder.Result);

                    Console.WriteLine("Channel: {0}\tCommand:{1}\tData1: {2}\tData2: {3}",
                        builder.MidiChannel, builder.Command, builder.Data1, builder.Data2);

                    Thread.Sleep(10);
                } while (Console.KeyAvailable == false);
            }
        }
    }
}
