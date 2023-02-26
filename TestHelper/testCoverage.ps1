if (Test-Path -Path "raw") {
    rm -r -fo "raw"
}
cd "..\FluffRestTest"
dotnet test --collect:"XPlat Code Coverage" --results-directory "..\TestHelper\raw"
cd "..\TestHelper"
if (Test-Path -Path "report") {
    rm -r -fo "report"
}
$directoryName = Get-ChildItem -Path raw -Recurse | Select-Object -First 1
reportgenerator -reports:"raw\$directoryName\coverage.cobertura.xml" -targetdir:"report" -reporttypes:Html
	
Invoke-Item "report\index.html"
