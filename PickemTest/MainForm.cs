using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;

namespace PickemTest
{
    public partial class MainForm : Form
    {
        String tournamentLayoutJSON = @"";
        String tournamentPredictionsJSON = @"";
        public static Layout_ResultWrapper deserializedLayoutResults;
        Prediction_ResultWrapper deserializedPredictionResults;
        Inventory_ResultWrapper availableItems;
        List<String> availableTeamStickers = new List<String>();
        List<String> availablePlayerStickers = new List<String>();
        List<Label> proPickemStatLabels = new List<Label>();
        Inventory getInventory;
        Fantasy fantasyPlayers;
        private int[] pickIdsArr;
        private string[] teamsArr;
        List<String> proPlayerList = new List<String>();
        IEnumerable<object> dropDownsToUpdate;
        IEnumerable<object> matchBoxesToUpdate;

        public MainForm()
        {
            InitializeComponent();
            dropDownsToUpdate = new List<object>() { day1commando, day1clutchking, day1ecowarrior, day1entryfragger, day1sniper, day2commando, day2clutchking, day2ecowarrior, day2entryfragger, day2sniper, day3commando, day3clutchking, day3ecowarrior, day3entryfragger, day3sniper, day4commando, day4clutchking, day4ecowarrior, day4entryfragger, day4sniper, day5commando, day5clutchking, day5ecowarrior, day5entryfragger, day5sniper, day6commando, day6clutchking, day6ecowarrior, day6entryfragger, day6sniper };
            matchBoxesToUpdate = new List<object>() { day1match1box1, day1match1box2, day1match2box1, day1match2box2, day1match3box1, day1match3box2, day1match4box1, day1match4box2, day1match5box1, day1match5box2, day1match6box1, day1match6box2, day1match7box1, day1match7box2, day1match8box1, day1match8box2, day2match1box1, day2match1box2, day2match2box1, day2match2box2, day2match3box1, day2match3box2, day2match4box1, day2match4box2, day2match5box1, day2match5box2, day2match6box1, day2match6box2, day2match7box1, day2match7box2, day2match8box1, day2match8box2, day3match1box1, day3match1box2, day3match2box1, day3match2box2, day3match3box1, day3match3box2, day3match4box1, day3match4box2, day4match1box1, day4match1box2, day4match2box1, day4match2box2, day4match3box1, day4match3box2, day4match4box1, day4match4box2, day5match1box1, day5match1box2, day5match2box1, day5match2box2, day6match1box1, day6match1box2 };
            updateAppearance();
            updateFantasyAppearance();
        }

        public void updateAppearance() //Updates all tabs, radio boxes, combo boxes, etc. under the Team Prediction Tab
        {
            try
            {
                WebRequest tournamentInfoGET = WebRequest.Create(Properties.Settings.Default.tournamentLayout + Properties.Settings.Default.tournamentID);
                tournamentInfoGET.ContentType = "application/json; charset=utf-8";
                Stream tournamentStream = tournamentInfoGET.GetResponse().GetResponseStream();
                StreamReader tournamentReader = new StreamReader(tournamentStream);

                StringBuilder sb = new StringBuilder();

                while (tournamentReader.EndOfStream != true)
                {
                    sb.Append(tournamentReader.ReadLine());
                }
                tournamentReader.Close();
                tournamentStream.Close();
                tournamentLayoutJSON = sb.ToString();

                deserializedLayoutResults = JsonConvert.DeserializeObject<Layout_ResultWrapper>(tournamentLayoutJSON);
            }
            catch (Exception exc)
            {
                MessageBox.Show("ERROR GIRAFFE:\n\nThere was an issue retrieving tournament information: " + exc.ToString());
            }

            getInventory = new Inventory();
            fantasyPlayers = new Fantasy();

            eventName.Text = deserializedLayoutResults.result.name; //Set the event name at the top middle to the current selected event in settings.
            tabbedControls.TabPages[0].Text = deserializedLayoutResults.result.sections[0].name; //Gets the section name (E.g. Group Stage | Day 1 | 29th) and sets tab name to that
            tabbedControls.TabPages[1].Text = deserializedLayoutResults.result.sections[1].name;
            tabbedControls.TabPages[2].Text = deserializedLayoutResults.result.sections[2].name;
            tabbedControls.TabPages[3].Text = deserializedLayoutResults.result.sections[3].name;
            tabbedControls.TabPages[4].Text = deserializedLayoutResults.result.sections[4].name;
            if (deserializedLayoutResults.result.sections[5].name.Contains("All-star")) //Thanks Columbus for making me add this :)
            {
                tabbedControls.TabPages[5].Text = deserializedLayoutResults.result.sections[6].name;
            }
            else
            {
                tabbedControls.TabPages[5].Text = deserializedLayoutResults.result.sections[5].name;
            }

            generateTeamArrays(); //Generates 2 arrays that correspond to team names and pick ids

            foreach (RadioButton radio in matchBoxesToUpdate) //Clear results 
            {
                radio.BackColor = Color.White;
                radio.Text = "";
                radio.CheckedChanged -= new System.EventHandler(isSelectionPossiblePickemPrediction); //Without this short removal the popup for not having a sticker would pop up every time, as the match names are technically not regenerated yet..
                radio.Checked = false;
                radio.CheckedChanged += new System.EventHandler(isSelectionPossiblePickemPrediction);
                radio.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Regular);
            }

            generateBoxAndSelectionNames();
            generateWinnerLosers();

