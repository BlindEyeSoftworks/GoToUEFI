// (c) 2022 BlindEye Softworks. All rights reserved.

using System;

namespace GoToUEFI
{
    [Flags]
    public enum SHUTDOWN_TYPE : uint
    {
        EWX_REBOOT = 0x2
    }
}