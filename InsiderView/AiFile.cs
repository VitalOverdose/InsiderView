using System.IO.Compression;
using System.Text;

namespace InsiderView
{
    /// <summary>
    /// Reads the names of the top-level layers straight out of an Adobe Illustrator .ai file,
    /// without Illustrator.
    ///
    /// How an .ai file is put together: it is a PDF on the outside. Illustrator's own copy of the
    /// drawing sits inside it as numbered blocks (AIPrivateData1, AIPrivateData2, ...). Joined
    /// together, those blocks are a short plain header followed by the drawing itself, packed
    /// with Zstandard (Illustrator 2020 and later) or zlib (older). Unpacked, it is text, and
    /// every layer in it looks like:
    ///
    ///     %AI5_BeginLayer
    ///     ... Lb
    ///     (Layer name) Ln
    ///     ...
    ///     %AI5_EndLayer--
    ///
    /// A layer inside another layer sits inside the outer one's Begin/End pair, so a name seen
    /// while exactly one layer is open belongs to a top-level layer.
    /// </summary>
    internal static class AiFile
    {
        private static readonly byte[] BlockKey = "/AIPrivateData"u8.ToArray();
        private static readonly byte[] ZstandardMark = "%AI24_ZStandard_Data"u8.ToArray();
        private static readonly byte[] ZlibMark = "%AI12_CompressedData"u8.ToArray();

        public static IReadOnlyList<string> TopLayerNames(string path)
        {
            byte[] file = File.ReadAllBytes(path);
            byte[] drawing = Unpack(PrivateData(file));
            return LayerNames(drawing);
        }

        // The AIPrivateData blocks joined in order. A file saved without the PDF wrapper has
        // no blocks: the whole file is already Illustrator's own data.
        private static byte[] PrivateData(byte[] file)
        {
            var blocks = new SortedDictionary<int, int>();   // block number -> PDF object number
            for (int at = IndexOf(file, BlockKey, 0); at >= 0; at = IndexOf(file, BlockKey, at + BlockKey.Length))
            {
                int p = at + BlockKey.Length;
                if (!ReadNumber(file, ref p, out int block)) continue;
                SkipSpaces(file, ref p);
                if (!ReadNumber(file, ref p, out int obj)) continue;
                if (StartsWith(file, p, " 0 R"u8)) blocks[block] = obj;
            }

            if (blocks.Count == 0) return file;

            using var joined = new MemoryStream();
            foreach (int obj in blocks.Values)
            {
                ReadOnlySpan<byte> block = StreamOf(file, obj);
                joined.Write(block);
            }

            return joined.ToArray();
        }

        // The contents of one PDF stream object: "65 0 obj<</Length 1234>>stream ... endstream".
        private static ReadOnlySpan<byte> StreamOf(byte[] file, int obj)
        {
            byte[] key = Encoding.ASCII.GetBytes(obj + " 0 obj");
            for (int at = IndexOf(file, key, 0); at >= 0; at = IndexOf(file, key, at + key.Length))
            {
                // "165 0 obj" must not be taken for "65 0 obj".
                if (at > 0 && IsDigit(file[at - 1])) continue;

                int streamWord = IndexOf(file, "stream"u8, at);
                if (streamWord < 0 || streamWord - at > 400) continue;

                ReadOnlySpan<byte> header = file.AsSpan(at, streamWord - at);
                int lengthWord = header.IndexOf("/Length"u8);
                if (lengthWord < 0) continue;

                int p = at + lengthWord + "/Length".Length;
                SkipSpaces(file, ref p);
                if (!ReadNumber(file, ref p, out int length)) continue;

                int start = streamWord + "stream".Length;
                if (start < file.Length && file[start] == '\r') start++;
                if (start < file.Length && file[start] == '\n') start++;

                // "/Length 70 0 R" points at another object that holds the length. Rather than
                // chase it, take everything up to the word that closes the stream.
                if (StartsWith(file, p, " 0 R"u8))
                {
                    int end = IndexOf(file, "endstream"u8, start);
                    if (end < 0) continue;
                    length = end - start;
                }

                if (length < 0 || start + length > file.Length) continue;
                return file.AsSpan(start, length);
            }

            throw new InvalidDataException("Part of the drawing is missing from this file.");
        }

