using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace Solene.AdminClient.Consts
{
    public static class Secrets
    {
        private const string FunctionsKeysFileName = "functionKeys.txt";

        public static async Task InitializeAsync()
        {
            StorageFile functionKeys = null;
            if (StorageApplicationPermissions.MostRecentlyUsedList.ContainsItem(FunctionsKeysFileName))
            {
                functionKeys = await StorageApplicationPermissions.MostRecentlyUsedList.GetFileAsync(FunctionsKeysFileName);
            }
            else
            {
                var picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(".txt");
                functionKeys = await picker.PickSingleFileAsync();
                StorageApplicationPermissions.MostRecentlyUsedList.Add(functionKeys);
            }

            IList<string> lines = await FileIO.ReadLinesAsync(functionKeys);
            CreatePlayerFunctionCode = lines[0];
            DeletePlayerFunctionCode = lines[1];
            GetPlayerFunctionCode = lines[2];
            GetAllPlayersFunctionCode = lines[3];
            GetPlayerQuestionsFunctionCode = lines[4];
            AddQuestionFunctionCode = lines[5];
            GetAllPlayersAndQuestionsFunctionCode = lines[6];
        }
        
        public static string CreatePlayerFunctionCode { get; private set; }
        public static string DeletePlayerFunctionCode { get; private set; }
        public static string GetPlayerFunctionCode { get; private set; }
        public static string GetAllPlayersFunctionCode { get; private set; }
        public static string GetPlayerQuestionsFunctionCode { get; private set; }
        public static string AddQuestionFunctionCode { get; private set; }
        public static string GetAllPlayersAndQuestionsFunctionCode { get; private set; }
    }
}
