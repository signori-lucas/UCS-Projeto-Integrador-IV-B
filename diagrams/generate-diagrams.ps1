# Generates PNG diagrams from .puml files using PlantUML
# Requires either: 
#  - PlantUML jar + Java installed
#  - or Docker (plantuml/plantuml)

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
Set-Location $scriptDir
$puFiles = Get-ChildItem -Path . -Filter *.puml -Recurse
if ($puFiles.Count -eq 0) {
	Write-Host "No .puml files found in $scriptDir" -ForegroundColor Yellow
	exit 1
}

# Try to use docker if available
function Use-Docker {
	docker --version > $null 2>&1
	return $LASTEXITCODE -eq 0
}

if (Use-Docker) {
	Write-Host "Using Docker to render diagrams..."
	foreach ($f in $puFiles) {
		$in = $f.FullName
		$outDir = Split-Path -Parent $in
		docker run --rm -v "${PWD}:/workspace" plantuml/plantuml:latest -tpng "/workspace/$($f.FullName.Replace('\','/'))"
	}
	Write-Host "Done. PNG files next to each .puml." -ForegroundColor Green
	exit 0
}

# Fallback to plantuml.jar if present in tools\plantuml.jar
$jar = Join-Path $scriptDir "tools\plantuml.jar"
if (Test-Path $jar) {
	Write-Host "Using plantuml.jar to render diagrams..."
	foreach ($f in $puFiles) {
		& java -jar $jar -tpng $f.FullName
	}
	Write-Host "Done. PNG files next to each .puml." -ForegroundColor Green
	exit 0
}

Write-Host "No Docker or plantuml.jar found. Install Docker or place plantuml.jar in $scriptDir\tools\plantuml.jar" -ForegroundColor Red
exit 2
