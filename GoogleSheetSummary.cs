using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace SpendPoint
{
    public static class GoogleSheetSummary
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "CSV To Excel";

        public static SheetsService AuthenticateUser(string credentialsDirectory)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = credentialsDirectory;
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credentialsDirectory, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            return service;
        }

        private static string GenerateOneLineRecordFromOutput(OutputSummary summary)
        {
            string response = string.Empty;

            response = summary.ClientName
            + "," + summary.JobNumber
            + "," + summary.ProjectID
            + "," + summary.SupervisorName
            + "," + summary.RepresentativeName
            + "," + summary.TotalHouseholds.Replace(",", "")
            + "," + summary.SummaryPaneFilterSettings
            + "," + summary.CustomerNumber
            + "," + summary.ResponseTime
            + "," + summary.User
            + "," + summary.MailingType;

            return response;
        }

        public static List<OutputSummary> ToSummaryViewModel(this string filePath)
        {
            List<OutputSummary> summaries = new List<OutputSummary>();
            string[] fieldData;
            using (TextFieldParser csvReader = new TextFieldParser(filePath))
            {
                csvReader.SetDelimiters(new string[] { "," });
                csvReader.HasFieldsEnclosedInQuotes = true;
                string[] colFields = csvReader.ReadFields();
                while (!csvReader.EndOfData)
                {
                    try
                    {
                        fieldData = csvReader.ReadFields();
                        summaries.Add(new OutputSummary
                        {
                            ClientName = fieldData[0],
                            JobNumber = fieldData[1],
                            ProjectID = fieldData[2],
                            SupervisorName = fieldData[3],
                            RepresentativeName = fieldData[4],
                            TotalHouseholds = fieldData[5],
                            SummaryPaneFilterSettings = fieldData[6],
                            CustomerNumber = fieldData[7],
                            ResponseTime = fieldData[8],
                            User = fieldData.Length > 9 ? fieldData[9] : "",
                            MailingType = fieldData.Length > 10 ? fieldData[10] : ""
                        });
                    }
                    catch
                    {
                        throw;
                    }
                }
            }

            return summaries;
        }

        public static void WritePreviousNulEntriesToGoogleSheets(SheetsService service, string outputDirectoryPath)
        {
            bool allRecordsSynced = false;
            try
            {
                var fileName = Path.Combine(outputDirectoryPath, ApplicationConstants.googleSheetNulName);

                if (File.Exists(fileName))
                {
                    var previousNonSynchedEntries = ToSummaryViewModel(fileName);
                    if (previousNonSynchedEntries.Any()) // header + at least one non synched record
                    {
                        Logger.WriteLog("Starting to sync previous non-synced records.", outputDirectoryPath);
                        foreach (OutputSummary summary in previousNonSynchedEntries)
                        {
                            allRecordsSynced = UpdateGoogleOutputSheet(service, summary, outputDirectoryPath, false);
                            if (!allRecordsSynced)
                            {
                                // if one record is not synched means - it cannot sync other records as well
                                break;
                            }
                        }
                        if (allRecordsSynced)
                        {
                            Logger.WriteLog("All previous non-synced records have been synced successfully. Deleting the NUL file now.", outputDirectoryPath);
                            File.Delete(fileName);
                        }
                        Logger.WriteLog("Finished sync operation of previous non-synced records.", outputDirectoryPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog("Error in method WritePreviousNulEntriesToGoogleSheets : " + ex.Message, outputDirectoryPath);
                allRecordsSynced = false;
            }
        }

        public static bool UpdateGoogleOutputSheet(SheetsService service, OutputSummary summary, string outputDirectoryPath, bool writeToLocalGoogleNULFile)
        {
            try
            {
                // Define request parameters.
                String spreadsheetId = "1bNaqld9WNGoulG-HXxrrnCmKHH0fsM1JQRGlOStl6ew"; // production file

                // Write to specified sheet
                String writeRange = "Quotes and Orders!A1:H";
                ValueRange valueRange = new ValueRange { MajorDimension = "ROWS" };
                IList<Object> dataList = new List<Object>
                {
                    summary.ClientName,
                    summary.JobNumber,
                    summary.ProjectID,
                    summary.SupervisorName,
                    summary.RepresentativeName,
                    summary.TotalHouseholds.Replace(",", ""),
                    summary.SummaryPaneFilterSettings,
                    summary.CustomerNumber,
                    summary.ResponseTime,
                    summary.User,
                    summary.MailingType
                };

                // Data is accessible through the DataReader object here.
                ValueRange valueDataRange = new ValueRange() { MajorDimension = "ROWS" };
                valueDataRange.Values = new List<IList<object>> { dataList };

                // API to append data to sheet
                SpreadsheetsResource.ValuesResource.AppendRequest appendRequest = service.Spreadsheets.Values.Append(valueDataRange, spreadsheetId, writeRange);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
                appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

                AppendValuesResponse appendValueResponse = appendRequest.Execute();
                //if (appendValueResponse.Updates.UpdatedRows != null && appendValueResponse.Updates.UpdatedRows.Value > 0)
                //{
                //}
                ApplicationConstants.GoogleSheetUploadStatus = GoogleSheetStatus.WrittenToCloud;
                return true;
            }
            catch (Exception ex)
            {
                // try to write to local NUL file
                if (writeToLocalGoogleNULFile)
                {
                    Logger.WriteLog("Error inside : UpdateSheet : Could not connect to server. Please contact administrator of the program." + ex.Message, outputDirectoryPath);
                    Logger.WriteLog(ex.ToString(), outputDirectoryPath);

                    var recordToWriteToLocalGoogleSheetNulFile = GenerateOneLineRecordFromOutput(summary);
                    GoogleSheetsNulProcessor.WriteNonSynchedEntry(recordToWriteToLocalGoogleSheetNulFile, outputDirectoryPath);
                }
                else
                {
                    Logger.WriteLog("Error while syncing old non-synced records from NUL file : UpdateSheet : Could not connect to server. Please contact administrator of the program." + ex.Message, outputDirectoryPath);
                    Logger.WriteLog("Error Details for the above exception : \n" + ex.ToString(), outputDirectoryPath);
                }

                return false;
            }
        }
    }
}
