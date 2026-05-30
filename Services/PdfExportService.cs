using System.Text;
using TravelPlannerApp.Models;

namespace TravelPlannerApp.Services;

public class PdfExportService
{
    public byte[] CreatePackingPdf(PackingList list)
    {
        var lines = new List<string>
        {
            $"Lista pakowania: {list.Name}",
            $"Wygenerowano: {DateTime.Now:dd.MM.yyyy HH:mm}",
            "",
            "Status    Przedmiot    Ilość"
        };

        if (list.Items.Count == 0)
        {
            lines.Add("Brak przedmiotów na liście.");
        }
        else
        {
            lines.AddRange(list.Items.Select(i => $"{(i.IsPacked ? "[x]" : "[ ]")} {i.Name} - {Math.Max(i.Quantity, 1)} szt."));
        }

        using var stream = new MemoryStream();
        var offsets = new List<long>();

        void WriteAscii(string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        void BeginObject(int id)
        {
            offsets.Add(stream.Position);
            WriteAscii($"{id} 0 obj\n");
        }

        void EndObject()
        {
            WriteAscii("\nendobj\n");
        }

        void WriteObject(int id, string body)
        {
            BeginObject(id);
            WriteAscii(body);
            EndObject();
        }

        var content = CreatePageContent(lines);

        WriteAscii("%PDF-1.4\n");
        WriteObject(1, "<< /Type /Catalog /Pages 2 0 R >>");
        WriteObject(2, "<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
        WriteObject(3, "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>");
        WriteObject(4, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding 6 0 R >>");

        BeginObject(5);
        WriteAscii($"<< /Length {content.Length} >>\nstream\n");
        stream.Write(content, 0, content.Length);
        WriteAscii("\nendstream");
        EndObject();

        WriteObject(6, "<< /Type /Encoding /BaseEncoding /WinAnsiEncoding /Differences [128 /Aogonek /Cacute /Eogonek /Lslash /Nacute /Sacute /Zacute /Zdotaccent /aogonek /cacute /eogonek /lslash /nacute /sacute /zacute /zdotaccent] >>");

        var xref = stream.Position;
        WriteAscii("xref\n");
        WriteAscii("0 7\n");
        WriteAscii("0000000000 65535 f \n");
        foreach (var position in offsets)
        {
            WriteAscii($"{position:0000000000} 00000 n \n");
        }

        WriteAscii("trailer << /Size 7 /Root 1 0 R >>\n");
        WriteAscii("startxref\n");
        WriteAscii($"{xref}\n");
        WriteAscii("%%EOF");
        return stream.ToArray();
    }

    private static byte[] CreatePageContent(IEnumerable<string> lines)
    {
        using var content = new MemoryStream();

        void WriteAscii(string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            content.Write(bytes, 0, bytes.Length);
        }

        WriteAscii("BT\n");
        WriteAscii("/F1 18 Tf 50 790 Td\n");

        var first = true;
        foreach (var line in lines)
        {
            if (!first)
            {
                WriteAscii("0 -22 Td\n");
            }

            WriteAscii("(");
            var encoded = EncodePdfText(line);
            content.Write(encoded, 0, encoded.Length);
            WriteAscii(") Tj\n");

            if (first)
            {
                WriteAscii("/F1 12 Tf\n");
            }

            first = false;
        }

        WriteAscii("ET");
        return content.ToArray();
    }

    private static byte[] EncodePdfText(string text)
    {
        using var result = new MemoryStream();

        foreach (var ch in text)
        {
            switch (ch)
            {
                case '(':
                    result.WriteByte((byte)'\\');
                    result.WriteByte((byte)'(');
                    break;
                case ')':
                    result.WriteByte((byte)'\\');
                    result.WriteByte((byte)')');
                    break;
                case '\\':
                    result.WriteByte((byte)'\\');
                    result.WriteByte((byte)'\\');
                    break;
                case 'Ą': result.WriteByte(128); break;
                case 'Ć': result.WriteByte(129); break;
                case 'Ę': result.WriteByte(130); break;
                case 'Ł': result.WriteByte(131); break;
                case 'Ń': result.WriteByte(132); break;
                case 'Ś': result.WriteByte(133); break;
                case 'Ź': result.WriteByte(134); break;
                case 'Ż': result.WriteByte(135); break;
                case 'ą': result.WriteByte(136); break;
                case 'ć': result.WriteByte(137); break;
                case 'ę': result.WriteByte(138); break;
                case 'ł': result.WriteByte(139); break;
                case 'ń': result.WriteByte(140); break;
                case 'ś': result.WriteByte(141); break;
                case 'ź': result.WriteByte(142); break;
                case 'ż': result.WriteByte(143); break;
                case 'Ó': result.WriteByte(211); break;
                case 'ó': result.WriteByte(243); break;
                default:
                    result.WriteByte(ch <= 255 ? (byte)ch : (byte)'?');
                    break;
            }
        }

        return result.ToArray();
    }
}
