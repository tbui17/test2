using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;

namespace Pipeline;

public partial class Build
{
    AbsolutePath TestResultsDirectory => TemporaryDirectory / "TestResults";

    Target Test => _ => _
        .Executes(() =>
            {
                var settings = new DotNetTestSettings()
                    .SetConfiguration(Configuration)
                    .SetLoggers("trx")
                    .SetDataCollector("XPlat Code Coverage")
                    .SetResultsDirectory(TestResultsDirectory)
                    .EnableNoLogo();

                DotNetTasks.DotNetTest(settings);
            }
        );
}
