// (c) 2022 BlindEye Softworks. All rights reserved.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GoToUEFI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AdjustPrivileges(PRIVILEGE_NAME.SE_SYSTEM_ENVIRONMENT_NAME,
                             PRIVILEGE_NAME.SE_SHUTDOWN_NAME);

            /* Allocate memory for the OsIndications variable which is an unsigned 64-bit integer
               bitmask. For additional variable definitions refer to section 8.5.4 of the UEFI 2.9
               specification. */
            IntPtr buffer = Marshal.AllocHGlobal(8);

            Marshal.WriteInt64(buffer, (long)EFI_OS_INDICATION.BOOT_TO_FW_UI);

            bool setVariableResult = SetFirmwareEnvironmentVariableW(
                 EFI_GLOBAL_VARIABLE.OS_INDICATIONS, EFI_GLOBAL_VARIABLE.VENDOR_GUID, buffer, 8);
            int lastWin32Error = Marshal.GetLastWin32Error();

            Marshal.FreeHGlobal(buffer);

            if (!setVariableResult)
            {
                string errorMessage;

                if (lastWin32Error == (int)SYSTEM_ERROR.ERROR_INVALID_FUNCTION)
                    errorMessage = "This operation is not supported on legacy BIOS-based systems " +
                        "or UEFI-based systems with Windows installed using legacy BIOS.";
                else
                    errorMessage = "An unexpected error has occurred.";

                FatalError(errorMessage, lastWin32Error);
            }

            ExitWindowsEx(SHUTDOWN_TYPE.EWX_REBOOT, SHTDN_REASON.PLANNED);
            Environment.Exit(0);
        }

        static void AdjustPrivileges(params string[] privilegeNames)
        {
            bool openTokenResult = OpenProcessToken(Process.GetCurrentProcess().Handle,
                TOKEN_ACCESS_RIGHT.TOKEN_ADJUST_PRIVILEGES, out IntPtr tokenHandle);

            if (!openTokenResult)
                FatalError("An unexpected error has occurred while opening the access token " +
                    "associated with this process.", Marshal.GetLastWin32Error());

            /* Due to the CLR having a difficult time marshalling flexible array members without
               size constants set it is going to be more intuitive to build the TOKEN_PRIVILEGES
               buffer manually since elements are contiguously allocated. The layout of said
               structure begins with an unsigned 32-bit integer representing the privilege count
               followed by a variable-length array of LUID_AND_ATTRIBUTES structs representing
               privileges. */

            int sizeOfLUIDAndAttributes = Marshal.SizeOf(typeof(LUID_AND_ATTRIBUTES));

            IntPtr tokenBuffer = Marshal.AllocHGlobal(
                4 + (sizeOfLUIDAndAttributes * privilegeNames.Length)),
                   offset = IntPtr.Add(tokenBuffer, 4); // Factor in count value about to be written

            Marshal.WriteInt32(tokenBuffer, privilegeNames.Length);

            for (int i = 0; i < privilegeNames.Length; i++)
            {
                bool lookupResult = LookupPrivilegeValueW(null, privilegeNames[i], out LUID luid);

                if (!lookupResult)
                    FatalError($"Unable to lookup privilege name '{privilegeNames[i]}'. " +
                        "It may not exist.", Marshal.GetLastWin32Error());

                var luidAndAttributes = new LUID_AND_ATTRIBUTES();
                luidAndAttributes.LUID = luid;
                luidAndAttributes.Attributes = PRIVILEGE_ATTRIBUTE.SE_PRIVILEGE_ENABLED;

                Marshal.StructureToPtr(luidAndAttributes, offset, false);
                offset = IntPtr.Add(offset, sizeOfLUIDAndAttributes);
            }

            /* Restoring the access token to its original state is unnecessary since additional
               processes are not being created and a system reboot is intended. */
            bool adjustTokenResult = AdjustTokenPrivileges(
                tokenHandle, false, tokenBuffer, 0, IntPtr.Zero, IntPtr.Zero);

            int lastWin32Error = Marshal.GetLastWin32Error();

            Marshal.FreeHGlobal(tokenBuffer);

            if (!adjustTokenResult || lastWin32Error == (int)SYSTEM_ERROR.ERROR_NOT_ALL_ASSIGNED)
                FatalError("The operation could not be completed due to insufficient privileges.",
                    Marshal.GetLastWin32Error());
        }

        static void FatalError(string errorMessage, int errorCode)
        {
            Console.WriteLine("{0}\n\nError code: 0x{1}\n", errorMessage, errorCode.ToString("x8"));
            Console.ReadKey(false);
            Environment.Exit(errorCode);
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool OpenProcessToken(IntPtr processHandle, TOKEN_ACCESS_RIGHT desiredAccess,
            out IntPtr tokenHandle);

        [DllImport("advapi32.dll", ExactSpelling = true,
            CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool LookupPrivilegeValueW(string systemName, string name, out LUID luid);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool AdjustTokenPrivileges(IntPtr tokenHandle, bool disableAllPrivileges,
            IntPtr newState, uint bufferLength, IntPtr previousState, IntPtr returnLength);

        [DllImport("kernel32.dll", ExactSpelling = true,
            CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool SetFirmwareEnvironmentVariableW(string name, string guid, IntPtr buffer,
            uint size);

        [DllImport("user32.dll")]
        static extern bool ExitWindowsEx(SHUTDOWN_TYPE shutdownType, SHTDN_REASON shutdownReason);
    }
}