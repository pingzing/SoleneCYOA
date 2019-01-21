Param(
    [String]$uwpAppCenterKey,
    [String]$androidAppCenterKey,

    [String]$createPlayerFunctionCode,
    [String]$getPlayerFunctionCode,
    [String]$getPlayerAnswersFunctionCode, #oops, this should be getPlayerQUESTIONSFunctionCode. oh well.
    [String]$answerQuestionFunctionCode, 
    [String]$registerPushNotificationFunctionCode
)

#Main
$secretsContent = Get-Content "./Solene.MobileApp/Solene.MobileApp.Core/Consts/Secrets.cs";
$secretsContent = $secretsContent.Replace("<UwpReplaceMe>", $uwpAppCenterKey);
$secretsContent = $secretsContent.Replace("<AndroidReplaceMe>", $androidAppCenterKey);
$secretsContent = $secretsContent.Replace("<CreatePlayerFunctionCodeFunctionsReplaceMe>", $createPlayerFunctionCode);
$secretsContent = $secretsContent.Replace("<GetPlayerFunctionCodeReplaceMe>", $getPlayerFunctionCode);
$secretsContent = $secretsContent.Replace("<GetPlayerQuestionsFunctionCodeReplaceMe>", $getPlayerAnswersFunctionCode);
$secretsContent = $secretsContent.Replace("<AnswerQuestionFunctionCodeReplaceMe>", $answerQuestionFunctionCode);
$secretsContent = $secretsContent.Replace("<RegisterPushNotificationsCodeReplaceMe>", $registerPushNotificationFunctionCode);
Set-Content "./Solene.MobileApp/Solene.MobileApp.Core/Consts/Secrets.cs" $secretsContent;
Write-Host "Contents of Secrets.cs filled in with keys.";