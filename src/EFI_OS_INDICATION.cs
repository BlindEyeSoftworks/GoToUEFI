// (c) 2022 BlindEye Softworks. All rights reserved.

using System;

namespace GoToUEFI
{
    [Flags]
    public enum EFI_OS_INDICATION : ulong
    {
        BOOT_TO_FW_UI = 0x1
    }
}