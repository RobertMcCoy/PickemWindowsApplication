using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace PickemTest
{
    public class Fantasy
    {
        private string itemsURL;
        private string schemaJSON;
        private dynamic deserializedSchemaResults;
        private string fantasyLineupJSON = @"";
        private dynamic deserliazedProPlayers;
        private string proPlayersJSON = @"";
        private int[] pickIdsArr;
        private string[] teamsArr;
        FantasyLineup_ResultWrapper deserializedFantasyLineup;
        List<String> proPlayers = new List<String>();
        Dictionary<string, string> proPlayerLookup = new Dictionary<string, string>();

        public Fantasy()
        {
            try
            {
                WebRequest schemaInfoGET = WebRequest.Create("https://api.steampowered.com/IEconItems_730/GetSchemaURL/v2/?key=" + Properties.Settings.Default.apiKey + "&format=json");
                schemaInfoGET.ContentType = "application/json; charset=utf-8";
                Stream schemaStream = schemaInfoGET.GetResponse().GetResponseStream();
                StreamReader schemaReader = new StreamReader(schemaStream);

                StringBuilder sb = new StringBuilder();
                
                while (schemaReader.EndOfStream != true)
                {
                    sb.Append(schemaReader.ReadLine());
                }
                
                schemaJSON = sb.ToString();

                deserializedSchemaResults = JsonConvert.DeserializeObject(schemaJSON);
                foreach (var items_game_url in deserializedSchemaResults)
                {
                    itemsURL = items_game_url.Value.items_game_url; //This link is the link given to the current items_game.txt, meaning the list of players and their IDs will always be the most recent
                }
                generateTeamArrays(); //Copied method from the MainForm class, necessary for determining teams of players for Fantasy
            }
            catch (Exception exc)
            {
                MessageBox.Show("ERROR PENGUIN:\n\nThere was an issue retrieving schema information: " + exc.ToString());
            }
        }

        public List<String> getListProPlayers()
        {
            try
            {
                WebRequest proPlayersInfoGET = WebRequest.Create(itemsURL + "?format=json"); //Takes items_game.txt link from constructor and gets the VDF string
                proPlayersInfoGET.ContentType = "application/json; charset=utf-8";
                Stream proPlayersStream = proPlayersInfoGET.GetResponse().GetResponseStream();
                StreamReader proPlayersReader = new StreamReader(proPlayersStream);

                StringBuilder sb = new StringBuilder();
                sb.Append("{\n\"items_game\"\n{\n"); //Add this for easy json conversion later
                bool isProPlayerPortion = false;
                string lastLine = "";
                while (proPlayersReader.EndOfStream != true)
                {
                    string currentLine = proPlayersReader.ReadLine();
                    if (currentLine.Contains("	\"pro_players\"")) //Only take the lines for pro_players portion, otherwise this is 65k lines
                    {
                        isProPlayerPortion = true;
                    }
                    if (currentLine.Contains("items_game_live")) //This is the end of the pro_players portion, should be a couple thousand lines
                    {
                        isProPlayerPortion = false;
                        break;
                    }
                    if (isProPlayerPortion)
                    {
                        sb.Append(currentLine + Environment.NewLine); //TODO: NewLine makes for clean formatting, might want to experiment without that for performance reasons?
                    }
                    lastLine = currentLine; //This is a check, because there are two lines with "pro_players" and the one we want has } before it, and the other has {, this is a really crappy fix, might want to change this in the future
                }
                sb.Append("}\n}");
                proPlayersJSON = sb.ToString();
                proPlayersJSON = convertVDFtoJSON(proPlayersJSON); //Get the JSON string, convert from VDF
                deserliazedProPlayers = JsonConvert.DeserializeObject(proPlayersJSON);
                foreach (var desc in deserliazedProPlayers.items_game.pro_players) //Each desc will be a different player, but they have random number names
                {
                    string currentPlayer = desc.Value.name;
                    var playerId = desc.Name;
                    proPlayers.Add(currentPlayer);
                    if (playerId is string) //This again is a really crappy fix for this in my opinion, but my limited undetermined var deserialization knowledge forces me to make this check
                    {
                        proPlayerLookup.Add(playerId, currentPlayer); //Provides quick lookup for players when listing names on all tabs
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("ERROR PENGUIN:\n\nThere was an issue retrieving schema information: " + exc.ToString());
            }
            return proPlayers;
        }

        private String convertVDFtoJSON(String pro) //This will convert the Valve VDF format over to JSON via Regex (Converted from Alien Hoboken's PHP to C# on Github https://gist.github.com/AlienHoboken/5571903)
        {
            pro = Regex.Replace(pro, "\"(?<first>[^\"]*)\"(?<seconds>\\s*){", "\"${first}\": {"); //Changes all pre-bracket keywords to "": { from "" {
            pro = Regex.Replace(pro, "\"([^\"]*)\"\\s*\"([^\"]*)\"", "\"${1}\": \"${2}\","); //Changes all description/value types to "" : "" from "" ""
            pro = Regex.Replace(pro, ",(\\s*[}\\]])", "${1}"); //Changes all values in square brackets to no square brackets (Not in this VDF format, but just in case it is added later)
            pro = Regex.Replace(pro, "([}\\]])(\\s*)(\"[^\"]*\":\\s*)?([{\\[])", "${1},${2}${3}${4}"); //Removes trailing commas from unecessary locations (Again I don't think this is in our current string)
            pro = Regex.Replace(pro, "}(\\s*\"[^\"]*\":)", "},${1}"); //Add commas after each set of curly brackets
            return pro;
        }

        public void updateFantasyLists(IEnumerable<object> dropDowns) //This method is called when the checkbox for only available is unchecked, and it will show ALL players in every combo box on every page
        {
            sortPlayerList(proPlayers);
            foreach (ComboBox drop in dropDowns)
            {
                drop.BindingContext = new BindingContext(); //Necessary to break-up combo boxes, otherwise they would all change together
                drop.DataSource = proPlayers; //Set it equal to the sorted list from the constructor so the combobox fills with the proper individuals
                setSelectedItemFantasy(drop); //Get's the currently picked player in this box and autofills it with their name
            }
        }

        public bool updateFantasyListsOnlyAvailable(IEnumerable<object> dropDowns) //This method is called with the checkbox for only available is checked, and it will only show players who play that day
        {
            List<List<int>> allTeamsThatPlay = getTeamsThatPlayEachDay();
            int dayCounter = 1;
            if (allTeamsThatPlay != null) //getTeamsThatPlayEachDay will return null if there is any error during the process
            {
                foreach (List<int> currentDayTeams in allTeamsThatPlay) //For each day (or list) in the list
                {
                    if (dayCounter == 7) //Because the fantasy game is day 6...this has to be made into day 6 again after incrementation
                    {
                        dayCounter = 6;
                    }
                    List<String> availableProPlayers = new List<String>();
                    foreach (int teamNumber in currentDayTeams) //For each team number that is in each days list
                    {
                        availableProPlayers.AddRange(getProPlayersFromATeam(teamNumber)); //Combine the lists until all available players are added from each current days team
                    }
                    availableProPlayers = sortPlayerList(availableProPlayers);
                    foreach (ComboBox drop in dropDowns)
                    {
                        if (drop.Name.Contains("day" + dayCounter)) //Only update comboboxes that have "day#<position>" with current day teams
                        {
                            drop.BindingContext = new BindingContext(); //Necessary to break-up combo boxes, otherwise they would all change together
                            drop.DataSource = availableProPlayers; //Set it equal to the sorted list from the constructor so the combobox fills with the proper individuals
                            setSelectedItemFantasy(drop); //Get's the currently picked player in this box and autofills it with their name
                        }
                    }
                    availableProPlayers = null;
                    dayCounter++;
                }
                return true;
            }
            else
            {
                MessageBox.Show("ERROR WHALE:\n\nThere was an issue displaying only available players, instead showing all players.");
                return false;
            }
        }

        private List<String> getProPlayersFromATeam(int teamNumber)
        {
            List<String> listOfPlayers = new List<String>();
            foreach (var desc in deserliazedProPlayers.items_game.pro_players)
            {
                if (desc.Value.events != null) //Thank you pyth for making me have to add this :)
                {
                    foreach (JProperty info in desc.Value.events)
                    {
                        if (Int32.Parse(info.Name) == Properties.Settings.Default.fantasyTournament)
                        {
                            if (Int32.Parse(info.First["team"].ToString()) == teamNumber)
                            {
                                listOfPlayers.Add((string)desc.Value.name);
                                break;
                            }
                        }
                    }
                }
            }
            return listOfPlayers;
        }

        private List<List<int>> getTeamsThatPlayEachDay() //Parses each section of the tournament and retrieves all teams that play each day, returning them in a list of a list of ints
        {
            List<int> currentDay; //List to be used for all teams that play in that day
            List<List<int>> listOfAllDaysAndTeams = new List<List<int>>(); //List of a list of ints, so therefore the first list of ints in the list is day 1, the second list is day 2, etc.
            foreach (Layout_Section sect in MainForm.deserializedLayoutResults.result.sections) //The global tournament layout is used because the fantasy one is tied to statistics
            {
                currentDay = new List<int>();
                for (int i = 0; i < sect.groups.Count; i++)
                {
                    currentDay.Add(sect.groups[i].teams[0].pickid); //First team in the group match
                    currentDay.Add(sect.groups[i].teams[1].pickid); //Second team in the group match
                }
                listOfAllDaysAndTeams.Add(currentDay); //After each section is processed, the list of teams is added to the list of all days
            }
            return listOfAllDaysAndTeams;
        }

        private void setSelectedItemFantasy(ComboBox combo)
        {
            /*
             * Every single box has day(#)(position) and is referenced easily by this
             * The dictionary lookup identifies the current player ID and finds their name
             * The combobox is then fitted with that players name, otherwise it is left blank, as value is an empty string for each time this method is called 
            */ 
            string value = "";
            getFantasyPredictionsJSON();
            if (deserializedFantasyLineup.result.teams.Count != 0)
            {
                if (combo.Name.Contains("day1"))
                {
                    if (combo.Name.Contains("commando"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[0].picks[0].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("clutchking"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[0].picks[1].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("ecowarrior"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[0].picks[2].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("entryfragger"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[0].picks[3].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("sniper"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[0].picks[4].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                }
                if (combo.Name.Contains("day2"))
                {
                    if (combo.Name.Contains("commando"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[1].picks[0].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("clutchking"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[1].picks[1].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("ecowarrior"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[1].picks[2].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("entryfragger"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[1].picks[3].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("sniper"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[1].picks[4].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                }
                if (combo.Name.Contains("day3"))
                {
                    if (combo.Name.Contains("commando"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[2].picks[0].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("clutchking"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[2].picks[1].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("ecowarrior"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[2].picks[2].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("entryfragger"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[2].picks[3].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("sniper"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[2].picks[4].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                }
                if (combo.Name.Contains("day4"))
                {
                    if (combo.Name.Contains("commando"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[3].picks[0].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("clutchking"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[3].picks[1].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("ecowarrior"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[3].picks[2].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("entryfragger"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[3].picks[3].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("sniper"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[3].picks[4].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                }
                if (combo.Name.Contains("day5"))
                {
                    if (combo.Name.Contains("commando"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[4].picks[0].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("clutchking"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[4].picks[1].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("ecowarrior"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[4].picks[2].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("entryfragger"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[4].picks[3].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("sniper"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[4].picks[4].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                }
                if (combo.Name.Contains("day6"))
                {
                    if (combo.Name.Contains("commando"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[5].picks[0].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("clutchking"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[5].picks[1].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("ecowarrior"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[5].picks[2].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("entryfragger"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[5].picks[3].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                    if (combo.Name.Contains("sniper"))
                    {
                        proPlayerLookup.TryGetValue(deserializedFantasyLineup.result.teams[5].picks[4].ToString(), out value);
                        combo.SelectedItem = value;
                    }
                }
            }
        }

        private void getFantasyPredictionsJSON() //Straightforward fantasy lineup deserialization much like the tournament layout one
        {
            try
            {
                WebRequest fantasyLineupInfoGET = WebRequest.Create("https://api.steampowered.com/ICSGOTournaments_730/GetTournamentFantasyLineup/v1?key=" + Properties.Settings.Default.apiKey + Properties.Settings.Default.tournamentPickemPredictions);
                fantasyLineupInfoGET.ContentType = "application/json; charset=utf-8";
                Stream fantasyLineupStream = fantasyLineupInfoGET.GetResponse().GetResponseStream();
                StreamReader fantasyLineupReader = new StreamReader(fantasyLineupStream);

                StringBuilder sb = new StringBuilder();

                while (fantasyLineupReader.EndOfStream != true)
                {
                    sb.Append(fantasyLineupReader.ReadLine());
                }

                fantasyLineupJSON = sb.ToString();

                deserializedFantasyLineup = JsonConvert.DeserializeObject<FantasyLineup_ResultWrapper>(fantasyLineupJSON);
            }
            catch (Exception exc)
            {
                MessageBox.Show("ERROR AARDVARK:\n\nThere was an issue retrieving fantasy lineup information: " + exc.ToString());
            }
        }

        public string updatePlayerInfo(ComboBox combo)
        {
            /*
             * This method is called for every single combobox on each tab of the fantasy pickem from day 1-6
             * They are in a list called "dropDownsToUpdate" in the MainForm class
             * This gets the current setting for the statistics range to show and then returns the text with that information to the method call 
            */ 
            string labelText = "";
            if (combo.SelectedItem == null || (combo.SelectedItem.ToString().Contains("--- ") && combo.SelectedItem.ToString().Contains(" ---")))
            {
                return "Invalid Selection"; //This was a team that was selected...
            }
            foreach (var desc in deserliazedProPlayers.items_game.pro_players)
            {
                if (desc.Value.name == combo.SelectedItem) //Gets if the current player name matches the combobox name
                {
                    if (Properties.Settings.Default.fantasyStats.Equals("All Majors")) //Pull information from all availables Majors for this player
                    {
                        int totalKills = 0, totalDeaths = 0, totalClutchKills = 0, totalPistolKills = 0, totalOpeningKills = 0, totalSniperKills = 0, totalMatchesPlayed = 0;
                        string teamName = "";
                        foreach (JProperty info in desc.Value.events) //Loops through each event (E.g. "7", "8", "9") available for each player
                        {
                            /*
                             * The following name portion works currently, but may be an issue in the future
                             * It relies on the currently selected fantasy tournament to figure out the team name
                             * This would only be an issue if a player played in a previous major, but not the most recent one, and was on a team that no longer exists
                             * It is a rare case, but there are some occurences in the pro_players portion
                            */ 
                            if (Int32.Parse(info.Name) == Properties.Settings.Default.fantasyTournament)
                            {
                                teamName = getTeamNameFromPickId((int)info.Value["team"]);
                            }

                            if (info.Count != 0)
                            {
                                if (info.Value["enemy_kills"] != null)
                                {
                                    totalKills += (int)info.Value["enemy_kills"];
                                }
                                if (info.Value["deaths"] != null)
                                {
                                    totalDeaths += (int)info.Value["deaths"];
                                }
                                if (info.Value["clutch_kills"] != null)
                                {
                                    totalClutchKills += (int)info.Value["clutch_kills"];
                                }
                                if (info.Value["pistol_kills"] != null)
                                {
                                    totalPistolKills += (int)info.Value["pistol_kills"];
                                }
                                if (info.Value["opening_kills"] != null)
                                {
                                    totalOpeningKills += (int)info.Value["opening_kills"];
                                }
                                if (info.Value["sniper_kills"] != null)
                                {
                                    totalSniperKills += (int)info.Value["sniper_kills"];
                                }
                                if (info.Value["matches_played"] != null)
                                {
                                    totalMatchesPlayed += (int)info.Value["matches_played"];
                                }
                            }
                        }
                        labelText += "Team: " + teamName + "\nKills: " + totalKills + "\nDeaths: " + totalDeaths + "\nKDR: " + ((float) totalKills / (float) totalDeaths) + "\nClutch Kills: " + totalClutchKills + "\nPistol Kills: " + totalPistolKills + "\nEntry Frags: " + totalOpeningKills + "\nSniper Kills: " + totalSniperKills + "\nMatches Played: " + totalMatchesPlayed;
                        return labelText;
                    }
                    else if (Properties.Settings.Default.fantasyStats.Equals("Entire Previous Major"))
                    {
                        foreach (var info in desc.Value.events)
                        {
                            if (Int32.Parse(info.Name) == (Properties.Settings.Default.fantasyTournament - 1)) //The previous major (AKA the one before this one) should just be one less than the current major given the schema from Valve
                            {
                                labelText += "Kills: " + info.Value.enemy_kills + "\nDeaths: " + info.Value.deaths + "\nKDR: " + info.Value.KDR + "\nClutch Kills: " + info.Value.clutch_kills + "\nPistol Kills: " + info.Value.pistol_kills + "\nEntry Frags: " + info.Value.opening_kills + "\nSniper Kills: " + info.Value.sniper_kills + "\nMatches Played: " + info.Value.matches_played;
                                return labelText;
                            }
                        }
                    }
                    else if (Properties.Settings.Default.fantasyStats.Equals("Prior Day Only"))
                    {
                        foreach (JProperty info in desc.Value.events)
                        {
                            if (Int32.Parse(info.Name) == Properties.Settings.Default.fantasyTournament) //TODO: Get this working. My lack of knowledge with handling "unknown" JSON Objects prevents me from knowing how to access the last index of the event node (Which would be the last stage)
                            {
                                labelText += "Team: " + getTeamNameFromPickId((int)info.Value["team"]);
                                foreach (JObject stage in info) //TODO: Might have to make this a switch statement and determine if stage5 exists, else if stage4 exists, etc.
                                {
                                    return labelText += "\nKills: " + stage.Last.First["enemy_kills"] + "\nDeaths: " + stage.Last.First["deaths"] + "\nKDR: " + stage.Last.First["KDR"] + "\nClutch Kills: " + stage.Last.First["clutch_kills"] + "\nPistol Kills: " + stage.Last.First["pistol_kills"] + "\nEntry Frags: " + stage.Last.First["opening_kills"] + "\nSniper Kills: " + stage.Last.First["sniper_kills"] + "\nMatches Played: " + stage.Last.First["matches_played"];
                                }
                            }
                        }
                    }
                    else if (Properties.Settings.Default.fantasyStats.Equals("Entire Current Major So Far"))
                    {
                        foreach (var info in desc.Value.events)
                        {
                            if (Int32.Parse(info.Name) == Properties.Settings.Default.fantasyTournament) //Everyday the base event numbers are updated, so this just simply needs to get that
                            {
                                labelText += "Team: " + getTeamNameFromPickId((int)info.Value.team) + "\nKills: " + info.Value.enemy_kills + "\nDeaths: " + info.Value.deaths + "\nKDR: " + info.Value.KDR + "\nClutch Kills: " + info.Value.clutch_kills + "\nPistol Kills: " + info.Value.pistol_kills + "\nEntry Frags: " + info.Value.opening_kills + "\nSniper Kills: " + info.Value.sniper_kills + "\nMatches Played: " + info.Value.matches_played;
                                return labelText;
                            }
                        }
                    }
                }
            }
            return "No Information Available"; //This is the final return, if any of the if branches above fail, it will simply update the label to say this
        }

        private void generateTeamArrays() //Used for associating pickids with actual team names
        {
            int numberOfTeams = MainForm.deserializedLayoutResults.result.teams.Count;
            pickIdsArr = new int[numberOfTeams];
            teamsArr = new string[numberOfTeams];

            for (int i = 0; i < numberOfTeams; i++)
            {
                pickIdsArr[i] = MainForm.deserializedLayoutResults.result.teams[i].pickid;
                teamsArr[i] = MainForm.deserializedLayoutResults.result.teams[i].name;
            }
        }

        private string getTeamNameFromPickId(int pickId) //Method that loops through arrays to identify team names. 
        {
            for (int i = 0; i < pickIdsArr.Length; i++)
            {
                if (pickIdsArr[i] == pickId)
                {
                    return teamsArr[i];
                }
            }
            MessageBox.Show("ERROR: PANDA\n\nThere was an issue generating team names from the provided picking options, pickid = " + pickId);
            return "";
        }

        private List<String> sortPlayerList(List<String> players)
        {
            List<String> sortedList = new List<String>();
            if (Properties.Settings.Default.fantasyPlayerSortOrder.Equals("By Name"))
            {
                players.Sort();
            }
            else if (Properties.Settings.Default.fantasyPlayerSortOrder.Equals("By Team"))
            {
                List<String> tempListOfPlayersAndTeams = new List<String>();
                foreach (int currentTeam in pickIdsArr)
                {
                    if (!getTeamNameFromPickId(currentTeam).Contains("All-Star"))
                    {
                        tempListOfPlayersAndTeams.Add("--- " + getTeamNameFromPickId(currentTeam) + " ---");
                        foreach (var desc in deserliazedProPlayers.items_game.pro_players)
                        {
                            string playerName = (string)desc.Value.name;
                            if (desc.Value.events != null) //Thank you pyth for making me have to add this :)
                            {
                                foreach (JProperty info in desc.Value.events)
                                {
                                    if (Int32.Parse(info.Name) == Properties.Settings.Default.fantasyTournament && Int32.Parse(info.Value["team"].ToString()) == currentTeam)
                                    {
                                        tempListOfPlayersAndTeams.Add(playerName);
                                    }
                                }
                            }
                        }
                    }
                }
                players = tempListOfPlayersAndTeams;
            }
            return players;
        }
    }
}
