param ($from, $to)

$fromFileXml = [xml](Get-Content -Path $from -Raw)
$toFileXml = [xml](Get-Content -Path $to -Raw)

if ( $toFileXml.configuration.runtime.InnerXml -Ne $fromFileXml.configuration.runtime.InnerXml ) {
    $toFileXml.configuration.runtime.InnerXml = $fromFileXml.configuration.runtime.InnerXml
    $toFileXml.Save($to)
}