using System;

using System.Runtime.InteropServices;

public class CryptoTest
{
    public static uint CRYPT_NEWKEYSET = 0x8;
    public static uint CRYPT_DELETEKEYSET = 0x10;
    public static uint CRYPT_MACHINE_KEYSET = 0x20;
    public static uint CRYPT_SILENT = 0x40;
    public static uint CRYPT_DEFAULT_CONTAINER_OPTIONAL = 0x80;
    public static uint CRYPT_VERIFYCONTEXT = 0xF0000000;
    public static uint PROV_RSA_FULL = 1;

    public static uint PRIVATEKEYBLOB = 7;
    public static uint PUBLICKEYBLOB = 6;

    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CryptAcquireContext(ref IntPtr hProv, string pszContainer, string pszProvider, uint dwProvType, uint dwFlags);

    [DllImport(@"Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool CryptGenKey(
            [In] IntPtr hProv,
            [In] uint Algid,
            [In] uint dwFlags,
            [Out] out IntPtr phKey
        );

    [DllImport(@"advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CryptImportKey(IntPtr hProv, byte[] pbKeyData, UInt32 dwDataLen, IntPtr hPubKey, UInt32 dwFlags, ref IntPtr hKey);

    [DllImport(@"advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CryptEncrypt(IntPtr hKey, IntPtr hHash, int Final, uint dwFlags, byte[] pbData, ref uint pdwDataLen, uint dwBufLen);

    [DllImport(@"advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CryptExportKey(IntPtr hKey, IntPtr hExpKey, uint dwBlobType, uint dwFlags, [In, Out] byte[] pbData, ref uint dwDataLen);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CryptDecrypt(IntPtr hKey, IntPtr hHash, int Final, uint dwFlags, byte[] pbData, ref uint pdwDataLen);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CryptDuplicateKey(IntPtr hKey, IntPtr pdwReserved, uint dwFlags, ref IntPtr phKey);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool CryptGetKeyParam(
        IntPtr hKey,
        uint dwParam,
        [Out] byte[] pbData,
        [In, Out] ref uint pdwDataLen,
        uint dwFlags);

    public class CRYPT_STRING_FLAGS
    {
        public const uint CRYPT_STRING_BASE64HEADER = 0;
        public const uint CRYPT_STRING_BASE64 = 1;
        public const uint CRYPT_STRING_BINARY = 2;
        public const uint CRYPT_STRING_BASE64REQUESTHEADER = 3;
        public const uint CRYPT_STRING_HEX = 4;
        public const uint CRYPT_STRING_HEXASCII = 5;
        public const uint CRYPT_STRING_BASE64_ANY = 6;
        public const uint CRYPT_STRING_ANY = 7;
        public const uint CRYPT_STRING_HEX_ANY = 8;
        public const uint CRYPT_STRING_BASE64X509CRLHEADER = 9;
        public const uint CRYPT_STRING_HEXADDR = 10;
        public const uint CRYPT_STRING_HEXASCIIADDR = 11;
        public const uint CRYPT_STRING_HEXRAW = 12;
        public const uint CRYPT_STRING_NOCRLF = 0x40000000;
        public const uint CRYPT_STRING_NOCR = 0x80000000;
    }

    [DllImport("crypt32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CryptStringToBinary([MarshalAs(UnmanagedType.LPWStr)] string pszString, uint cchString, uint dwFlags, byte[] pbBinary, ref uint pcbBinary, out uint pdwSkip, out uint pdwFlags);

    [DllImport("crypt32.dll", SetLastError = true)]
    public static extern bool CryptEncodeObject(
            uint dwCertEncodingType,
            int lpszStructType,
            byte[] pvStructInfo,
            byte[] pbEncoded,
            ref uint pcbEncoded);
}
