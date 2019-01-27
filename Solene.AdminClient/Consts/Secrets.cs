namespace Solene.AdminClient.Consts
{
    /// <summary>
    /// This file contains the secret keys.
    /// It should be marked as 'git update-index --skip-worktree' in git, allowing it to be
    /// safely modified locally. It should never be checked in to source control after
    /// being modified. (Except to add new, unfilled keys)
    /// In the CI build, the ReplaceMe strings will be swapped out as a build step.
    /// </summary>
    public static class Secrets
    {
        // <AzureFunctionsReplaceMe>        
        public static string CreatePlayerFunctionCode => "<CreatePlayerFunctionCode>";
        public static string DeletePlayerFunctionCode => "<DeletePlayerFunctionCode>";
        public static string GetPlayerFunctionCode => "<GetPlayerFunctionCode>";
        public static string GetAllPlayersFunctionCode => "<GetAllPlayersFunctionCode>";
        public static string GetPlayerQuestionsFunctionCode => "<GetPlayerQuestionsFunctionCode>";
        public static string AddQuestionFunctionCode => "<AddQuestionFunctionCode>";
        public static string GetAllPlayersAndQuestionsFunctionCode => "<GetAllPlayersAndQuestionsFunctionCode>";
    }
}
