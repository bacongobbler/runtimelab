// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Generated by Fuzzlyn v1.5 on 2021-10-12 17:42:07
// Run on .NET 6.0.0-rc.1.21451.13 on X64 Windows
// Seed: 4133580165890247722

using System.Runtime.CompilerServices;

public class Program
{
    public static int Main() => Test(31) == -65538 ? 100 : 0;

    [MethodImpl(MethodImplOptions.NoInlining)]
    static int Test(int x) => -(1 << x) / 32767;
}