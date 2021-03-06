﻿// Copyright (c) 2020 Benito Palacios Sánchez (pleonex)

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
namespace Nitraper
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;
    using System.Threading.Tasks;

    public static class Program
    {
        public static Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("Deobfuscator of AP code in NDS games");

            var ninokuni = new Command("ninokuni", "Deobfuscate the files from Ninokuni");
            ninokuni.AddOption(new Option<string>("--overlay19", "The overlay9_19 with AP to deobfuscate"));
            ninokuni.Handler = CommandHandler.Create<string>(DeobfuscateNinokuni);
            rootCommand.AddCommand(ninokuni);

            return rootCommand.InvokeAsync(args);
        }

        public static int DeobfuscateNinokuni(string overlay19)
        {
            if (!File.Exists(overlay19)) {
                Console.WriteLine("ERROR: The overlay file does not exist!");
                return 10;
            }

            Ninokuni.Decrypt(overlay19);
            return 0;
        }
    }
}
