using ContentHub.MockImplementation.Utils;
using ExcelDataReader;
using Stylelabs.M.Base.Querying;
using Stylelabs.M.Sdk.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Linq;
using ContentHub.MockImplementation.Scripts;

namespace ContentHub.MockImplementation
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.WriteLine("started.");

            (new RejectionNotification()).SendEmail().Wait();

            //var assets = GetAssetsFromFile();
            //foreach (var asset in assets)
            //{
            //    Asset.SetupAsset(asset).Wait();
            //}


            //var link = MConnector.Client.Entities.GetAsync(10878).Result;

            //var user = MConnector.Client.Users.GetUserAsync("TestUser").Result;

            //var relation = user.GetRelationAsync<IChildToManyParentsRelation>("UserGroupToUser").Result;
            //if (!relation.Parents.Contains(5))
            //{
            //    relation.Parents.Add(5);
            //}
            //MConnector.Client.Entities.SaveAsync(user).Wait();


            Console.WriteLine("completed.");
            Console.ReadKey();
        }


        public static List<Asset> GetAssetsFromFile()
        {
            var filePath = "C:\\Users\\vasfomic\\Desktop\\mock-implementation\\ContentHub.MockImplementation\\import\\mock_data.xlsx";

            var assets = new List<Asset>();
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    var sheet = result.Tables["M.Asset"];
                    var rows = sheet.Rows;
                    var rowCount = 0;
                    foreach (DataRow row in rows)
                    {
                        if (rowCount == 0)
                        {
                            rowCount++;
                            continue;
                        }
                        assets.Add(new Asset
                        {
                            OriginUrl = row.ItemArray[0] as string,
                            Description = row.ItemArray[1] as string,
                            MarketingDescription = row.ItemArray[2] as string,
                            AssetType = row.ItemArray[3] as string,
                            SocialMediaChannel = row.ItemArray[4] as string,
                            ContentSecurity = row.ItemArray[5] as string,
                            AssetSource = row.ItemArray[6] as string,
                            LifecycleStatus = row.ItemArray[7] as string
                        });
                        rowCount++;
                    }
                }
            }
            return assets;
        }
    }
}
