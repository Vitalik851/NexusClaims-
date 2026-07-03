param([string]$DocxPath, [string]$OutPath)

$tempDir = "$env:TEMP\docx_extract_$(Get-Random)"
$zipPath = "$env:TEMP\temp_docx_$(Get-Random).zip"

Copy-Item $DocxPath $zipPath
Expand-Archive $zipPath -DestinationPath $tempDir -Force

[xml]$xml = Get-Content "$tempDir\word\document.xml" -Encoding UTF8
$nsmgr = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
$nsmgr.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main")

$paragraphs = $xml.SelectNodes("//w:p", $nsmgr)
$lines = @()
foreach ($p in $paragraphs) {
    $texts = $p.SelectNodes(".//w:t", $nsmgr)
    $line = ""
    foreach ($t in $texts) {
        $line += $t.InnerText
    }
    $lines += $line
}

$result = $lines -join "`n"
$result | Out-File -FilePath $OutPath -Encoding UTF8
Write-Host "Done. Output written to $OutPath"

# Cleanup
Remove-Item $zipPath -Force -ErrorAction SilentlyContinue
Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
