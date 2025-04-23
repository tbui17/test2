using System.Collections.Generic;
using System.Linq;
using Lokql.Engine;
using NotNullStrings;

namespace Pipeline.Wiki;

public class VerbRenderer
{
    public string RenderMarkdownHelp(IEnumerable<IVerbEntry> verbs)
    {
        var rows = verbs
            .Select(x => new VerbRow(x))
            .Select(x => x.Render())
            .Prepend(VerbRow.HeaderRow())
            .JoinAsLines();


        var table = $"""
                     <table>{rows}</table>
                     """;

        return table;
    }
}

file class VerbRow(IVerbEntry verbEntry)
{

    public static string HeaderRow()
    {
        const string header = """
                              <tr>
                                  <th style="font-weight: bold">Name / Alias</th>
                                  <th style="font-weight: bold">Details</th>
                              </tr>
                              """;
        return header;
    }

    public string Render() =>
        $"""
         <tr> 
             <td style="border: none; border-top: solid;">
                 {NameCell()}
             </td>
             <td>
                 {HelpSection()}
             </td>
         </tr>
         """;

    private string NameCell()
    {
        var aliases = verbEntry.Aliases
            .Select(x => x.Trim())
            .JoinString(", ");
        return $"""
                <div>
                    <div style="font-weight: bold">{verbEntry.Name}</div>
                    <div style="font-weight: bold">{aliases}</div>
                </div>
                """;
    }

    private string HelpSection()
    {

        var help = verbEntry.HelpText
            .Trim();

        return $"""
                <section>
                    <code>{help}</code> 
                </section>
                """;
    }
}