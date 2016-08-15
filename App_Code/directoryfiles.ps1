foreach ($f in Get-ChildItem "C:\Users\gerhardl\Documents\My Received Files\") {
    $offices | select @{n=$prop.Name;e={$prop.Value}}, OFFICES
} | Export-Csv 'C:\path\to\combined.csv' -NoTypeInformation


(foreach ($f in Get-ChildItem "C:\Users\gerhardl\Documents\My Received Files\") {
    $offices | select @{n=$prop.Name;e={$prop.Value}}, OFFICES
} | Export-Csv 'C:\path\to\combined.csv' -NoTypeInformation) | Out-File 'C:\path\to\combined.csv' -NoTypeInformation

Get-ChildItem "C:\Users\gerhardl\Documents\My Received Files\" | ForEach-Object {
    $offices | select @{n=$prop.Name;e={$prop.Value}}, OFFICES
} | Out-File 'C:\combined.txt' -NoTypeInformation




foreach ($f in Get-ChildItem "C:\Users\topho\Dropbox\Apps\Azure\herbertmilhomme\") {
	Write-Output [io.path]::GetFullPath($f)
	Write-Output [io.path]::GetDirectoryName([io.path]::GetFullPath($f))
	Write-Output [io.path]::GetFileNameWithoutExtension([io.path]::GetFullPath($f))
	Write-Output [io.path]::GetExtension([io.path]::GetFullPath($f))
} | Out-File 'C:\combined.txt'

[io.path]::GetFileName($name)
Write-Output [io.path]::GetFullPath($f)
Write-Output [io.path]::GetDirectoryName([io.path]::GetFullPath($f))
Write-Output [io.path]::GetFileNameWithoutExtension([io.path]::GetFullPath($f))
Write-Output [io.path]::GetExtension([io.path]::GetFullPath($f))
LastModified


dir -r  | % { if ($_.PsIsContainer) { $_.FullName -replace "C:\\Users\\topho\\Dropbox\\Apps\\Azure\\herbertmilhomme","" + "\" } else { $_.FullName -replace "C:\\Users\\topho\\Dropbox\\Apps\\Azure\\herbertmilhomme",""} } | Export-Csv '..\combined.csv' -NoTypeInformation  | Out-File -filepath "..\combined.txt" 

dir | % { $_.FullName -replace "C:\\Users\\topho\\Dropbox\\Apps\\Azure\\herbertmilhomme","" }
dir | % { $_.FullName -replace "C:\\","" }