            if (Properties.Settings.Default.tournamentPredictions != String.Empty && Properties.Settings.Default.tournamentPredictions != "")
            {
                generatePredictionDeserializedObject(); //Get deserialized result of team predictions
                generateUserPicks();
            }
            updateCurrentScore(); //Updates score in the top right of the form
        }

        public void updateFantasyAppearance() //Updates each of the fantasy tabs player info and labels
        {
            if (Properties.Settings.Default.steamID64 != string.Empty && Properties.Settings.Default.tournamentName != string.Empty)
            {
                availablePlayerStickers = getInventory.returnAvailableStickersPlayers(); //Gets a list of all user player stickers
                availableTeamStickers = getInventory.returnAvailableStickersTeams(); //Gets a list of all user team stickers
                proPlayerList = fantasyPlayers.getListProPlayers(); //Gets a list of all pro players from the Fantasy class
                if (Properties.Settings.Default.displayOnlyAvailable) //If the user has indicated that they want only players that are available on the day of matches, this will only show players in teams that are playing
                {
                    fantasyPlayers.updateFantasyListsOnlyAvailable(dropDownsToUpdate);
                }
                else //Shows all players, in case if a player wants that. 
                {
                    fantasyPlayers.updateFantasyLists(dropDownsToUpdate);
                }

                if (Properties.Settings.Default.displayOnlyPlayersWithStickersOwned)
                {
                    fantasyPlayers.updateWithStickersOnly(dropDownsToUpdate, availablePlayerStickers);
                }

                int counter = 1; //Counter for amount of combo boxes on tab pages
                int currentTabPage = 0; //Current tab page to place labels
                if (proPickemStatLabels.Count == 0) //If the list is empty (or if this is the first run of the program in a new instance)
                {
                    foreach (ComboBox combo in dropDownsToUpdate) //Every single Fantasy combo box is in this collection
                    {
                        Label newLabel = new Label();
                        newLabel.Location = new Point(combo.Location.X, combo.Location.Y + 20); //Makes the label 20 pixels below the combo box
                        if (combo.SelectedItem != null)
                        {
                            newLabel.Text = fantasyPlayers.updatePlayerInfo(combo); //Gets the player information for the combo box, and then gets the label text in return
                        }
                        newLabel.AutoSize = true; //Makes it so that I don't have to mess with sizing for things :)
                        if (counter % 6 == 0) //Will increment to the next tab page once 5 labels have been created for the first 5 combo box..there might be a better way to do this
                        {
                            currentTabPage++;
                            counter = 1;
                        }
                        tabControl1.TabPages[currentTabPage].Controls.Add(newLabel); //Add the label to the tabPage form
                        counter++;
                        newLabel.Tag = combo; //Set the tag for updating later on
                        combo.Tag = newLabel; //Will be used in the individual player change combo box portion
                        proPickemStatLabels.Add(newLabel); //Add the label to the collection (next time this method is run, the else statement will always be the one being run
                    }
                }
                else //This branch runs if this method is called after the initial instance runs (E.g. "Update Stats" on the settings page or "Save" on settings)
                {
                    foreach (Label label in proPickemStatLabels) //Here we already have all the labels for fantasy stats, so we only need to loop through these
                    {
                        ComboBox labelCombo = (ComboBox)label.Tag; //Remember how we set the combo as that tag? This makes this 100x easier to identify the player, and saves a lot of unecessary steps
                        if (labelCombo != null)
                        {
                            label.Text = fantasyPlayers.updatePlayerInfo(labelCombo); //Updates the label that is associated with the combobox in it's tag
                        }
                    }
                }

                /*
                 * I now see why most applications only update things when "Save" and "Ok" are clicked. This needs to be changed to a system like that
                 * in the future. This stuff eats up performance, so the event handlers are removed before being called, or else this method calls itself 4 times, for no reason. 
                */ 

                fantasyTournamentToUse.SelectedIndexChanged -= new EventHandler(fantasyTournamentToUse_SelectedIndexChanged);
                fantasyTournamentToUse.Text = Properties.Settings.Default.fantasyTournament.ToString(); //Updates the settings page for fantasy pick'em
                fantasyTournamentToUse.SelectedIndexChanged += new EventHandler(fantasyTournamentToUse_SelectedIndexChanged);

                selectedEventLbl.Text = retrieveEventName(Properties.Settings.Default.tournamentLayout + Properties.Settings.Default.fantasyTournament);

                fantasyStatisticsRange.SelectedIndexChanged -= new EventHandler(fantasyStatisticsRange_SelectedIndexChanged);
                fantasyStatisticsRange.Text = Properties.Settings.Default.fantasyStats;
                fantasyStatisticsRange.SelectedIndexChanged += new EventHandler(fantasyStatisticsRange_SelectedIndexChanged);

                displayOnlyAvailable.CheckedChanged -= new EventHandler(displayOnlyAvailable_CheckedChanged);
                displayOnlyAvailable.Checked = Properties.Settings.Default.displayOnlyAvailable;
                displayOnlyAvailable.CheckedChanged += new EventHandler(displayOnlyAvailable_CheckedChanged);

                playerSortingOrderCombo.SelectedIndexChanged -= new EventHandler(playerSortingOrderCombo_SelectedIndexChanged);
                playerSortingOrderCombo.Text = Properties.Settings.Default.fantasyPlayerSortOrder;
                playerSortingOrderCombo.SelectedIndexChanged += new EventHandler(playerSortingOrderCombo_SelectedIndexChanged);

                displayOnlyPlayersWithStickersOwner.CheckedChanged -= new EventHandler(displayOnlyPlayersWithStickersOwner_CheckedChanged);
                displayOnlyPlayersWithStickersOwner.Checked = Properties.Settings.Default.displayOnlyPlayersWithStickersOwned;
                displayOnlyPlayersWithStickersOwner.CheckedChanged += new EventHandler(displayOnlyPlayersWithStickersOwner_CheckedChanged);
            }
        }

