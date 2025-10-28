$ErrorActionPreference = 'Stop'
$base = "C:\Ilyas\New folder\IIS-Host\IIS-WebApi\partners\partner1"
$terms = @("terminal1","terminal2","terminal3")
foreach ($t in $terms) {
  $folder = Join-Path $base $t
  if (Test-Path $folder) {
    $tmp = Join-Path $env:TEMP ([guid]::NewGuid().ToString())
    New-Item -ItemType Directory -Force -Path $tmp | Out-Null
    $dummy = Join-Path $tmp "dummy.txt"
    "sample content" | Out-File -FilePath $dummy -Encoding ascii
    Compress-Archive -Path $dummy -DestinationPath (Join-Path $folder "report1.zip") -Force
    Compress-Archive -Path $dummy -DestinationPath (Join-Path $folder "report2.zip") -Force
    Remove-Item $tmp -Recurse -Force
  }
}
