using System.Runtime.InteropServices;

namespace InsiderView
{
    /// <summary>
    /// The small icon Windows itself shows for a file in Explorer.
    /// </summary>
    internal static class ShellIcons
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)] public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string szTypeName;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_SMALLICON = 0x1;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x10;
        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;

        // File types whose icon is different for every file, so it has to be read from the
        // file itself. Every other type has one icon for the whole type.
        private static readonly HashSet<string> OwnIcon = new(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".lnk", ".ico", ".cur", ".ani", ".scr", ".url", ".appref-ms"
        };

        /// <summary>
        /// The name to file this path's icon under: the extension, or the whole path for the
        /// types that carry their own icon.
        /// </summary>
        public static string KeyFor(string path)
        {
            string extension = Path.GetExtension(path);
            return OwnIcon.Contains(extension) ? path : extension.ToLowerInvariant();
        }

        /// <summary>The small Explorer icon for a file, or null if Windows gave none.</summary>
        public static Bitmap? Small(string path)
        {
            bool fromTheFile = OwnIcon.Contains(Path.GetExtension(path));
            uint flags = SHGFI_ICON | SHGFI_SMALLICON | (fromTheFile ? 0 : SHGFI_USEFILEATTRIBUTES);

            var info = new SHFILEINFO();
            IntPtr result = SHGetFileInfo(path, FILE_ATTRIBUTE_NORMAL, ref info, (uint)Marshal.SizeOf<SHFILEINFO>(), flags);
            if (result == IntPtr.Zero || info.hIcon == IntPtr.Zero) return null;

            try
            {
                using Icon icon = Icon.FromHandle(info.hIcon);
                return icon.ToBitmap();
            }
            finally
            {
                DestroyIcon(info.hIcon);
            }
        }
    }
}