        public void updateCurrentScore()
        {
            int totalScore = 0; //Initial score is zero
            bool isPickMade; //Every day a single pick is made, the user automatically gets 1 point, so this will increment each day a pick has been made

            for (int p = 0; p < deserializedLayoutResults.result.sections.Count; p++) //Go through every section given in JSON
            {
                isPickMade = false;
                for (int i = 0; i < deserializedLayoutResults.result.sections[p].groups.Count; i++) //Go through every single match in JSON
                {
                    for (int j = 0; j < deserializedPredictionResults.result.picks.Count; j++) //Go through every user prediction in JSON
                    {
                        if (deserializedLayoutResults.result.sections[p].groups[i].groupid == deserializedPredictionResults.result.picks[j].groupid) //If the winning layout group id matches the user prediction group id give points
                        {
                            isPickMade = true;
                            if (deserializedLayoutResults.result.sections[p].groups[i].picks[0].pickids.Count > 0) //If the match has not happened yet, there will be nothing in pickids and a null exception is thrown. This makes sure the match has happened and the results have been published.
                            {
                                if (deserializedLayoutResults.result.sections[p].groups[i].picks[0].pickids[0] == deserializedPredictionResults.result.picks[j].pick) //If the winning team was the pick we made, give the points for the match
                                {
                                    totalScore += deserializedLayoutResults.result.sections[p].groups[i].points_per_pick;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (isPickMade) //Increment score by 1 for just making a pick, as talked about above
                {
                    totalScore += 1; //Increment total score because a prediction was made for that day (section)
                }
            }

            currentScore.Text = "Current Score: " + totalScore + "/100"; //Label is located in the top right
        }

        private void generateTeamArrays() //Each index of both arrays indicated the pickid for the team, and the other array indicates the team name (E.g. pickIdsArr[0] = 1, teamsArr[0] = Ninjas in Pyjamas)
        {
            int numberOfTeams = deserializedLayoutResults.result.teams.Count;
            pickIdsArr = new int[numberOfTeams];
            teamsArr = new string[numberOfTeams];

            for (int i = 0; i < numberOfTeams; i++)
            {
                pickIdsArr[i] = deserializedLayoutResults.result.teams[i].pickid;
                teamsArr[i] = deserializedLayoutResults.result.teams[i].name;
            }
        }

        private void generateBoxAndSelectionNames()
        {
            List<GroupBox> groupBoxes = new List<GroupBox>();
            if (deserializedLayoutResults.result.sections[0].groups[0].teams[0].pickid != 0)
            {
                groupBoxes.AddRange(addItemsToList(day1matchBox1, day1matchBox2, day1matchBox3, day1matchBox4, day1matchBox5, day1matchBox6, day1matchBox7, day1matchBox8));
            }
            else
            {
                tabbedControls.TabPages[1].Enabled = false;
            }
            if (deserializedLayoutResults.result.sections[1].groups[0].teams[0].pickid != 0)
            {
                groupBoxes.AddRange(addItemsToList(day2matchBox1, day2matchBox2, day2matchBox3, day2matchBox4, day2matchBox5, day2matchBox6, day2matchBox7, day2matchBox8));
            }
            else
            {
                tabbedControls.TabPages[1].Enabled = false;
            }
            if (deserializedLayoutResults.result.sections[2].groups[0].teams[0].pickid != 0)
            {
                groupBoxes.AddRange(addItemsToList(day3matchBox1, day3matchBox2, day3matchBox3, day3matchBox4));
            }
            else
            {
                tabbedControls.TabPages[2].Enabled = false;
            }
            if (deserializedLayoutResults.result.sections[3].groups[0].teams[0].pickid != 0)
            {
                groupBoxes.AddRange(addItemsToList(day4matchBox1, day4matchBox2, day4matchBox3, day4matchBox4));
            }
            else
            {
                tabbedControls.TabPages[3].Enabled = false;
            }
            if (deserializedLayoutResults.result.sections[4].groups[0].teams[0].pickid != 0)
            {
                groupBoxes.AddRange(addItemsToList(day5matchBox1, day5matchBox2));
            }
            else
            {
                tabbedControls.TabPages[4].Enabled = false;
            }
            if (deserializedLayoutResults.result.sections[5].name.Contains("All Star"))
            {
                if (deserializedLayoutResults.result.sections[6].groups[0].teams[0].pickid != 0)
                {
                    groupBoxes.AddRange(addItemsToList(day6matchBox1));
                }
                else
                {
                    tabbedControls.TabPages[5].Enabled = false;
                }
            }
            else
            {
                if (deserializedLayoutResults.result.sections[5].groups[0].teams[0].pickid != 0)
                {
                    groupBoxes.AddRange(addItemsToList(day6matchBox1));
                }
                else
                {
                    tabbedControls.TabPages[5].Enabled = false;
                }
            }

            foreach (GroupBox groupBox in groupBoxes)
            {
                groupBox.Enabled = true; //Do this to make sure it is enabled if tournament IDs are swapped. They will be disabled again if the pick isn't allowed later on...

                int sectionNumber = Int32.Parse(groupBox.Name.Substring(3, 1)) - 1;
                int groupNumber = Int32.Parse(groupBox.Name.Substring(12, 1)) - 1;

                if (groupBox.Name.Contains("day6") && deserializedLayoutResults.result.sections[5].name.Contains("All Star"))
                {
                    sectionNumber += 1;
                }

                groupBox.Text = deserializedLayoutResults.result.sections[sectionNumber].groups[groupNumber].name + " - Points for Match: " + deserializedLayoutResults.result.sections[sectionNumber].groups[groupNumber].points_per_pick;
                groupBox.Tag = deserializedLayoutResults.result.sections[sectionNumber].groups[groupNumber];
                foreach (RadioButton radioInGroupBox in groupBox.Controls)
                {
                    if (radioInGroupBox.Name.Contains("box1"))
                    {
                        if (deserializedLayoutResults.result.sections[sectionNumber].groups[groupNumber].teams[0].pickid != 0)
                        {
                            radioInGroupBox.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[sectionNumber].groups[groupNumber].teams[0].pickid);
                            radioInGroupBox.Tag = deserializedLayoutResults.result.sections[sectionNumber].groups[groupNumber];
                        }
                    }
                    if (radioInGroupBox.Name.Contains("box2"))
                    {
                        if (deserializedLayoutResults.result.sections[sectionNumber].groups[groupNumber].teams[1].pickid != 0)
                        {
                            radioInGroupBox.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[sectionNumber].groups[groupNumber].teams[1].pickid);
                            radioInGroupBox.Tag = deserializedLayoutResults.result.sections[sectionNumber].groups[groupNumber];
                        }
                    }

                    if (!deserializedLayoutResults.result.sections[sectionNumber].groups[groupNumber].picks_allowed)
                    {
                        groupBox.Enabled = false;
                    }
                }
            }
        }

        private string getTeamNameFromPickId(int pickId) //This will loop through the arrays for teams and their pickids, and is called when a pickid is retrieved from deserialization and the actual team name needs to be displayed to the user for friendliness
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

        private void generatePredictionDeserializedObject() //Gets the users individual predictions after they have entered the right settings
        {
            try
            {
                WebRequest predictionInfoGET = WebRequest.Create("https://api.steampowered.com/ICSGOTournaments_730/GetTournamentPredictions/v1?key=" + Properties.Settings.Default.apiKey + Properties.Settings.Default.tournamentPredictions);
                predictionInfoGET.ContentType = "application/json; charset=utf-8";
                Stream tournamentStream = predictionInfoGET.GetResponse().GetResponseStream();
                StreamReader tournamentReader = new StreamReader(tournamentStream);

                StringBuilder sb = new StringBuilder();

                while (tournamentReader.EndOfStream != true)
                {
                    sb.Append(tournamentReader.ReadLine());
                }
                predictionInfoGET = null;
                tournamentPredictionsJSON = sb.ToString();
                tournamentStream.Close();
                tournamentReader.Close();
                deserializedPredictionResults = JsonConvert.DeserializeObject<Prediction_ResultWrapper>(tournamentPredictionsJSON);
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR ELEPHANT:\n\nThere was an issue retrieving prediction information: " + e.ToString());
            }
        }

        private void generateUserPicks()
        {
            List<GroupBox> matchBoxes = new List<GroupBox>() { day1matchBox1, day1matchBox2, day1matchBox3, day1matchBox4, day1matchBox5, day1matchBox6, day1matchBox7, day1matchBox8, day2matchBox1, day2matchBox2, day2matchBox3, day2matchBox4, day2matchBox5, day2matchBox6, day2matchBox7, day2matchBox8, day3matchBox1, day3matchBox2, day3matchBox3, day3matchBox4, day4matchBox1, day4matchBox2, day4matchBox3, day4matchBox4, day5matchBox1, day5matchBox2, day6matchBox1 };
            foreach (GroupBox matchBox in matchBoxes)
            {
                foreach (RadioButton button in matchBox.Controls)
                {
                    int teamNumber = Int32.Parse(button.Name.Substring(13, 1)) - 1;
                    for (int i = 0; i < deserializedPredictionResults.result.picks.Count; i++)
                    {
                        if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(matchBox, 'g') && deserializedPredictionResults.result.picks[i].pick == getTagInfo(matchBox, 'p', teamNumber))
                        {
                            button.CheckedChanged -= new EventHandler(isSelectionPossiblePickemPrediction);
                            button.Checked = true;
                            button.CheckedChanged += new EventHandler(isSelectionPossiblePickemPrediction);
                        }
                    }
                }
            }
        }

        private List<T> addItemsToList<T>(params T[] listItems)
        {
            List<T> tempList = new List<T>();
            foreach (T item in listItems)
            {
                tempList.Add(item);
            }
            return tempList;
        }

        private void generateWinnerLosers()
        {
            List<RadioButton> matchBoxes = new List<RadioButton>();
            if (deserializedLayoutResults.result.sections[0].groups[0].picks[0].pickids.Count != 0)
            {
                matchBoxes.AddRange(addItemsToList(day1match1box1, day1match1box2, day1match2box1, day1match2box2, day1match3box1, day1match3box2, day1match4box1, day1match4box2, day1match5box1, day1match5box2, day1match6box1, day1match6box2, day1match7box1, day1match7box2, day1match8box1, day1match8box2));
            }
            if (deserializedLayoutResults.result.sections[1].groups[0].picks[0].pickids.Count != 0)
            {
                matchBoxes.AddRange(addItemsToList(day2match1box1, day2match1box2, day2match2box1, day2match2box2, day2match3box1, day2match3box2, day2match4box1, day2match4box2, day2match5box1, day2match5box2, day2match6box1, day2match6box2, day2match7box1, day2match7box2, day2match8box1, day2match8box2));
            }
            if (deserializedLayoutResults.result.sections[2].groups[0].picks[0].pickids.Count != 0)
            {
                matchBoxes.AddRange(addItemsToList(day3match1box1, day3match1box2, day3match2box1, day3match2box2, day3match3box1, day3match3box2, day3match4box1, day3match4box2));
            }
            if (deserializedLayoutResults.result.sections[3].groups[0].picks[0].pickids.Count != 0)
            {
                matchBoxes.AddRange(addItemsToList(day4match1box1, day4match1box2, day4match2box1, day4match2box2, day4match3box1, day4match3box2, day4match4box1, day4match4box2));
            }
            if (deserializedLayoutResults.result.sections[4].groups[0].picks[0].pickids.Count != 0)
            {
                matchBoxes.AddRange(addItemsToList(day5match1box1, day5match1box2, day5match2box1, day5match2box2));
            }
            if (deserializedLayoutResults.result.sections[5].name.Contains("All Star"))
            {
                if (deserializedLayoutResults.result.sections[6].groups[0].picks[0].pickids.Count != 0)
                {
                    matchBoxes.AddRange(addItemsToList(day6match1box1, day6match1box2));
                }
            }
            else
            {
                if (deserializedLayoutResults.result.sections[5].groups[0].picks[0].pickids.Count != 0)
                {
                    matchBoxes.AddRange(addItemsToList(day6match1box1, day6match1box2));
                }
            }

            foreach (RadioButton button in matchBoxes)
            {
                int groupNumber = Int32.Parse(button.Name.Substring(9, 1)) - 1;
                int sectionNumber = Int32.Parse(button.Name.Substring(3, 1)) - 1;
                int teamNumber = Int32.Parse(button.Name.Substring(13, 1)) - 1;

                if (button.Name.Contains("day6") && deserializedLayoutResults.result.sections[5].name.Contains("All Star"))
                {
                    sectionNumber += 1;
                }

                if (deserializedLayoutResults.result.sections[sectionNumber].groups[groupNumber].picks[0].pickids.Count != 0)
                {
                    if (getTagInfo(button, 'p', teamNumber) == deserializedLayoutResults.result.sections[sectionNumber].groups[groupNumber].picks[0].pickids[0])
                    {
                        button.BackColor = Color.Green;
                        button.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                    }
                    else
                    {
                        button.BackColor = Color.Red;
                    }
                }
            }
        }

        private int getTagInfo(Control obj, char infoType, int team = 0) //This method is designed to get the tag info. Because all tags I store are control objects, this was easier to make into it's own method.
                                                                         //Each branch has a specific purpose, and can easily be added onto later on.
        {
            switch (infoType)
            {
                case 'g': //Groupid
                    if (obj is GroupBox)
                    {
                        Layout_Group objectTag = (Layout_Group)obj.Tag;
                        if (objectTag != null)
                        {
                            return objectTag.groupid;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    else
                    {
                        MessageBox.Show("ERROR RHINO:\n\nError comparing object to it's control type for groupID comparison for object " + obj.Name);
                        return -1;
                    }
                case 'p': //Pick
                    if (obj is GroupBox)
                    {
                        Layout_Group objectTag = (Layout_Group)obj.Tag;
                        return objectTag.teams[team].pickid;
                    }
                    else if (obj is RadioButton)
                    {
                        Layout_Group objectTag = (Layout_Group)obj.Tag;
                        return objectTag.teams[team].pickid;
                    }
                    else
                    {
                        MessageBox.Show("ERROR ZEBRA:\n\nError comparing object to it's control type for pick id comparison for object " + obj.Name);
                        return -1;
                    }
                default:
                    MessageBox.Show("ERROR FOX:\n\nDefault case somehow reached in switch statement for control tag info");
                    return -1;
            }

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit(); //Another exit. I personally hate making this because there's an X for a reason, but every application has it, so why not? -.-
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e) //Opens Settings form, passes this form to the constructor
        {
            Settings settings = new Settings(this);
            settings.Show();
            settings.BringToFront();
        }

        private void fantasyTournamentToUse_SelectedIndexChanged(object sender, EventArgs e)
        {
            string tournamentLayout = Properties.Settings.Default.tournamentLayout + fantasyTournamentToUse.Text;
            selectedEventLbl.Text = retrieveEventName(tournamentLayout); //Updates the event name that the user sees below the selection box
            Properties.Settings.Default.fantasyTournament = Int32.Parse(fantasyTournamentToUse.Text); //Sets the setting to the user specified option
            if (selectedEventLbl.Text.Equals("Error"))
            {
                fantasyTournamentToUse.SelectedItem = Properties.Settings.Default.fantasyTournament;
            }
            Properties.Settings.Default.Save();
        }

        private string retrieveEventName(string tournamentLayout) //Simply returns the event name for the settings option
        {
            Layout_ResultWrapper deserializedLayoutResult;
            try
            {
                WebRequest tournamentInfoGET = WebRequest.Create(tournamentLayout);
                tournamentInfoGET.ContentType = "application/json; charset=utf-8";
                Stream tournamentStream = tournamentInfoGET.GetResponse().GetResponseStream();
                StreamReader tournamentReader = new StreamReader(tournamentStream);

                StringBuilder sb = new StringBuilder();

                while (tournamentReader.EndOfStream != true)
                {
                    sb.Append(tournamentReader.ReadLine());
                }
                tournamentStream.Close();
                tournamentStream.Close();
                deserializedLayoutResult = JsonConvert.DeserializeObject<Layout_ResultWrapper>(sb.ToString());
                return deserializedLayoutResult.result.name;
            }
            catch (Exception exc)
            {
                MessageBox.Show("ERROR ARMADILLO:\n\nThere was an issue retrieving tournament information in Main: " + exc.ToString());
                return "Error";
            }
        }

        private void button1_Click(object sender, EventArgs e) //Awesome control object name, I know. I don't think this button even exists anymore, but I honestly have no clue...I think this was my save button before I did on-edit settings saving...
        {
            Properties.Settings.Default.Save();
        }

        private void fantasyStatisticsRange_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.fantasyStats = fantasyStatisticsRange.Text;
            Properties.Settings.Default.Save();
        }

        private void displayOnlyAvailable_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.displayOnlyAvailable = displayOnlyAvailable.Checked;
            Properties.Settings.Default.Save();
        }

        private void button1_Click_1(object sender, EventArgs e) //Some pretty fluff text for the update option, the update process takes ~5 seconds, so I have to indicate to the user that there is at least something going on. I could make this async, but I have no experience with that. Something for the future...
        {
            updateSettingsLabel.Font = new Font("Times New Roman", 14.0f, FontStyle.Bold, GraphicsUnit.Pixel);
            updateSettingsLabel.ForeColor = Color.Red;
            updateSettingsLabel.Text = "Updating...";
            updateFantasyAppearance();
            updateSettingsLabel.ForeColor = Color.Green;
            updateSettingsLabel.Text = "Update Complete";
        }

        private void fantasySelectedIndexChange(object sender, EventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            Label associatedLabel = (Label)combo.Tag;
            if (associatedLabel != null && combo != null)
            {
                associatedLabel.Text = fantasyPlayers.updatePlayerInfo(combo);
                int dayNumber = Int32.Parse(combo.Name.Substring(3, 1)); //This should really be TryParse, but I'm confident in my code that the passed object will never have any issue with this
                if (dayNumber != 0)
                {
                    int startIndex = (dayNumber * 5 - 5); //In the dropDownsToUpdate each set of associated fantasy player dropdown's index starts at intervals of 5. Day 1 = 0, Day 2 = 5, Day 3 = 10, etc., and therefore all we need to do is multiply the day by 5, then subtract 5 from that number to get the start index
                    for (int i = startIndex; i < (startIndex + 5); i++) //Keep it in the range of 5 comboboxes (0-4,5-9,etc) as thats how many are in each set
                    {
                        if (dropDownsToUpdate.ElementAt(i) != combo) //Makes sure we are not comparing the dropdown for this set of fantasy combos to itself
                        {
                            ComboBox comparingCombo = (ComboBox)dropDownsToUpdate.ElementAt(i);
                            if (comparingCombo != null && comparingCombo.SelectedItem != null && comparingCombo.SelectedItem.Equals(combo.SelectedItem))
                            {
                                comparingCombo.SelectedIndexChanged -= new System.EventHandler(fantasySelectedIndexChange); //Prevent this loop from running on this combobox, because since it is null, it can obviously never be the same as another
                                comparingCombo.SelectedItem = null;
                                comparingCombo.SelectedIndexChanged += new System.EventHandler(fantasySelectedIndexChange);
                                Label comparingComboLabel = (Label)comparingCombo.Tag;
                                comparingComboLabel.Text = "";
                            }
                        }
                    }
                }
            }
        }

        private void playerSortingOrderCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.fantasyPlayerSortOrder = playerSortingOrderCombo.Text;
            Properties.Settings.Default.Save();
        }

        private void teamPredictionSubmit(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Making picks will lock your stickers that you have chosen. They will be unusable and untradable until the end of the match day. Removing a pick at a later time will not undo the lock.", "Confirm Sticker Lock", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                List<String> postData = new List<String>();
                IEnumerable<GroupBox> matchBoxes;
                int sectionNumber;
                Button submitButton = (Button)sender;
                switch (submitButton.Name)
                {
                    case "day1predictionSubmit":
                        sectionNumber = 0;
                        matchBoxes = new List<GroupBox>() { day1matchBox1, day1matchBox2, day1matchBox3, day1matchBox4, day1matchBox5, day1matchBox6, day1matchBox7, day1matchBox8 };
                        break;
                    case "day2predictionSubmit":
                        sectionNumber = 1;
                        matchBoxes = new List<GroupBox>() { day2matchBox1, day2matchBox2, day2matchBox3, day2matchBox4, day2matchBox5, day2matchBox6, day2matchBox7, day2matchBox8 };
                        break;
                    case "day3predictionSubmit":
                        sectionNumber = 2;
                        matchBoxes = new List<GroupBox>() { day3matchBox1, day3matchBox2, day3matchBox3, day3matchBox4 };
                        break;
                    case "day4predictionSubmit":
                        sectionNumber = 3;
                        matchBoxes = new List<GroupBox>() { day4matchBox1, day4matchBox2, day4matchBox3, day4matchBox4 };
                        break;
                    case "day5predictionSubmit":
                        sectionNumber = 4;
                        matchBoxes = new List<GroupBox>() { day5matchBox1, day5matchBox2 };
                        break;
                    case "day6predictionSubmit":
                        if (!deserializedLayoutResults.result.sections[5].name.Contains("All Star"))
                        {
                            sectionNumber = 5;
                        }
                        else
                        {
                            sectionNumber = 6;
                        }
                        matchBoxes = new List<GroupBox>() { day6matchBox1 };
                        break;
                    default:
                        sectionNumber = -1;
                        matchBoxes = null;
                        break;
                }
                if (sectionNumber != -1 && matchBoxes != null)
                {
                    List<int> alreadyPickedTeams = new List<int>();
                    for(int i = 0; i < deserializedPredictionResults.result.picks.Count; i++)
                    {
                        for (int j = 0; j < deserializedLayoutResults.result.sections[sectionNumber].groups.Count; j++) {
                            if (deserializedLayoutResults.result.sections[sectionNumber].groups[j].groupid == deserializedPredictionResults.result.picks[i].groupid) {
                                alreadyPickedTeams.Add(deserializedPredictionResults.result.picks[i].pick);
                            }
                        }
                    }
                    for (int i = 0; i < matchBoxes.Count(); i++)
                    {
                        RadioButton checkedRadio;
                        checkedRadio = matchBoxes.ElementAt(i).Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked); //Loops through each ComboBox and identifies the control that is selected
                        if (checkedRadio != null) //Null check verifies that we are only performing operations on radio buttons that are selected or not error'd out
                        {
                            Layout_Group radioTag = (Layout_Group)checkedRadio.Tag;
                            int teamPick;
                            string itemId = string.Empty;
                            if (checkedRadio.Name.Contains("box1"))
                            {
                                teamPick = radioTag.teams[0].pickid;
                            }
                            else
                            {
                                teamPick = radioTag.teams[1].pickid;
                            }
                            bool isAlreadyPicked = false;
                            foreach (int pickedTeam in alreadyPickedTeams)
                            {
                                if (teamPick == pickedTeam)
                                {
                                    isAlreadyPicked = true;
                                }
                            }
                            if (!isAlreadyPicked)
                            {
                                for (int j = 0; j < availableItems.result.items.Count; j++)
                                {
                                    if (Int32.Parse(availableItems.result.items[j].teamid) == teamPick && availableItems.result.items[j].type.Equals("team")) //Find the associated sticker item id to the team id
                                    {
                                        itemId = availableItems.result.items[j].itemid;
                                    }
                                }
                                if (itemId != string.Empty && !alreadyPickedTeams.Contains(teamPick))
                                {
                                    postData.Add(Properties.Settings.Default.tournamentItems + "&sectionid=" + deserializedLayoutResults.result.sections[sectionNumber].sectionid + "&groupid=" + radioTag.groupid + "&index=0&pickid=" + teamPick + "&itemid=" + itemId);
                                }
                            }
                        }
                    }
                    try
                    {
                        foreach (string postInformation in postData)
                        {
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.steampowered.com/ICSGOTournaments_730/UploadTournamentPredictions/v1?");
                            var data = Encoding.ASCII.GetBytes("key=" + Properties.Settings.Default.apiKey + postInformation);
                            request.Method = "POST";
                            request.ContentType = "application/x-www-form-urlencoded";
                            request.ContentLength = data.Length;
                            using (var stream = request.GetRequestStream())
                            {
                                stream.Write(data, 0, data.Length);
                            }
                            var response = (HttpWebResponse)request.GetResponse();
                            if (response.StatusCode == HttpStatusCode.Gone)
                            {
                                MessageBox.Show("One match has already begun, and therefore your submission for that match cannot be placed.\nThe program will continue placing the rest of your predictions.");
                            }
                            response.Close();
                            request = null;
                        }
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show("ERROR HONEY BADGER:\n\nThere was an issue submitting teams:" + exc.ToString());
                    }
                    updateAppearance();
                }
            }
        }

        private void isSelectionPossiblePickemPrediction(object sender, EventArgs e)
        {
            availableTeamStickers = getInventory.returnAvailableStickersTeams();
            availableItems = getInventory.deserializedInventoryResults;
            RadioButton currentRadio = (RadioButton)sender;
            Layout_Group radioTag = (Layout_Group)currentRadio.Tag;
            if (radioTag.picks_allowed.Equals("true"))
            { //If picks aren't allowed, there is no need to prompt the user to purchase a sticker...
                int teamPick;
                if (currentRadio.Name.Contains("box1")) //If it's box1, we want the first pickid, if it's box 2, its the 2nd team, so the second pickid
                {
                    teamPick = radioTag.teams[0].pickid;
                }
                else
                {
                    teamPick = radioTag.teams[1].pickid;
                }

                bool wasTeamFound = false; //Will be used for pulling up the market in a little bit...
                foreach (string availableTeams in availableTeamStickers)
                {
                    if (teamPick.ToString() == availableTeams) //If the team pickId is an available team sticker, allow the boolean to be set to be true
                    {
                        wasTeamFound = true;
                    }
                }

                if (!wasTeamFound)
                {
                    if (MessageBox.Show(this, "You do not have that team sticker. Would you like to go to the market now and purchase one?", "Team Sticker Unavailable", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        string stickerURL = "http://steamcommunity.com/market/search?q=&category_730_TournamentTeam%5B%5D=tag_Team" + teamPick + "&category_730_StickerCategory%5B%5D=tag_TeamLogo&category_730_Tournament%5B%5D=tag_Tournament" + Properties.Settings.Default.tournamentID + "&appid=730";
                        System.Diagnostics.Process.Start(stickerURL); //Opens up the Steam Market with a search for the team from teamPick
                    }
                    currentRadio.CheckedChanged -= new EventHandler(isSelectionPossiblePickemPrediction); //Remove the event temporarily so it is not triggered again
                    currentRadio.Checked = false;
                    currentRadio.CheckedChanged += new EventHandler(isSelectionPossiblePickemPrediction);
                }
            }
        }

        private void displayOnlyPlayersWithStickersOwner_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.displayOnlyPlayersWithStickersOwned = displayOnlyPlayersWithStickersOwner.Checked;
            Properties.Settings.Default.Save();
            updateFantasyAppearance();
        }

        private bool verifyPlayerStickerIsOwned(ComboBox combo)
        {
            if (!combo.Text.Equals("") && combo.Text != string.Empty && (!combo.Text.Contains("--- ")) && combo.SelectedItem != null)
            { //If null or a team header, don't do the check
                foreach (string playerName in availablePlayerStickers)
                {
                    if (combo.SelectedItem.Equals(playerName))
                    {
                        return true;
                    }
                }
                Dictionary<string, string> playerCodeNames = fantasyPlayers.getProPlayerDictionary("ProPlayerCodeNames"); //Get the dictionary with pro player names and their code name for the market
                string currentPlayerCodeName = string.Empty;
                playerCodeNames.TryGetValue(combo.Text, out currentPlayerCodeName);
                MessageBox.Show("ComboBox Text = " + combo.Text);
                if (MessageBox.Show(this, "You do not have that player sticker. Would you like to go to the market now and purchase one?", "Player Sticker Unavailable", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes && currentPlayerCodeName != string.Empty)
                {
                    string stickerURL = "http://steamcommunity.com/market/search?q=&category_730_ProPlayer%5B%5D=tag_" + currentPlayerCodeName + "&category_730_StickerCategory%5B%5D=tag_PlayerSignature&category_730_Tournament%5B%5D=tag_Tournament" + Properties.Settings.Default.tournamentID + "&appid=730";
                    System.Diagnostics.Process.Start(stickerURL); //Opens up the Steam Market with a search for the team from teamPick
                    combo.SelectedItem = null;
                }
            }
            return false;
        }

        private void refreshAppearance_Click(object sender, EventArgs e)
        {
            updateAppearance();
        }

        private void resetFantasy(object sender, EventArgs e)
        {
            updateFantasyAppearance();
        }

        private void submitFantasyLineup(object sender, EventArgs e)
        {
            Button submissionButton = (Button)sender;
            int fantasyListStartIndex = (Int32.Parse(submissionButton.Name.Substring(3, 1)) - 1) * 5; //Gets the day number, subtracts 1 for index of 0, and then multiplies by 5 to get the proper index to start with
            bool isEveryDropDownFilled = true;
            List<string> playerIds = new List<string>();
            for (int i = fantasyListStartIndex; i < (fantasyListStartIndex + 5); i++)
            {
                ComboBox currentCombo = (ComboBox)dropDownsToUpdate.ElementAt(i);
                if (currentCombo.SelectedItem == null)
                {
                    isEveryDropDownFilled = false;
                }
                else
                {
                    Dictionary<string, string> playerLookupDict = fantasyPlayers.getProPlayerDictionary("ProPlayerIds");
                    string playerId = string.Empty;
                    playerLookupDict.TryGetValue(currentCombo.Text, out playerId);
                    if (playerId != string.Empty)
                    {
                        playerIds.Add(playerId);
                    }
                }
            }

            if (isEveryDropDownFilled)
            {
                string postData = Properties.Settings.Default.tournamentPickemPredictions;
                postData += "&sectionid=" + deserializedLayoutResults.result.sections[Int32.Parse(submissionButton.Name.Substring(3, 1)) - 1].sectionid;
                string itemId = string.Empty;
                bool isPostDataCorrect = true;
                for (int i = 0; i < 5; i++)
                {
                    foreach (string items in availablePlayerStickers)
                    {
                        if (items.Equals(playerIds[i]))
                        {
                            itemId = items;
                        }
                    }
                    if (itemId != string.Empty)
                    {
                        postData += "pickid" + i + "=" + playerIds[i] + "&itemid" + i + "=" + itemId;
                    }
                    else
                    {
                        isPostDataCorrect = false;
                        MessageBox.Show("ERROR SHARK:\n\nThere was an issue submitting your fantasy roster. Player with issues: " + playerIds[i]);
                    }
                }

                if (isPostDataCorrect)
                {
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.steampowered.com/ICSGOTournaments_730/UploadTournamentFantasyLineupv1?");
                        var data = Encoding.ASCII.GetBytes("key=" + Properties.Settings.Default.apiKey + postData);
                        request.Method = "POST";
                        request.ContentType = "application/x-www-form-urlencoded";
                        request.ContentLength = data.Length;
                        using (var stream = request.GetRequestStream())
                        {
                            stream.Write(data, 0, data.Length);
                        }
                        var response = (HttpWebResponse)request.GetResponse();
                        if (response.StatusCode == HttpStatusCode.Gone)
                        {
                            MessageBox.Show("The first match of the day has already begun. You can not submit a lineup after a match has already taken place on that day.");
                        }
                        response.Close();
                        request = null;
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show("ERROR OCTOPUS:\n\nThere was an issue submitting teams:" + exc.ToString());
                    }
                    updateFantasyAppearance();
                }
            }
        }
    }
}
