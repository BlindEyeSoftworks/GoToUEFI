// (c) 2022 BlindEye Softworks. All rights reserved.

using System;

namespace GoToUEFI
{
    [Flags]
    public enum SHTDN_REASON : uint
    {
        SHTDN_REASON_MAJOR_OTHER = 0x0,
        SHTDN_REASON_MINOR_OTHER = 0x0,
        SHTDN_REASON_FLAG_PLANNED = 0x80000000,
        PLANNED = SHTDN_REASON_MAJOR_OTHER | SHTDN_REASON_MINOR_OTHER | SHTDN_REASON_FLAG_PLANNED
    }
}