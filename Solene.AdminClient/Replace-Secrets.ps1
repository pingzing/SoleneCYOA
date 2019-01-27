Param(
    [String]$createPlayerFunctionCode,
    [String]$deletePlayerFunctionCode,
    [String]$getPlayerFunctionCode,
    [String]$getAllPlayersFunctionCode,    
    [String]$getPlayerQuestionsFunctionCode,
    [String]$addQuestionFunctionCode,    
    [String]$getAllPlayersAndQuestionsFunctionCode,    
)

#Main
$secretsContent = Get-Content "./Solene.MobileApp/Solene.MobileApp.Core/Consts/Secrets.cs";

$secretsContent = $secretsContent.Replace("<CreatePlayerFunctionCode>", $createPlayerFunctionCode);
$secretsContent = $secretsContent.Replace("<DeletePlayerFunctionCode>", $deletePlayerFunctionCode);
$secretsContent = $secretsContent.Replace("<GetPlayerFunctionCode>", $getPlayerFunctionCode);
$secretsContent = $secretsContent.Replace("<GetAllPlayersFunctionCode>", $getAllPlayersFunctionCode);
$secretsContent = $secretsContent.Replace("<GetPlayerQuestionsFunctionCode>", $getPlayerQuestionsFunctionCode);
$secretsContent = $secretsContent.Replace("<AddQuestionFunctionCode>", $addQuestionFunctionCode);
$secretsContent = $secretsContent.Replace("<GetAllPlayersAndQuestionsFunctionCode>")

Set-Content "./Solene.MobileApp/Solene.MobileApp.Core/Consts/Secrets.cs" $secretsContent;
Write-Host "Contents of Secrets.cs filled in with keys.";