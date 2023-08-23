// Copyright (c) 2020 Benito Palacios Sánchez (pleonex)

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
    using System.IO;
    using System.Threading.Tasks;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    public static class Program
    {
        public static Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("Deobfuscator of AP code in NDS games");

            var ninoOv19 = new Option<string>("--overlay19", "The overlay9_19 with AP to deobfuscate");
            var ninokuni = new Command("ninokuni", "Deobfuscate the files from Ninokuni") { ninoOv19 };
            ninokuni.SetHandler(DeobfuscateNinokuni, ninoOv19);
            rootCommand.AddCommand(ninokuni);

            var vpypOv8 = new Option<string>("--overlay8", "The overlay9_8 with AP to deobfuscate");
            var vpypOv9 = new Option<string>("--overlay9", "The overlay9_9 with AP to deobfuscate");
            var vpyp = new Command("vpyp", "Deobfuscate the files from Pokemon Conquest US") { vpypOv9, vpypOv8 };
            vpyp.SetHandler(DeobfuscateVpyp, vpypOv8, vpypOv9);
            rootCommand.AddCommand(vpyp);

            return rootCommand.InvokeAsync(args);
        }

        public static void DeobfuscateNinokuni(string overlay19)
        {
            if (!File.Exists(overlay19)) {
                Console.WriteLine("ERROR: The overlay file does not exist!");
                return;
            }

            Ninokuni.Decrypt(overlay19);
        }

        public static void DeobfuscateVpyp(string overlay8Path, string overlay9Path)
        {
            var overlay8Info = new AntiPiracyOverlayInfo {
                OverlayRamAddress = 0x02215BE0,
                AddressOffset = 0x2200,
                IntegerOffset = 0x22174C0,
                InfrastructureEncryption = new() {
                    KeySeed = 0xA471ABB,
                    DecryptionCallers = new() {
                        0x02215ebc,
                        0x02216274,
                        0x0221656c,
                        0x02216ee0,
                        0x02217418,
                    },
                },
                JumpFunctions = new() {
                    new AntiPiracyJumpFunction(0x02215e10, 0x25, CodeChecksumKind.VPY, 0x9F75A8D6),
                },
            };

            string inputDirectory = Path.GetDirectoryName(Path.GetFullPath(overlay8Path));

            using Node overlay8 = NodeFactory.FromFile(overlay8Path, FileOpenMode.Read);
            overlay8.TransformWith(new Deobfuscator(overlay8Info))
                .Stream.WriteTo($"{inputDirectory}/overlay9_8.new.bin");
        }
    }
}
