// (c) 2022 BlindEye Softworks. All rights reserved.

using System;

namespace GoToUEFI
{
    [Flags]
    public enum TOKEN_ACCESS_RIGHT : uint
    {
        TOKEN_ADJUST_PRIVILEGES = 0x20
    }
}