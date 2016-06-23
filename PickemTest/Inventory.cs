using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace PickemTest
{
    class Inventory
    {

        public string inventoryJSON = @"";
        dynamic deserializedInventoryResults;

        public Inventory()
        {
            try
            {
                WebRequest InventoryInfoGET = WebRequest.Create("http://steamcommunity.com/profiles/" + Properties.Settings.Default.steamID64 + "/inventory/json/730/2");
                InventoryInfoGET.ContentType = "application/json; charset=utf-8";
                Stream inventoryStream = InventoryInfoGET.GetResponse().GetResponseStream();
                StreamReader inventoryReader = new StreamReader(inventoryStream);

                StringBuilder sb = new StringBuilder();

                while (inventoryReader.EndOfStream != true)
                {
                    sb.Append(inventoryReader.ReadLine());
                }

                inventoryJSON = sb.ToString();

                deserializedInventoryResults = JsonConvert.DeserializeObject(inventoryJSON);
            }
            catch (Exception exc)
            {
                MessageBox.Show("ERROR PIRANNHA:\n\nThere was an issue retrieving inventory information: " + exc.ToString());
            }
        }

        public List<String> returnAvailableStickers()
        {
            List<String> listOfTeamStickers = new List<String>();
            foreach (var name in deserializedInventoryResults.rgDescriptions)
            {
                string currentItem = name.Value.name;
                if (currentItem.Contains("Sticker |"))
                {
                    listOfTeamStickers.Add(currentItem);
                }
            }
            return listOfTeamStickers;
        }
    }
}
