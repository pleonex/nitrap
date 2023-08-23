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
using System.Collections.ObjectModel;

namespace Nitraper;

public class AntiPiracyOverlayInfo
{
    /// <summary>
    /// Gets or sets the address in RAM of the overlay.
    /// </summary>
    public uint OverlayRamAddress { get; set; }

    /// <summary>
    /// Gets or sets the offsets applied to every address (and integer) to
    /// obfuscate, so it doesn't point anywhere.
    /// It can be found in the data pool of the XOR decryption func.
    /// </summary>
    public uint AddressOffset { get; set; }

    /// <summary>
    /// Gets or sets the constant applied to small integers like lengths to make
    /// them look like an address. It's usually the end point of the AP overlay
    /// in the RAM plus some random small offset (0 to 16).
    /// It can be found as part of the XOR decryption func.
    /// </summary>
    public uint IntegerOffset { get; set; }

    public InfrastructureEncryption InfrastructureEncryption { get; set; }

    public Collection<AntiPiracyJumpFunction> JumpFunctions { get; set; }

    public Collection<AntiPiracyCodeEncryption> CodeEncryptions { get; set; }
}

public record InfrastructureEncryption
{
    public uint KeySeed { get; init; }
    public Collection<uint> DecryptionCallers { get; init; }
}

public record AntiPiracyJumpFunction(
    uint Address,
    int Length,
    CodeChecksumKind Kind,
    uint ExpectedChecksum);

public enum CodeChecksumKind
{
    Unknown,
    Ninokuni,
    VPY,
}

public record AntiPiracyCodeEncryption(
    uint Address,
    uint Length,
    uint KeySeed);
