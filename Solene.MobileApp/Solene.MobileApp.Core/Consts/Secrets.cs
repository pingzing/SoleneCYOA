namespace Solene.MobileApp.Core.Consts
{

    /// <summary>
    /// This file contains the secret keys.
    /// It should be marked as --skip-worktree in git, allowing it to be
    /// safely modified locally. It should never be checked in to source control after
    /// being modified.
    /// In the CI build, the ReplaceMe strings will be swapped out as a build step.
    /// </summary>
    public static class Secrets
    {

        // <UwpReplaceMe>
        public static string UwpAppCenterKey => "<UwpReplaceMe>";

        // <AndroidReplaceMe>
        public static string AndroidAppCenterKey => "<AndroidReplaceMe>";

        // <AzureFunctionsReplaceMe>
        public static string CreatePlayerFunctionCode => "<CreatePlayerFunctionCodeFunctionsReplaceMe>";
        public static string GetPlayerFunctionCode => "<GetPlayerFunctionCodeReplaceMe>";
        public static string GetPlayerQuestionsFunctionCode => "<GetPlayerQuestionsFunctionCodeReplaceMe>";
        public static string AnswerQuestionFunctionCode => "<AnswerQuestionFunctionCodeReplaceMe>";
        public static string RegisterPushNotificationsCode => "<RegisterPushNotificationsCodeReplaceMe>";
        public static string SimulateDeveloperResponseFunctionCode => "<SimulateDeveloperResponseFunctionCodeReplaceMe>";
    }
}