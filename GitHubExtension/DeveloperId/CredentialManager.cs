// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;

namespace GitHubExtension.DeveloperId;

public static class CredentialManager
{
    public enum CRED_TYPE : int
    {
        GENERIC = 1,
        DOMAIN_PASSWORD = 2,
        DOMAIN_CERTIFICATE = 3,
        DOMAIN_VISIBLE_PASSWORD = 4,
        MAXIMUM = 5,
    }

    public enum CRED_PERSIST : uint
    {
        Session = 1,
        LocalMachine = 2,
        Enterprise = 3,
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CREDENTIAL
    {
        public int Flags;
        public CRED_TYPE Type;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string TargetName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public int CredentialBlobSize;
        public IntPtr CredentialBlob;
        public int Persist;
        public int AttributeCount;
        public IntPtr CredAttribute;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string TargetAlias;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string UserName;
    }

    [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CredWrite(CREDENTIAL credential, int flags);

    [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CredDelete(string target, CRED_TYPE type, int flags);

    [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CredRead(string target, CRED_TYPE type, int flags, out IntPtr credential);

    [DllImport("advapi32.dll", EntryPoint = "CredEnumerateW", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool CredEnumerate(string filter, uint flags, out uint count, out IntPtr credentials);

    [DllImport("advapi32.dll", EntryPoint = "CredFree", CharSet = CharSet.Unicode)]
    internal static extern void CredFree(IntPtr buffer);
}
