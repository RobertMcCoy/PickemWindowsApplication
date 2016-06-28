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
        public Inventory_ResultWrapper deserializedInventoryResults;

        public Inventory()
        {
            try
            {
                WebRequest InventoryInfoGET = WebRequest.Create("https://api.steampowered.com/ICSGOTournaments_730/GetTournamentItems/v1?key=" + Properties.Settings.Default.apiKey + Properties.Settings.Default.tournamentItems);
                InventoryInfoGET.ContentType = "application/json; charset=utf-8";
                Stream inventoryStream = InventoryInfoGET.GetResponse().GetResponseStream();
                StreamReader inventoryReader = new StreamReader(inventoryStream);

                StringBuilder sb = new StringBuilder();

                while (inventoryReader.EndOfStream != true)
                {
                    sb.Append(inventoryReader.ReadLine());
                }

                inventoryJSON = sb.ToString();

                deserializedInventoryResults = JsonConvert.DeserializeObject<Inventory_ResultWrapper>(inventoryJSON);
            }
            catch (Exception exc)
            {
                MessageBox.Show("ERROR PIRANNHA:\n\nThere was an issue retrieving inventory information: " + exc.ToString());
            }
        }

        public List<string> returnAvailableStickersTeams()
        {
            List<string> listOfStickers = new List<string>();
            for (int i = 0; i < deserializedInventoryResults.result.items.Count; i++)
            {
                if (deserializedInventoryResults.result.items[i].type.Equals("team"))
                {
                    listOfStickers.Add(deserializedInventoryResults.result.items[i].teamid.ToString());
                }
            }
            return listOfStickers;
        }

        public List<string> returnAvailableStickersPlayers()
        {
            List<string> listOfStickers = new List<string>();
            for (int i = 0; i < deserializedInventoryResults.result.items.Count; i++)
            {
                if (deserializedInventoryResults.result.items[i].type.Equals("player"))
                {
                    listOfStickers.Add(deserializedInventoryResults.result.items[i].playerid.ToString());
                }
            }
            return listOfStickers;
        }
    }

    public class Inventory_ResultWrapper
    {
        public Inventory_Result result { get; set; }
    }

    public class Inventory_Result
    {
        public List<Inventory_Items> items { get; set; }
    }

    public class Inventory_Items
    {
        public string type { get; set; }
        public string playerid { get; set; }
        public string teamid { get; set; }
        public string itemid { get; set; }
    }
}
