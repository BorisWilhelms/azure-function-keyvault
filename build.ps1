$project = "src/KeyVault/KeyVault.csproj"
$outDir = "../../publish"

dotnet build $project -c release
dotnet pack $project -o $outDir -c release --no-build