        private static byte[] Unpack(byte[] data)
        {
            int at = IndexOf(data, ZstandardMark, 0);
            if (at >= 0)
            {
                int start = at + ZstandardMark.Length;
                using var packed = new MemoryStream(data, start, data.Length - start, writable: false);
                using var unpacker = new ZstdSharp.DecompressionStream(packed);
                using var plain = new MemoryStream();
                unpacker.CopyTo(plain);
                return plain.ToArray();
            }

            at = IndexOf(data, ZlibMark, 0);
            if (at >= 0)
            {
                int start = at + ZlibMark.Length;
                using var packed = new MemoryStream(data, start, data.Length - start, writable: false);
                using var unpacker = new ZLibStream(packed, CompressionMode.Decompress);
                using var plain = new MemoryStream();
                unpacker.CopyTo(plain);
                return plain.ToArray();
            }

            // Very old files keep the drawing as plain text.
            return data;
        }

        private static List<string> LayerNames(byte[] drawing)
        {
            var names = new List<string>();
            int open = 0;        // how many layers we are inside
            int lineStart = 0;

            for (int i = 0; i <= drawing.Length; i++)
            {
                if (i < drawing.Length && drawing[i] != '\r' && drawing[i] != '\n') continue;

                ReadOnlySpan<byte> line = drawing.AsSpan(lineStart, i - lineStart).Trim((byte)' ');
                lineStart = i + 1;

                if (line.StartsWith("%AI5_BeginLayer"u8)) open++;
                else if (line.StartsWith("%AI5_EndLayer"u8)) open = Math.Max(0, open - 1);
                else if (open == 1 && line.Length > 5 && line[0] == '(' && line.EndsWith(") Ln"u8))
                    names.Add(PostScriptText(line.Slice(1, line.Length - 5)));
            }

            return names;
        }

        // Text between the brackets of a PostScript string: a backslash protects the next
        // character, or starts a three-digit character code.
        private static string PostScriptText(ReadOnlySpan<byte> raw)
        {
            var bytes = new List<byte>(raw.Length);
            for (int i = 0; i < raw.Length; i++)
            {
                byte b = raw[i];
                if (b != '\\' || i + 1 >= raw.Length) { bytes.Add(b); continue; }

                byte next = raw[++i];
                if (next is >= (byte)'0' and <= (byte)'7')
                {
                    int code = next - '0';
                    for (int digits = 1; digits < 3 && i + 1 < raw.Length && raw[i + 1] is >= (byte)'0' and <= (byte)'7'; digits++)
                        code = (code * 8) + (raw[++i] - '0');
                    bytes.Add((byte)code);
                }
                else
                {
                    bytes.Add(next switch
                    {
                        (byte)'n' => (byte)'\n',
                        (byte)'r' => (byte)'\r',
                        (byte)'t' => (byte)'\t',
                        _ => next
                    });
                }
            }

            byte[] text = bytes.ToArray();
            try
            {
                return new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(text);
            }
            catch (ArgumentException)
            {
                return Encoding.Latin1.GetString(text);
            }
        }

        // ---- small helpers for looking through the file's bytes ----

        private static int IndexOf(byte[] data, ReadOnlySpan<byte> what, int from)
        {
            if (from >= data.Length) return -1;
            int found = data.AsSpan(from).IndexOf(what);
            return found < 0 ? -1 : from + found;
        }

        private static bool StartsWith(byte[] data, int at, ReadOnlySpan<byte> what)
            => at >= 0 && at + what.Length <= data.Length && data.AsSpan(at, what.Length).SequenceEqual(what);

        private static bool IsDigit(byte b) => b is >= (byte)'0' and <= (byte)'9';

        private static void SkipSpaces(byte[] data, ref int at)
        {
            while (at < data.Length && data[at] == ' ') at++;
        }

        private static bool ReadNumber(byte[] data, ref int at, out int number)
        {
            number = 0;
            int start = at;
            while (at < data.Length && IsDigit(data[at]) && at - start < 9)
                number = (number * 10) + (data[at++] - '0');
            return at > start;
        }
    }
}
