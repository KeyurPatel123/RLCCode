using System;

namespace Abiomed.DotNetCore.MailConsole
{
    class Program
    {
        static private Mail _mail;
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Initialize();

            _mail.SendEmailAsync(@"rlussier@abiomed.com", @"test",@"This is a simple validation/poc test").Wait();
        }

        static private void Initialize()
        {
            _mail = new Mail();
        }
    }
}