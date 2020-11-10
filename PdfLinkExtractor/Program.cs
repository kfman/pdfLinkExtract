using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text.pdf;

void WriteLine(string text, ConsoleColor? color = null) {
    var defaultColor = Console.ForegroundColor;
    if (color != null)
        Console.ForegroundColor = color.Value;
    Console.WriteLine(text);
    Console.ForegroundColor = defaultColor;
}

var links = new List<string>();
WriteLine("C# PDF extractor");
if (!Directory.Exists("./pdf")) {
    WriteLine("Directory ./pdf  not found", ConsoleColor.Red);
    return;
}

var fileInfo = Directory.GetFiles("./pdf");
if ((fileInfo?.Length ?? 0) == 0) {
    WriteLine("No PDFs found in ./pdf", ConsoleColor.Red);
    return;
}

foreach (var fileName in fileInfo!) {
    WriteLine($"Opening file {fileName}", ConsoleColor.Yellow);
    var reader = new PdfReader(fileName);
    foreach (var info in reader.Info) {
        WriteLine($"{info.Key.PadRight(15)}\t{info.Value}");
    }

    for (var i = 1; i < reader.NumberOfPages; i++)
        if (reader.GetLinks(i) != null) {
            WriteLine($"Reading page {i}:");
            foreach (var link in reader.GetLinks(i)) {
                WriteLine(
                    link.GetParameters()
                        .Aggregate($"{link}: \r\n\t",
                            (p, v) =>
                                $"{p} {v.Key.ToString().PadRight(15)}: ({v.Value.GetType()}) {v.Value}\r\n\t"),
                    ConsoleColor.Green);
                if (link.GetParameters().ContainsKey(PdfName.A)) {
                    var value = link.GetParameters()[PdfName.A];

                    if (value is PdfDictionary dictionary)
                        foreach (var (key, pdfObject) in dictionary) {
                            Console.WriteLine(
                                $"{(key?.ToString() ?? "").PadRight(15)} ({pdfObject.GetType()}) {pdfObject}");
                            if (Equals(key, PdfName.URI))
                                links.Add(pdfObject.ToString());
                        }
                }
            }
        }

    reader.Close();

    foreach (var link in links) {
        WriteLine(link, ConsoleColor.Blue);
    }
}
