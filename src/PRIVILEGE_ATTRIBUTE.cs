// (c) 2022 BlindEye Softworks. All rights reserved.

using System;

namespace GoToUEFI
{
    [Flags]
    public enum PRIVILEGE_ATTRIBUTE : uint
    {
        SE_PRIVILEGE_ENABLED = 0x2
    }
}