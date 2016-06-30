﻿using System;
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
            dropDownsToUpdate = new List<object>() { day1clutchking, day1commando, day1ecowarrior, day1entryfragger, day1sniper, day2clutchking, day2commando, day2ecowarrior, day2entryfragger, day2sniper, day3clutchking, day3commando, day3ecowarrior, day3entryfragger, day3sniper, day4clutchking, day4commando, day4ecowarrior, day4entryfragger, day4sniper, day5clutchking, day5commando, day5ecowarrior, day5entryfragger, day5sniper, day6clutchking, day6commando, day6ecowarrior, day6entryfragger, day6sniper };
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
            generateFirstDayGroupNames(); //Sets group boxes and radio buttons names to the first day of groups

            if (deserializedLayoutResults.result.sections[1].groups[0].teams[0].pickid != 0)
            {
                generateSecondDayGroupNames(); //So long as the section exists, fill in radio buttons and group boxes with information
            }
            else
            {
                tabbedControls.TabPages[1].Enabled = false;
            }
            if (deserializedLayoutResults.result.sections[2].groups[0].teams[0].pickid != 0)
            {
                generateThirdDayGroupNames();
            }
            else
            {
                tabbedControls.TabPages[2].Enabled = false;
            }
            if (deserializedLayoutResults.result.sections[3].groups[0].teams[0].pickid != 0)
            {
                generateFourthDayGroupNames();
            }
            else
            {
                tabbedControls.TabPages[3].Enabled = false;
            }
            if (deserializedLayoutResults.result.sections[4].groups[0].teams[0].pickid != 0)
            {
                generateFifthDayGroupNames();
            }
            else
            {
                tabbedControls.TabPages[4].Enabled = false;
            }
            if (deserializedLayoutResults.result.sections[5].name.Contains("All Star"))
            {
                if (deserializedLayoutResults.result.sections[6].groups[0].teams[0].pickid != 0)
                {
                    generateSixthDayGroupNamesAllStar(); //This layout contains all-star game, so therefore we have to skip a section, so another method is needed
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
                    generateSixthDayGroupNames(); //This layout uses section 5, because there is no all-star game
                }
                else
                {
                    tabbedControls.TabPages[5].Enabled = false;
                }
            }

            foreach (RadioButton radio in matchBoxesToUpdate)
            {
                radio.BackColor = Color.White;
                radio.Checked = false;
                radio.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Regular);
            }

            if (deserializedLayoutResults.result.sections[0].groups[0].picks[0].pickids.Count != 0)
            {
                generateWinnerLosersDay1(); //If the losers and winners have been determined (they're in the picks jobject) color the winner green and loser red
            }
            if (deserializedLayoutResults.result.sections[1].groups[0].picks[0].pickids.Count != 0)
            {
                generateWinnerLosersDay2();
            }
            if (deserializedLayoutResults.result.sections[2].groups[0].picks[0].pickids.Count != 0)
            {
                generateWinnerLosersDay3();
            }
            if (deserializedLayoutResults.result.sections[3].groups[0].picks[0].pickids.Count != 0)
            {
                generateWinnerLosersDay4();
            }
            if (deserializedLayoutResults.result.sections[4].groups[0].picks[0].pickids.Count != 0)
            {
                generateWinnerLosersDay5();
            }
            if (deserializedLayoutResults.result.sections[5].name.Contains("All Star"))
            {
                if (deserializedLayoutResults.result.sections[6].groups[0].picks[0].pickids.Count != 0)
                {
                    generateWinnerLosersDay6AllStar();
                }
            }
            else
            {
                if (deserializedLayoutResults.result.sections[5].groups[0].picks[0].pickids.Count != 0)
                {
                    generateWinnerLosersDay6();
                }
            }

            if (Properties.Settings.Default.tournamentPredictions != String.Empty && Properties.Settings.Default.tournamentPredictions != "")
            {
                generatePredictionDeserializedObject(); //Get deserialized result of team predictions
                if (deserializedPredictionResults.result.picks.Count != 0)
                {
                    if (deserializedPredictionResults.result.picks[0] != null)
                    {
                        generateRadioButtonUserPicksDay1(); //If picks were made, then show which team the user picked
                    }
                    if (deserializedPredictionResults.result.picks.Count > 1 && deserializedPredictionResults.result.picks[1] != null)
                    {
                        generateRadioButtonUserPicksDay2();
                    }
                    if (deserializedPredictionResults.result.picks.Count > 2 && deserializedPredictionResults.result.picks[2] != null)
                    {
                        generateRadioButtonUserPicksDay3();
                    }
                    if (deserializedPredictionResults.result.picks.Count > 3 && deserializedPredictionResults.result.picks[3] != null)
                    {
                        generateRadioButtonUserPicksDay4();
                    }
                    if (deserializedPredictionResults.result.picks.Count > 4 && deserializedPredictionResults.result.picks[4] != null)
                    {
                        generateRadioButtonUserPicksDay5();
                    }
                    if (deserializedPredictionResults.result.picks.Count > 5 && deserializedPredictionResults.result.picks[5] != null)
                    {
                        generateRadioButtonUserPicksDay6();
                    }
                }
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
                /*
                 * The following was made before finding out about their API to get all stickers, so this queries the user inventory and manually identifies
                 * stickers that correlate to the current selected tournament. This takes the tournament name property and identifies if the sticker has
                 * the tournament name anywhere inside of it (E.g. Sticker | Team Liquid | MLG Columbus 2016 contains MLG Columbus 2016), and then furthermore
                 * sees if it contains a team name for the teamStickers list and if it contains a pro player name from the pro player list
                 * 
                 * This should probably be redone in the future, but this may be more reliable if the CSGO Developers are slow in releasing updated API for 
                 * the next tournament. It will also depend if the sticker naming conventions ever change.
                */
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
                fantasyTournamentToUse.Text = Properties.Settings.Default.fantasyTournament.ToString(); //Updates the settings page for fantasy pick'em
                selectedEventLbl.Text = retrieveEventName(Properties.Settings.Default.tournamentLayout + Properties.Settings.Default.fantasyTournament);
                fantasyStatisticsRange.Text = Properties.Settings.Default.fantasyStats;
                displayOnlyAvailable.Checked = Properties.Settings.Default.displayOnlyAvailable;
                playerSortingOrderCombo.Text = Properties.Settings.Default.fantasyPlayerSortOrder;
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

        /*
         * The following set of methods (generate[Number]DayGroupNames) day each group box name to the match name and points for the pick, 
         * set the tag of the match box to be the group jobject, set each radio button to the proper team names, and then also set the radio button
         * tags to the group jobject as well, which is used later to determine a users pick, and also winners. The match box is also disabled if
         * the layout deserialization indicates that the picks_allowed = false. This just does this for every tab page.
         * 
         * TODO: I assume there is a much better way of doing this, but for now it works, and I may need to ask some more experienced UI individuals how
         * to improve this code, or at least shrink it down tremendously. Might have to do a foreach loop like for the fantasy players?
        */
        private void generateFirstDayGroupNames()
        {
            //Match 1
            day1matchBox1.Text = deserializedLayoutResults.result.sections[0].groups[0].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[0].groups[0].points_per_pick;
            day1matchBox1.Tag = deserializedLayoutResults.result.sections[0].groups[0];
            day1match1box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[0].groups[0].teams[0].pickid);
            day1match1box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[0].groups[0].teams[1].pickid);
            day1match1box1.Tag = deserializedLayoutResults.result.sections[0].groups[0];
            day1match1box2.Tag = deserializedLayoutResults.result.sections[0].groups[0];
            if (deserializedLayoutResults.result.sections[0].groups[0].picks_allowed == false)
            {
                day1matchBox1.Enabled = false;
            }

            //Match 2
            day1matchBox2.Text = deserializedLayoutResults.result.sections[0].groups[1].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[0].groups[0].points_per_pick;
            day1matchBox2.Tag = deserializedLayoutResults.result.sections[0].groups[1];
            day1match2box1.Tag = deserializedLayoutResults.result.sections[0].groups[1];
            day1match2box2.Tag = deserializedLayoutResults.result.sections[0].groups[1];
            day1match2box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[0].groups[1].teams[0].pickid);
            day1match2box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[0].groups[1].teams[1].pickid);
            if (deserializedLayoutResults.result.sections[0].groups[1].picks_allowed == false)
            {
                day1matchBox2.Enabled = false;
            }

            //Match 3
            day1matchBox3.Text = deserializedLayoutResults.result.sections[0].groups[2].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[0].groups[0].points_per_pick;
            day1matchBox3.Tag = deserializedLayoutResults.result.sections[0].groups[2];
            day1match3box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[0].groups[2].teams[0].pickid);
            day1match3box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[0].groups[2].teams[1].pickid);
            day1match3box1.Tag = deserializedLayoutResults.result.sections[0].groups[2];
            day1match3box2.Tag = deserializedLayoutResults.result.sections[0].groups[2];
            if (deserializedLayoutResults.result.sections[0].groups[2].picks_allowed == false)
            {
                day1matchBox3.Enabled = false;
            }

            //Match 4
            day1matchBox4.Text = deserializedLayoutResults.result.sections[0].groups[3].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[0].groups[0].points_per_pick;
            day1matchBox4.Tag = deserializedLayoutResults.result.sections[0].groups[3];
            day1match4box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[0].groups[3].teams[0].pickid);
            day1match4box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[0].groups[3].teams[1].pickid);
            day1match4box1.Tag = deserializedLayoutResults.result.sections[0].groups[3];
            day1match4box2.Tag = deserializedLayoutResults.result.sections[0].groups[3];
            if (deserializedLayoutResults.result.sections[0].groups[3].picks_allowed == false)
            {
                day1matchBox4.Enabled = false;
            }

            //Match 5
            day1matchBox5.Text = deserializedLayoutResults.result.sections[0].groups[4].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[0].groups[0].points_per_pick;
            day1matchBox5.Tag = deserializedLayoutResults.result.sections[0].groups[4];
            day1match5box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[0].groups[4].teams[0].pickid);
            day1match5box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[0].groups[4].teams[1].pickid);
            day1match5box1.Tag = deserializedLayoutResults.result.sections[0].groups[4];
            day1match5box2.Tag = deserializedLayoutResults.result.sections[0].groups[4];
            if (deserializedLayoutResults.result.sections[0].groups[4].picks_allowed == false)
            {
                day1matchBox5.Enabled = false;
            }

            //Match 6
            day1matchBox6.Text = deserializedLayoutResults.result.sections[0].groups[5].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[0].groups[0].points_per_pick;
            day1matchBox6.Tag = deserializedLayoutResults.result.sections[0].groups[5];
            day1match6box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[0].groups[5].teams[0].pickid);
            day1match6box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[0].groups[5].teams[1].pickid);
            day1match6box1.Tag = deserializedLayoutResults.result.sections[0].groups[5];
            day1match6box2.Tag = deserializedLayoutResults.result.sections[0].groups[5];
            if (deserializedLayoutResults.result.sections[0].groups[5].picks_allowed == false)
            {
                day1matchBox6.Enabled = false;
            }

            //Match 7
            day1matchBox7.Text = deserializedLayoutResults.result.sections[0].groups[6].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[0].groups[0].points_per_pick;
            day1matchBox7.Tag = deserializedLayoutResults.result.sections[0].groups[6];
            day1match7box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[0].groups[6].teams[0].pickid);
            day1match7box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[0].groups[6].teams[1].pickid);
            day1match7box1.Tag = deserializedLayoutResults.result.sections[0].groups[6];
            day1match7box2.Tag = deserializedLayoutResults.result.sections[0].groups[6];
            if (deserializedLayoutResults.result.sections[0].groups[6].picks_allowed == false)
            {
                day1matchBox7.Enabled = false;
            }

            //Match 8
            day1matchBox8.Text = deserializedLayoutResults.result.sections[0].groups[7].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[0].groups[0].points_per_pick;
            day1matchBox8.Tag = deserializedLayoutResults.result.sections[0].groups[7];
            day1match8box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[0].groups[7].teams[0].pickid);
            day1match8box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[0].groups[7].teams[1].pickid);
            day1match8box1.Tag = deserializedLayoutResults.result.sections[0].groups[7];
            day1match8box2.Tag = deserializedLayoutResults.result.sections[0].groups[7];
            if (deserializedLayoutResults.result.sections[0].groups[7].picks_allowed == false)
            {
                day1matchBox8.Enabled = false;
            }
        }

        private void generateSecondDayGroupNames()
        {
            //Match 1
            day2matchBox1.Text = deserializedLayoutResults.result.sections[1].groups[0].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[1].groups[0].points_per_pick;
            day2matchBox1.Tag = deserializedLayoutResults.result.sections[1].groups[0];
            day2match1box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[1].groups[0].teams[0].pickid);
            day2match1box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[1].groups[0].teams[1].pickid);
            day2match1box1.Tag = deserializedLayoutResults.result.sections[1].groups[0];
            day2match1box2.Tag = deserializedLayoutResults.result.sections[1].groups[0];
            if (deserializedLayoutResults.result.sections[1].groups[0].picks_allowed == false)
            {
                day2matchBox1.Enabled = false;
            }

            //Match 2
            day2matchBox2.Text = deserializedLayoutResults.result.sections[1].groups[1].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[1].groups[0].points_per_pick;
            day2matchBox2.Tag = deserializedLayoutResults.result.sections[1].groups[1];
            day2match2box1.Tag = deserializedLayoutResults.result.sections[1].groups[1];
            day2match2box2.Tag = deserializedLayoutResults.result.sections[1].groups[1];
            day2match2box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[1].groups[1].teams[0].pickid);
            day2match2box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[1].groups[1].teams[1].pickid);
            if (deserializedLayoutResults.result.sections[1].groups[1].picks_allowed == false)
            {
                day2matchBox2.Enabled = false;
            }

            //Match 3
            day2matchBox3.Text = deserializedLayoutResults.result.sections[1].groups[2].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[1].groups[0].points_per_pick;
            day2matchBox3.Tag = deserializedLayoutResults.result.sections[1].groups[2];
            day2match3box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[1].groups[2].teams[0].pickid);
            day2match3box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[1].groups[2].teams[1].pickid);
            day2match3box1.Tag = deserializedLayoutResults.result.sections[1].groups[2];
            day2match3box2.Tag = deserializedLayoutResults.result.sections[1].groups[2];
            if (deserializedLayoutResults.result.sections[1].groups[2].picks_allowed == false)
            {
                day2matchBox3.Enabled = false;
            }

            //Match 4
            day2matchBox4.Text = deserializedLayoutResults.result.sections[1].groups[3].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[1].groups[0].points_per_pick;
            day2matchBox4.Tag = deserializedLayoutResults.result.sections[1].groups[3];
            day2match4box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[1].groups[3].teams[0].pickid);
            day2match4box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[1].groups[3].teams[1].pickid);
            day2match4box1.Tag = deserializedLayoutResults.result.sections[1].groups[3];
            day2match4box2.Tag = deserializedLayoutResults.result.sections[1].groups[3];
            if (deserializedLayoutResults.result.sections[1].groups[3].picks_allowed == false)
            {
                day2matchBox4.Enabled = false;
            }

            //Match 5
            day2matchBox5.Text = deserializedLayoutResults.result.sections[1].groups[4].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[1].groups[0].points_per_pick;
            day2matchBox5.Tag = deserializedLayoutResults.result.sections[1].groups[4];
            day2match5box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[1].groups[4].teams[0].pickid);
            day2match5box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[1].groups[4].teams[1].pickid);
            day2match5box1.Tag = deserializedLayoutResults.result.sections[1].groups[4];
            day2match5box2.Tag = deserializedLayoutResults.result.sections[1].groups[4];
            if (deserializedLayoutResults.result.sections[1].groups[4].picks_allowed == false)
            {
                day2matchBox5.Enabled = false;
            }

            //Match 6
            day2matchBox6.Text = deserializedLayoutResults.result.sections[1].groups[5].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[1].groups[0].points_per_pick;
            day2matchBox6.Tag = deserializedLayoutResults.result.sections[1].groups[5];
            day2match6box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[1].groups[5].teams[0].pickid);
            day2match6box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[1].groups[5].teams[1].pickid);
            day2match6box1.Tag = deserializedLayoutResults.result.sections[1].groups[5];
            day2match6box2.Tag = deserializedLayoutResults.result.sections[1].groups[5];
            if (deserializedLayoutResults.result.sections[1].groups[5].picks_allowed == false)
            {
                day2matchBox6.Enabled = false;
            }

            //Match 7
            day2matchBox7.Text = deserializedLayoutResults.result.sections[1].groups[6].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[1].groups[0].points_per_pick;
            day2matchBox7.Tag = deserializedLayoutResults.result.sections[1].groups[6];
            day2match7box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[1].groups[6].teams[0].pickid);
            day2match7box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[1].groups[6].teams[1].pickid);
            day2match7box1.Tag = deserializedLayoutResults.result.sections[1].groups[6];
            day2match7box2.Tag = deserializedLayoutResults.result.sections[1].groups[6];
            if (deserializedLayoutResults.result.sections[1].groups[6].picks_allowed == false)
            {
                day2matchBox7.Enabled = false;
            }

            //Match 8
            day2matchBox8.Text = deserializedLayoutResults.result.sections[1].groups[7].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[1].groups[0].points_per_pick;
            day2matchBox8.Tag = deserializedLayoutResults.result.sections[1].groups[7];
            day2match8box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[1].groups[7].teams[0].pickid);
            day2match8box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[1].groups[7].teams[1].pickid);
            day2match8box1.Tag = deserializedLayoutResults.result.sections[1].groups[7];
            day2match8box2.Tag = deserializedLayoutResults.result.sections[1].groups[7];
            if (deserializedLayoutResults.result.sections[1].groups[7].picks_allowed == false)
            {
                day2matchBox8.Enabled = false;
            }
        }

        private void generateThirdDayGroupNames()
        {
            //Match 1
            day3matchBox1.Text = deserializedLayoutResults.result.sections[2].groups[0].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[2].groups[0].points_per_pick;
            day3matchBox1.Tag = deserializedLayoutResults.result.sections[2].groups[0];
            day3match1box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[2].groups[0].teams[0].pickid);
            day3match1box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[2].groups[0].teams[1].pickid);
            day3match1box1.Tag = deserializedLayoutResults.result.sections[2].groups[0];
            day3match1box2.Tag = deserializedLayoutResults.result.sections[2].groups[0];
            if (deserializedLayoutResults.result.sections[2].groups[0].picks_allowed == false)
            {
                day3matchBox1.Enabled = false;
            }

            //Match 2
            day3matchBox2.Text = deserializedLayoutResults.result.sections[2].groups[1].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[2].groups[0].points_per_pick;
            day3matchBox2.Tag = deserializedLayoutResults.result.sections[2].groups[1];
            day3match2box1.Tag = deserializedLayoutResults.result.sections[2].groups[1];
            day3match2box2.Tag = deserializedLayoutResults.result.sections[2].groups[1];
            day3match2box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[2].groups[1].teams[0].pickid);
            day3match2box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[2].groups[1].teams[1].pickid);
            if (deserializedLayoutResults.result.sections[2].groups[1].picks_allowed == false)
            {
                day3matchBox2.Enabled = false;
            }

            //Match 3
            day3matchBox3.Text = deserializedLayoutResults.result.sections[2].groups[2].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[2].groups[0].points_per_pick;
            day3matchBox3.Tag = deserializedLayoutResults.result.sections[2].groups[2];
            day3match3box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[2].groups[2].teams[0].pickid);
            day3match3box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[2].groups[2].teams[1].pickid);
            day3match3box1.Tag = deserializedLayoutResults.result.sections[2].groups[2];
            day3match3box2.Tag = deserializedLayoutResults.result.sections[2].groups[2];
            if (deserializedLayoutResults.result.sections[2].groups[2].picks_allowed == false)
            {
                day3matchBox3.Enabled = false;
            }

            //Match 4
            day3matchBox4.Text = deserializedLayoutResults.result.sections[2].groups[3].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[2].groups[0].points_per_pick;
            day3matchBox4.Tag = deserializedLayoutResults.result.sections[2].groups[3];
            day3match4box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[2].groups[3].teams[0].pickid);
            day3match4box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[2].groups[3].teams[1].pickid);
            day3match4box1.Tag = deserializedLayoutResults.result.sections[2].groups[3];
            day3match4box2.Tag = deserializedLayoutResults.result.sections[2].groups[3];
            if (deserializedLayoutResults.result.sections[2].groups[3].picks_allowed == false)
            {
                day3matchBox4.Enabled = false;
            }
        }

        private void generateFourthDayGroupNames()
        {
            //Match 1
            day4matchBox1.Text = deserializedLayoutResults.result.sections[3].groups[0].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[3].groups[0].points_per_pick;
            day4matchBox1.Tag = deserializedLayoutResults.result.sections[3].groups[0];
            day4match1box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[3].groups[0].teams[0].pickid);
            day4match1box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[3].groups[0].teams[1].pickid);
            day4match1box1.Tag = deserializedLayoutResults.result.sections[3].groups[0];
            day4match1box2.Tag = deserializedLayoutResults.result.sections[3].groups[0];
            if (deserializedLayoutResults.result.sections[3].groups[0].picks_allowed == false)
            {
                day4matchBox1.Enabled = false;
            }

            //Match 2
            day4matchBox2.Text = deserializedLayoutResults.result.sections[3].groups[1].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[3].groups[0].points_per_pick;
            day4matchBox2.Tag = deserializedLayoutResults.result.sections[3].groups[1];
            day4match2box1.Tag = deserializedLayoutResults.result.sections[3].groups[1];
            day4match2box2.Tag = deserializedLayoutResults.result.sections[3].groups[1];
            day4match2box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[3].groups[1].teams[0].pickid);
            day4match2box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[3].groups[1].teams[1].pickid);
            if (deserializedLayoutResults.result.sections[3].groups[1].picks_allowed == false)
            {
                day4matchBox2.Enabled = false;
            }

            //Match 3
            day4matchBox3.Text = deserializedLayoutResults.result.sections[3].groups[2].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[3].groups[0].points_per_pick;
            day4matchBox3.Tag = deserializedLayoutResults.result.sections[3].groups[2];
            day4match3box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[3].groups[2].teams[0].pickid);
            day4match3box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[3].groups[2].teams[1].pickid);
            day4match3box1.Tag = deserializedLayoutResults.result.sections[3].groups[2];
            day4match3box2.Tag = deserializedLayoutResults.result.sections[3].groups[2];
            if (deserializedLayoutResults.result.sections[3].groups[2].picks_allowed == false)
            {
                day4matchBox3.Enabled = false;
            }

            //Match 4
            day4matchBox4.Text = deserializedLayoutResults.result.sections[3].groups[3].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[3].groups[0].points_per_pick;
            day4matchBox4.Tag = deserializedLayoutResults.result.sections[3].groups[3];
            day4match4box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[3].groups[3].teams[0].pickid);
            day4match4box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[3].groups[3].teams[1].pickid);
            day4match4box1.Tag = deserializedLayoutResults.result.sections[3].groups[3];
            day4match4box2.Tag = deserializedLayoutResults.result.sections[3].groups[3];
            if (deserializedLayoutResults.result.sections[3].groups[3].picks_allowed == false)
            {
                day4matchBox4.Enabled = false;
            }
        }

        private void generateFifthDayGroupNames()
        {
            //Match 1
            day5matchBox1.Text = deserializedLayoutResults.result.sections[4].groups[0].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[4].groups[0].points_per_pick;
            day5matchBox1.Tag = deserializedLayoutResults.result.sections[4].groups[0];
            day5match1box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[4].groups[0].teams[0].pickid);
            day5match1box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[4].groups[0].teams[1].pickid);
            day5match1box1.Tag = deserializedLayoutResults.result.sections[4].groups[0];
            day5match1box2.Tag = deserializedLayoutResults.result.sections[4].groups[0];
            if (deserializedLayoutResults.result.sections[4].groups[0].picks_allowed == false)
            {
                day5matchBox1.Enabled = false;
            }

            //Match 2
            day5matchBox2.Text = deserializedLayoutResults.result.sections[4].groups[1].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[4].groups[0].points_per_pick;
            day5matchBox2.Tag = deserializedLayoutResults.result.sections[4].groups[1];
            day5match2box1.Tag = deserializedLayoutResults.result.sections[4].groups[1];
            day5match2box2.Tag = deserializedLayoutResults.result.sections[4].groups[1];
            day5match2box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[4].groups[1].teams[0].pickid);
            day5match2box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[4].groups[1].teams[1].pickid);
            if (deserializedLayoutResults.result.sections[4].groups[1].picks_allowed == false)
            {
                day5matchBox2.Enabled = false;
            }
        }

        private void generateSixthDayGroupNames()
        {
            //Match 1
            day6matchBox1.Text = deserializedLayoutResults.result.sections[5].groups[0].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[5].groups[0].points_per_pick;
            day6matchBox1.Tag = deserializedLayoutResults.result.sections[5].groups[0];
            day6match1box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[5].groups[0].teams[0].pickid);
            day6match1box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[5].groups[0].teams[1].pickid);
            day6match1box1.Tag = deserializedLayoutResults.result.sections[5].groups[0];
            day6match1box2.Tag = deserializedLayoutResults.result.sections[5].groups[0];
            if (deserializedLayoutResults.result.sections[5].groups[0].picks_allowed == false)
            {
                day6matchBox1.Enabled = false;
            }
        }

        private void generateSixthDayGroupNamesAllStar()
        {
            //Match 1
            day6matchBox1.Text = deserializedLayoutResults.result.sections[6].groups[0].name + " -  Points for Match: " + deserializedLayoutResults.result.sections[6].groups[0].points_per_pick;
            day6matchBox1.Tag = deserializedLayoutResults.result.sections[6].groups[0];
            day6match1box1.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[6].groups[0].teams[0].pickid);
            day6match1box2.Text = getTeamNameFromPickId(deserializedLayoutResults.result.sections[6].groups[0].teams[1].pickid);
            day6match1box1.Tag = deserializedLayoutResults.result.sections[6].groups[0];
            day6match1box2.Tag = deserializedLayoutResults.result.sections[6].groups[0];
            if (deserializedLayoutResults.result.sections[6].groups[0].picks_allowed == false)
            {
                day6matchBox1.Enabled = false;
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
            MessageBox.Show("ERROR: PANDA\n\nThere was an issue generating team names from the provided picking options");
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

        /*
         * The following methods (generateRadioButtonUserPickdsDay[Day#]) all have the same function, they just operate on different tabs
         * 
         * They identify the user picks for each set of matches and check the corresponding box.
         * 
         * TODO: Find out how to simplify this, much like the Group Name generation. I'm certain this can be cleaned up, but I don't know how quite frankly 
        */
        private void generateRadioButtonUserPicksDay1()
        {
            for (int i = 0; i < deserializedPredictionResults.result.picks.Count; i++)
            {
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day1matchBox1, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day1matchBox1, 'p', 0))
                    {
                        day1match1box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day1matchBox1, 'p', 1))
                    {
                        day1match1box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day1matchBox2, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day1matchBox2, 'p', 0))
                    {
                        day1match2box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day1matchBox2, 'p', 1))
                    {
                        day1match2box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day1matchBox3, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day1matchBox3, 'p', 0))
                    {
                        day1match3box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day1matchBox3, 'p', 1))
                    {
                        day1match3box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day1matchBox4, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day1matchBox4, 'p', 0))
                    {
                        day1match4box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day1matchBox4, 'p', 1))
                    {
                        day1match4box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day1matchBox5, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day1matchBox5, 'p', 0))
                    {
                        day1match5box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day1matchBox5, 'p', 1))
                    {
                        day1match5box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day1matchBox6, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day1matchBox6, 'p', 0))
                    {
                        day1match6box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day1matchBox6, 'p', 1))
                    {
                        day1match6box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day1matchBox7, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day1matchBox7, 'p', 0))
                    {
                        day1match7box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day1matchBox7, 'p', 1))
                    {
                        day1match7box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day1matchBox8, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day1matchBox8, 'p', 0))
                    {
                        day1match8box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day1matchBox8, 'p', 1))
                    {
                        day1match8box2.Checked = true;
                    }
                }
            }
        }

        private void generateRadioButtonUserPicksDay2()
        {
            for (int i = 0; i < deserializedPredictionResults.result.picks.Count; i++)
            {
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day2matchBox1, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day2matchBox1, 'p', 0))
                    {
                        day2match1box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day2matchBox1, 'p', 1))
                    {
                        day2match1box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day2matchBox2, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day2matchBox2, 'p', 0))
                    {
                        day2match2box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day2matchBox2, 'p', 1))
                    {
                        day2match2box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day2matchBox3, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day2matchBox3, 'p', 0))
                    {
                        day2match3box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day2matchBox3, 'p', 1))
                    {
                        day2match3box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day2matchBox4, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day2matchBox4, 'p', 0))
                    {
                        day2match4box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day2matchBox4, 'p', 1))
                    {
                        day2match4box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day2matchBox5, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day2matchBox5, 'p', 0))
                    {
                        day2match5box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day2matchBox5, 'p', 1))
                    {
                        day2match5box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day2matchBox6, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day2matchBox6, 'p', 0))
                    {
                        day2match6box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day2matchBox6, 'p', 1))
                    {
                        day2match6box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day2matchBox7, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day2matchBox7, 'p', 0))
                    {
                        day2match7box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day2matchBox7, 'p', 1))
                    {
                        day2match7box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day2matchBox8, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day2matchBox8, 'p', 0))
                    {
                        day2match8box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day2matchBox8, 'p', 1))
                    {
                        day2match8box2.Checked = true;
                    }
                }
            }
        }

        private void generateRadioButtonUserPicksDay3()
        {
            for (int i = 0; i < deserializedPredictionResults.result.picks.Count; i++)
            {
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day3matchBox1, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day3matchBox1, 'p', 0))
                    {
                        day3match1box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day3matchBox1, 'p', 1))
                    {
                        day3match1box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day3matchBox2, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day3matchBox2, 'p', 0))
                    {
                        day3match2box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day3matchBox2, 'p', 1))
                    {
                        day3match2box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day3matchBox3, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day3matchBox3, 'p', 0))
                    {
                        day3match3box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day3matchBox3, 'p', 1))
                    {
                        day3match3box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day3matchBox4, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day3matchBox4, 'p', 0))
                    {
                        day3match4box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day3matchBox4, 'p', 1))
                    {
                        day3match4box2.Checked = true;
                    }
                }
            }
        }

        private void generateRadioButtonUserPicksDay4()
        {
            for (int i = 0; i < deserializedPredictionResults.result.picks.Count; i++)
            {
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day4matchBox1, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day4matchBox1, 'p', 0))
                    {
                        day4match1box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day4matchBox1, 'p', 1))
                    {
                        day4match1box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day4matchBox2, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day4matchBox2, 'p', 0))
                    {
                        day4match2box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day4matchBox2, 'p', 1))
                    {
                        day4match2box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day4matchBox3, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day4matchBox3, 'p', 0))
                    {
                        day4match3box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day4matchBox3, 'p', 1))
                    {
                        day4match3box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day4matchBox4, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day4matchBox4, 'p', 0))
                    {
                        day4match4box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day4matchBox4, 'p', 1))
                    {
                        day4match4box2.Checked = true;
                    }
                }
            }
        }

        private void generateRadioButtonUserPicksDay5()
        {
            for (int i = 0; i < deserializedPredictionResults.result.picks.Count; i++)
            {
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day5matchBox1, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day5matchBox1, 'p', 0))
                    {
                        day5match1box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day5matchBox1, 'p', 1))
                    {
                        day5match1box2.Checked = true;
                    }
                }
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day5matchBox2, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day5matchBox2, 'p', 0))
                    {
                        day5match2box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day5matchBox2, 'p', 1))
                    {
                        day5match2box2.Checked = true;
                    }
                }
            }
        }

        private void generateRadioButtonUserPicksDay6()
        {
            for (int i = 0; i < deserializedPredictionResults.result.picks.Count; i++)
            {
                if (deserializedPredictionResults.result.picks[i].groupid == getTagInfo(day6matchBox1, 'g'))
                {
                    if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day6matchBox1, 'p', 0))
                    {
                        day6match1box1.Checked = true;
                    }
                    else if (deserializedPredictionResults.result.picks[i].pick == getTagInfo(day6matchBox1, 'p', 1))
                    {
                        day6match1box2.Checked = true;
                    }
                }
            }
        }

        /*
         * The following methods (generateWinnerLosersDay[Day#]) all have the same function, they just operate on different tabs
         * 
         * They identify which team one by the given tag on each group box, and if the team in day[#]match[#]box[#] was the winner, their side is
         * made green indicating they won, the losers side is marked red, and the winner side is marked bold
         * 
         * This doesn't look as ugly as I intiially thought it would. I wanted to make exclusively text red/green, but when a control object is disabled,
         * the text automatically goes to grey, and without overriding the paint function it would not be possible. This is a much easier, cleaner, and
         * overall more aesthetically pleasing than the text option.
         * 
         * TODO: Clean this up, much like the generateRadioButtonUserPicks and generateMatches, because I'm certain this can be cleaned up immensley, and
         * this takes up a lot of space.
        */
        private void generateWinnerLosersDay1()
        {
            for (int i = 0; i < deserializedLayoutResults.result.sections[0].groups.Count; i++)
            {
                //Match 1
                if (getTagInfo(day1match1box1, 'p', 0) == deserializedLayoutResults.result.sections[0].groups[0].picks[0].pickids[0])
                {
                    day1match1box1.BackColor = Color.Green;
                    day1match1box2.BackColor = Color.Red;
                    day1match1box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day1match1box2, 'p', 1) == deserializedLayoutResults.result.sections[0].groups[0].picks[0].pickids[0])
                {
                    day1match1box1.BackColor = Color.Red;
                    day1match1box2.BackColor = Color.Green;
                    day1match1box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 2
                if (getTagInfo(day1match2box1, 'p', 0) == deserializedLayoutResults.result.sections[0].groups[1].picks[0].pickids[0])
                {
                    day1match2box1.BackColor = Color.Green;
                    day1match2box2.BackColor = Color.Red;
                    day1match2box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day1match2box2, 'p', 1) == deserializedLayoutResults.result.sections[0].groups[1].picks[0].pickids[0])
                {
                    day1match2box1.BackColor = Color.Red;
                    day1match2box2.BackColor = Color.Green;
                    day1match2box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 3
                if (getTagInfo(day1match3box1, 'p', 0) == deserializedLayoutResults.result.sections[0].groups[2].picks[0].pickids[0])
                {
                    day1match3box1.BackColor = Color.Green;
                    day1match3box2.BackColor = Color.Red;
                    day1match3box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day1match3box2, 'p', 1) == deserializedLayoutResults.result.sections[0].groups[2].picks[0].pickids[0])
                {
                    day1match3box1.BackColor = Color.Red;
                    day1match3box2.BackColor = Color.Green;
                    day1match3box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 4
                if (getTagInfo(day1match4box1, 'p', 0) == deserializedLayoutResults.result.sections[0].groups[3].picks[0].pickids[0])
                {
                    day1match4box1.BackColor = Color.Green;
                    day1match4box2.BackColor = Color.Red;
                    day1match4box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day1match4box2, 'p', 1) == deserializedLayoutResults.result.sections[0].groups[3].picks[0].pickids[0])
                {
                    day1match4box1.BackColor = Color.Red;
                    day1match4box2.BackColor = Color.Green;
                    day1match4box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 5
                if (getTagInfo(day1match5box1, 'p', 0) == deserializedLayoutResults.result.sections[0].groups[4].picks[0].pickids[0])
                {
                    day1match5box1.BackColor = Color.Green;
                    day1match5box2.BackColor = Color.Red;
                    day1match5box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day1match5box2, 'p', 1) == deserializedLayoutResults.result.sections[0].groups[4].picks[0].pickids[0])
                {
                    day1match5box1.BackColor = Color.Red;
                    day1match5box2.BackColor = Color.Green;
                    day1match5box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 6
                if (getTagInfo(day1match6box1, 'p', 0) == deserializedLayoutResults.result.sections[0].groups[5].picks[0].pickids[0])
                {
                    day1match6box1.BackColor = Color.Green;
                    day1match6box2.BackColor = Color.Red;
                    day1match6box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day1match6box2, 'p', 1) == deserializedLayoutResults.result.sections[0].groups[5].picks[0].pickids[0])
                {
                    day1match6box1.BackColor = Color.Red;
                    day1match6box2.BackColor = Color.Green;
                    day1match6box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 7
                if (getTagInfo(day1match7box1, 'p', 0) == deserializedLayoutResults.result.sections[0].groups[6].picks[0].pickids[0])
                {
                    day1match7box1.BackColor = Color.Green;
                    day1match7box2.BackColor = Color.Red;
                    day1match7box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day1match7box2, 'p', 1) == deserializedLayoutResults.result.sections[0].groups[6].picks[0].pickids[0])
                {
                    day1match7box1.BackColor = Color.Red;
                    day1match7box2.BackColor = Color.Green;
                    day1match7box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 8
                if (getTagInfo(day1match8box1, 'p', 0) == deserializedLayoutResults.result.sections[0].groups[7].picks[0].pickids[0])
                {
                    day1match8box1.BackColor = Color.Green;
                    day1match8box2.BackColor = Color.Red;
                    day1match8box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day1match8box2, 'p', 1) == deserializedLayoutResults.result.sections[0].groups[7].picks[0].pickids[0])
                {
                    day1match8box1.BackColor = Color.Red;
                    day1match8box2.BackColor = Color.Green;
                    day1match8box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
            }
        }

        private void generateWinnerLosersDay2()
        {
            for (int i = 0; i < deserializedLayoutResults.result.sections[1].groups.Count; i++)
            {
                //Match 1
                if (getTagInfo(day2match1box1, 'p', 0) == deserializedLayoutResults.result.sections[1].groups[0].picks[0].pickids[0])
                {
                    day2match1box1.BackColor = Color.Green;
                    day2match1box2.BackColor = Color.Red;
                    day2match1box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day2match1box2, 'p', 1) == deserializedLayoutResults.result.sections[1].groups[0].picks[0].pickids[0])
                {
                    day2match1box1.BackColor = Color.Red;
                    day2match1box2.BackColor = Color.Green;
                    day2match1box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 2
                if (getTagInfo(day2match2box1, 'p', 0) == deserializedLayoutResults.result.sections[1].groups[1].picks[0].pickids[0])
                {
                    day2match2box1.BackColor = Color.Green;
                    day2match2box2.BackColor = Color.Red;
                    day2match2box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day2match2box2, 'p', 1) == deserializedLayoutResults.result.sections[1].groups[1].picks[0].pickids[0])
                {
                    day2match2box1.BackColor = Color.Red;
                    day2match2box2.BackColor = Color.Green;
                    day2match2box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 3
                if (getTagInfo(day2match3box1, 'p', 0) == deserializedLayoutResults.result.sections[1].groups[2].picks[0].pickids[0])
                {
                    day2match3box1.BackColor = Color.Green;
                    day2match3box2.BackColor = Color.Red;
                    day2match3box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day2match3box2, 'p', 1) == deserializedLayoutResults.result.sections[1].groups[2].picks[0].pickids[0])
                {
                    day2match3box1.BackColor = Color.Red;
                    day2match3box2.BackColor = Color.Green;
                    day2match3box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 4
                if (getTagInfo(day2match4box1, 'p', 0) == deserializedLayoutResults.result.sections[1].groups[3].picks[0].pickids[0])
                {
                    day2match4box1.BackColor = Color.Green;
                    day2match4box2.BackColor = Color.Red;
                    day2match4box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day2match4box2, 'p', 1) == deserializedLayoutResults.result.sections[1].groups[3].picks[0].pickids[0])
                {
                    day2match4box1.BackColor = Color.Red;
                    day2match4box2.BackColor = Color.Green;
                    day2match4box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 5
                if (getTagInfo(day2match5box1, 'p', 0) == deserializedLayoutResults.result.sections[1].groups[4].picks[0].pickids[0])
                {
                    day2match5box1.BackColor = Color.Green;
                    day2match5box2.BackColor = Color.Red;
                    day2match5box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day2match5box2, 'p', 1) == deserializedLayoutResults.result.sections[1].groups[4].picks[0].pickids[0])
                {
                    day2match5box1.BackColor = Color.Red;
                    day2match5box2.BackColor = Color.Green;
                    day2match5box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 6
                if (getTagInfo(day2match6box1, 'p', 0) == deserializedLayoutResults.result.sections[1].groups[5].picks[0].pickids[0])
                {
                    day2match6box1.BackColor = Color.Green;
                    day2match6box2.BackColor = Color.Red;
                    day2match6box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day2match6box2, 'p', 1) == deserializedLayoutResults.result.sections[1].groups[5].picks[0].pickids[0])
                {
                    day2match6box1.BackColor = Color.Red;
                    day2match6box2.BackColor = Color.Green;
                    day2match6box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 7
                if (getTagInfo(day2match7box1, 'p', 0) == deserializedLayoutResults.result.sections[1].groups[6].picks[0].pickids[0])
                {
                    day2match7box1.BackColor = Color.Green;
                    day2match7box2.BackColor = Color.Red;
                    day2match7box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day2match7box2, 'p', 1) == deserializedLayoutResults.result.sections[1].groups[6].picks[0].pickids[0])
                {
                    day2match7box1.BackColor = Color.Red;
                    day2match7box2.BackColor = Color.Green;
                    day2match7box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 8
                if (getTagInfo(day2match8box1, 'p', 0) == deserializedLayoutResults.result.sections[1].groups[7].picks[0].pickids[0])
                {
                    day2match8box1.BackColor = Color.Green;
                    day2match8box2.BackColor = Color.Red;
                    day2match8box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day2match8box2, 'p', 1) == deserializedLayoutResults.result.sections[1].groups[7].picks[0].pickids[0])
                {
                    day2match8box1.BackColor = Color.Red;
                    day2match8box2.BackColor = Color.Green;
                    day2match8box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
            }
        }

        private void generateWinnerLosersDay3()
        {
            for (int i = 0; i < deserializedLayoutResults.result.sections[2].groups.Count; i++)
            {
                //Match 1
                if (getTagInfo(day3match1box1, 'p', 0) == deserializedLayoutResults.result.sections[2].groups[0].picks[0].pickids[0])
                {
                    day3match1box1.BackColor = Color.Green;
                    day3match1box2.BackColor = Color.Red;
                    day3match1box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day3match1box2, 'p', 1) == deserializedLayoutResults.result.sections[2].groups[0].picks[0].pickids[0])
                {
                    day3match1box1.BackColor = Color.Red;
                    day3match1box2.BackColor = Color.Green;
                    day3match1box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 2
                if (getTagInfo(day3match2box1, 'p', 0) == deserializedLayoutResults.result.sections[2].groups[1].picks[0].pickids[0])
                {
                    day3match2box1.BackColor = Color.Green;
                    day3match2box2.BackColor = Color.Red;
                    day3match2box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day3match2box2, 'p', 1) == deserializedLayoutResults.result.sections[2].groups[1].picks[0].pickids[0])
                {
                    day3match2box1.BackColor = Color.Red;
                    day3match2box2.BackColor = Color.Green;
                    day3match2box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 3
                if (getTagInfo(day3match3box1, 'p', 0) == deserializedLayoutResults.result.sections[2].groups[2].picks[0].pickids[0])
                {
                    day3match3box1.BackColor = Color.Green;
                    day3match3box2.BackColor = Color.Red;
                    day3match3box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day3match3box2, 'p', 1) == deserializedLayoutResults.result.sections[2].groups[2].picks[0].pickids[0])
                {
                    day3match3box1.BackColor = Color.Red;
                    day3match3box2.BackColor = Color.Green;
                    day3match3box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 4
                if (getTagInfo(day3match4box1, 'p', 0) == deserializedLayoutResults.result.sections[2].groups[3].picks[0].pickids[0])
                {
                    day3match4box1.BackColor = Color.Green;
                    day3match4box2.BackColor = Color.Red;
                    day3match4box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day3match4box2, 'p', 1) == deserializedLayoutResults.result.sections[2].groups[3].picks[0].pickids[0])
                {
                    day3match4box1.BackColor = Color.Red;
                    day3match4box2.BackColor = Color.Green;
                    day3match4box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
            }
        }

        private void generateWinnerLosersDay4()
        {
            for (int i = 0; i < deserializedLayoutResults.result.sections[3].groups.Count; i++)
            {
                //Match 1
                if (getTagInfo(day4match1box1, 'p', 0) == deserializedLayoutResults.result.sections[3].groups[0].picks[0].pickids[0])
                {
                    day4match1box1.BackColor = Color.Green;
                    day4match1box2.BackColor = Color.Red;
                    day4match1box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day4match1box2, 'p', 1) == deserializedLayoutResults.result.sections[3].groups[0].picks[0].pickids[0])
                {
                    day4match1box1.BackColor = Color.Red;
                    day4match1box2.BackColor = Color.Green;
                    day4match1box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 2
                if (getTagInfo(day4match2box1, 'p', 0) == deserializedLayoutResults.result.sections[3].groups[1].picks[0].pickids[0])
                {
                    day4match2box1.BackColor = Color.Green;
                    day4match2box2.BackColor = Color.Red;
                    day4match2box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day4match2box2, 'p', 1) == deserializedLayoutResults.result.sections[3].groups[1].picks[0].pickids[0])
                {
                    day4match2box1.BackColor = Color.Red;
                    day4match2box2.BackColor = Color.Green;
                    day4match2box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 3
                if (getTagInfo(day4match3box1, 'p', 0) == deserializedLayoutResults.result.sections[3].groups[2].picks[0].pickids[0])
                {
                    day4match3box1.BackColor = Color.Green;
                    day4match3box2.BackColor = Color.Red;
                    day4match3box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day4match3box2, 'p', 1) == deserializedLayoutResults.result.sections[3].groups[2].picks[0].pickids[0])
                {
                    day4match3box1.BackColor = Color.Red;
                    day4match3box2.BackColor = Color.Green;
                    day4match3box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 4
                if (getTagInfo(day4match4box1, 'p', 0) == deserializedLayoutResults.result.sections[3].groups[3].picks[0].pickids[0])
                {
                    day4match4box1.BackColor = Color.Green;
                    day4match4box2.BackColor = Color.Red;
                    day4match4box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day4match4box2, 'p', 1) == deserializedLayoutResults.result.sections[3].groups[3].picks[0].pickids[0])
                {
                    day4match4box1.BackColor = Color.Red;
                    day4match4box2.BackColor = Color.Green;
                    day4match4box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
            }
        }

        private void generateWinnerLosersDay5()
        {
            for (int i = 0; i < deserializedLayoutResults.result.sections[4].groups.Count; i++)
            {
                //Match 1
                if (getTagInfo(day5match1box1, 'p', 0) == deserializedLayoutResults.result.sections[4].groups[0].picks[0].pickids[0])
                {
                    day5match1box1.BackColor = Color.Green;
                    day5match1box2.BackColor = Color.Red;
                    day5match1box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day5match1box2, 'p', 1) == deserializedLayoutResults.result.sections[4].groups[0].picks[0].pickids[0])
                {
                    day5match1box1.BackColor = Color.Red;
                    day5match1box2.BackColor = Color.Green;
                    day5match1box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }

                //Match 2
                if (getTagInfo(day5match2box1, 'p', 0) == deserializedLayoutResults.result.sections[4].groups[1].picks[0].pickids[0])
                {
                    day5match2box1.BackColor = Color.Green;
                    day5match2box2.BackColor = Color.Red;
                    day5match2box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day5match2box2, 'p', 1) == deserializedLayoutResults.result.sections[4].groups[1].picks[0].pickids[0])
                {
                    day5match2box1.BackColor = Color.Red;
                    day5match2box2.BackColor = Color.Green;
                    day5match2box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
            }
        }

        private void generateWinnerLosersDay6()
        {
            for (int i = 0; i < deserializedLayoutResults.result.sections[5].groups.Count; i++)
            {
                //Match 1
                if (getTagInfo(day6match1box1, 'p', 0) == deserializedLayoutResults.result.sections[5].groups[0].picks[0].pickids[0])
                {
                    day6match1box1.BackColor = Color.Green;
                    day6match1box2.BackColor = Color.Red;
                    day6match1box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day6match1box2, 'p', 1) == deserializedLayoutResults.result.sections[5].groups[0].picks[0].pickids[0])
                {
                    day6match1box1.BackColor = Color.Red;
                    day6match1box2.BackColor = Color.Green;
                    day6match1box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
            }
        }

        private void generateWinnerLosersDay6AllStar()
        {
            for (int i = 0; i < deserializedLayoutResults.result.sections[6].groups.Count; i++)
            {
                //Match 1
                if (getTagInfo(day6match1box1, 'p', 0) == deserializedLayoutResults.result.sections[6].groups[0].picks[0].pickids[0])
                {
                    day6match1box1.BackColor = Color.Green;
                    day6match1box2.BackColor = Color.Red;
                    day6match1box1.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
                }
                else if (getTagInfo(day6match1box2, 'p', 1) == deserializedLayoutResults.result.sections[6].groups[0].picks[0].pickids[0])
                {
                    day6match1box1.BackColor = Color.Red;
                    day6match1box2.BackColor = Color.Green;
                    day6match1box2.Font = new Font(DefaultFont.FontFamily, DefaultFont.Size, FontStyle.Bold);
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
                        return objectTag.groupid;
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

        private void day1predictionSubmit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Making picks will lock your stickers that you have chosen. They will be unusable and untradable until the end of the match day. Removing a pick at a later time will not undo the lock.", "Confirm Sticker Lock", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                List<String> postData = new List<String>();
                IEnumerable<GroupBox> day1MatchBoxes = new List<GroupBox>() { day1matchBox1, day1matchBox2, day1matchBox3, day1matchBox4, day1matchBox5, day1matchBox6, day1matchBox7, day1matchBox8 };
                for (int i = 0; i < day1MatchBoxes.Count(); i++)
                {
                    RadioButton checkedRadio;
                    checkedRadio = day1MatchBoxes.ElementAt(i).Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked); //Loops through each ComboBox and identifies the control that is selected
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

                        for (int j = 0; j < availableItems.result.items.Count; j++)
                        {
                            if (Int32.Parse(availableItems.result.items[j].teamid) == teamPick) //Find the associated sticker item id to the team id
                            {
                                itemId = availableItems.result.items[j].itemid;
                            }
                        }
                        if (itemId != string.Empty)
                        {
                            postData.Add(Properties.Settings.Default.tournamentItems + "&sectionid=" + deserializedLayoutResults.result.sections[0].sectionid + "&groupid=" + radioTag.groupid + "&index=0&pickid=" + teamPick + "&itemid=" + itemId);
                        }
                    }
                }
                try
                {
                    foreach (string postInformation in postData)
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.steampowered.com/ICSGOTournaments_730/UploadTournamentPredictions/v1");
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
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show("ERROR HONEY BADGER:\n\nThere was an issue submitting teams:" + exc.ToString());
                }

                updateAppearance();
            }
        }

        private void isSelectionPossiblePickemPrediction(object sender, EventArgs e)
        {
            availableTeamStickers = getInventory.returnAvailableStickersTeams();
            availableItems = getInventory.deserializedInventoryResults;
            RadioButton currentRadio = (RadioButton)sender;
            Layout_Group radioTag = (Layout_Group)currentRadio.Tag;
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
                currentRadio.CheckedChanged -= new System.EventHandler(isSelectionPossiblePickemPrediction); //Remove the event temporarily so it is not triggered again
                currentRadio.Checked = false;
                currentRadio.CheckedChanged += new System.EventHandler(isSelectionPossiblePickemPrediction);
            }
        }

        private void day2predictionSubmit_Click(object sender, EventArgs e) //For proper documentation of this method, check day1predictionSubmit. I will shift all 6 of these into a similar method in a bit, but for now there are slight differences between each one
        {
            if (MessageBox.Show(this, "Making picks will lock your stickers that you have chosen. They will be unusable and untradable until the end of the match day. Removing a pick at a later time will not undo the lock.", "Confirm Sticker Lock", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                List<String> postData = new List<String>();
                IEnumerable<GroupBox> day2MatchBoxes = new List<GroupBox>() { day2matchBox1, day2matchBox2, day2matchBox3, day2matchBox4, day2matchBox5, day2matchBox6, day2matchBox7, day2matchBox8 };
                for (int i = 0; i < day2MatchBoxes.Count(); i++)
                {
                    RadioButton checkedRadio;
                    checkedRadio = day2MatchBoxes.ElementAt(i).Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked);
                    if (checkedRadio != null)
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

                        for (int j = 0; j < availableItems.result.items.Count; j++)
                        {
                            if (Int32.Parse(availableItems.result.items[j].teamid) == teamPick)
                            {
                                itemId = availableItems.result.items[j].itemid;
                            }
                        }
                        if (itemId != string.Empty)
                        {
                            postData.Add(Properties.Settings.Default.tournamentItems + "&sectionid=" + deserializedLayoutResults.result.sections[0].sectionid + "&groupid=" + radioTag.groupid + "&index=0&pickid=" + teamPick + "&itemid=" + itemId);
                        }
                    }
                }
                try
                {
                    foreach (string postInformation in postData)
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.steampowered.com/ICSGOTournaments_730/UploadTournamentPredictions/v1");
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
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Error submitting teams:\n" + exc.ToString());
                }

                updateAppearance();
            }
        }

        private void day3predictionSubmit_Click(object sender, EventArgs e) //For proper documentation of this method, check day1predictionSubmit. I will shift all 6 of these into a similar method in a bit, but for now there are slight differences between each one
        {
            if (MessageBox.Show(this, "Making picks will lock your stickers that you have chosen. They will be unusable and untradable until the end of the match day. Removing a pick at a later time will not undo the lock.", "Confirm Sticker Lock", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                List<String> postData = new List<String>();
                IEnumerable<GroupBox> day3MatchBoxes = new List<GroupBox>() { day3matchBox1, day3matchBox2, day3matchBox3, day3matchBox4 };
                for (int i = 0; i < day3MatchBoxes.Count(); i++)
                {
                    RadioButton checkedRadio;
                    checkedRadio = day3MatchBoxes.ElementAt(i).Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked);
                    if (checkedRadio != null)
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

                        for (int j = 0; j < availableItems.result.items.Count; j++)
                        {
                            if (Int32.Parse(availableItems.result.items[j].teamid) == teamPick)
                            {
                                itemId = availableItems.result.items[j].itemid;
                            }
                        }
                        if (itemId != string.Empty)
                        {
                            postData.Add(Properties.Settings.Default.tournamentItems + "&sectionid=" + deserializedLayoutResults.result.sections[0].sectionid + "&groupid=" + radioTag.groupid + "&index=0&pickid=" + teamPick + "&itemid=" + itemId);
                        }
                    }
                }
                try
                {
                    foreach (string postInformation in postData)
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.steampowered.com/ICSGOTournaments_730/UploadTournamentPredictions/v1");
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
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Error submitting teams:\n" + exc.ToString());
                }

                updateAppearance();
            }
        }

        private void day4predictionSubmit_Click(object sender, EventArgs e) //For proper documentation of this method, check day1predictionSubmit. I will shift all 6 of these into a similar method in a bit, but for now there are slight differences between each one
        {
            if (MessageBox.Show(this, "Making picks will lock your stickers that you have chosen. They will be unusable and untradable until the end of the match day. Removing a pick at a later time will not undo the lock.", "Confirm Sticker Lock", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                List<String> postData = new List<String>();
                IEnumerable<GroupBox> day4MatchBoxes = new List<GroupBox>() { day4matchBox1, day4matchBox2, day4matchBox3, day4matchBox4 };
                for (int i = 0; i < day4MatchBoxes.Count(); i++)
                {
                    RadioButton checkedRadio;
                    checkedRadio = day4MatchBoxes.ElementAt(i).Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked);
                    if (checkedRadio != null)
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

                        for (int j = 0; j < availableItems.result.items.Count; j++)
                        {
                            if (Int32.Parse(availableItems.result.items[j].teamid) == teamPick)
                            {
                                itemId = availableItems.result.items[j].itemid;
                            }
                        }
                        if (itemId != string.Empty)
                        {
                            postData.Add(Properties.Settings.Default.tournamentItems + "&sectionid=" + deserializedLayoutResults.result.sections[0].sectionid + "&groupid=" + radioTag.groupid + "&index=0&pickid=" + teamPick + "&itemid=" + itemId);
                        }
                    }
                }
                try
                {
                    foreach (string postInformation in postData)
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.steampowered.com/ICSGOTournaments_730/UploadTournamentPredictions/v1");
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
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Error submitting teams:\n" + exc.ToString());
                }

                updateAppearance();
            }
        }

        private void day5predictionSubmit_Click(object sender, EventArgs e) //For proper documentation of this method, check day1predictionSubmit. I will shift all 6 of these into a similar method in a bit, but for now there are slight differences between each one
        {
            if (MessageBox.Show(this, "Making picks will lock your stickers that you have chosen. They will be unusable and untradable until the end of the match day. Removing a pick at a later time will not undo the lock.", "Confirm Sticker Lock", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                List<String> postData = new List<String>();
                IEnumerable<GroupBox> day5MatchBoxes = new List<GroupBox>() { day5matchBox1, day5matchBox2 };
                for (int i = 0; i < day5MatchBoxes.Count(); i++)
                {
                    RadioButton checkedRadio;
                    checkedRadio = day5MatchBoxes.ElementAt(i).Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked);
                    if (checkedRadio != null)
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

                        for (int j = 0; j < availableItems.result.items.Count; j++)
                        {
                            if (Int32.Parse(availableItems.result.items[j].teamid) == teamPick)
                            {
                                itemId = availableItems.result.items[j].itemid;
                            }
                        }
                        if (itemId != string.Empty)
                        {
                            postData.Add(Properties.Settings.Default.tournamentItems + "&sectionid=" + deserializedLayoutResults.result.sections[0].sectionid + "&groupid=" + radioTag.groupid + "&index=0&pickid=" + teamPick + "&itemid=" + itemId);
                        }
                    }
                }
                try
                {
                    foreach (string postInformation in postData)
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.steampowered.com/ICSGOTournaments_730/UploadTournamentPredictions/v1");
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
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Error submitting teams:\n" + exc.ToString());
                }

                updateAppearance();
            }
        }

        private void day6predictionSubmit_Click(object sender, EventArgs e) //For proper documentation of this method, check day1predictionSubmit. I will shift all 6 of these into a similar method in a bit, but for now there are slight differences between each one
        {
            if (MessageBox.Show(this, "Making picks will lock your stickers that you have chosen. They will be unusable and untradable until the end of the match day. Removing a pick at a later time will not undo the lock.", "Confirm Sticker Lock", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                List<String> postData = new List<String>();
                IEnumerable<GroupBox> day6MatchBoxes = new List<GroupBox>() { day6matchBox1 };
                for (int i = 0; i < day6MatchBoxes.Count(); i++)
                {
                    RadioButton checkedRadio;
                    checkedRadio = day6MatchBoxes.ElementAt(i).Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked);
                    if (checkedRadio != null)
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

                        for (int j = 0; j < availableItems.result.items.Count; j++)
                        {
                            if (Int32.Parse(availableItems.result.items[j].teamid) == teamPick)
                            {
                                itemId = availableItems.result.items[j].itemid;
                            }
                        }
                        if (itemId != string.Empty)
                        {
                            postData.Add(Properties.Settings.Default.tournamentItems + "&sectionid=" + deserializedLayoutResults.result.sections[0].sectionid + "&groupid=" + radioTag.groupid + "&index=0&pickid=" + teamPick + "&itemid=" + itemId);
                        }
                    }
                }
                try
                {
                    foreach (string postInformation in postData)
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.steampowered.com/ICSGOTournaments_730/UploadTournamentPredictions/v1");
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
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show("Error submitting teams:\n" + exc.ToString());
                }

                updateAppearance();
            }
        }

        private void displayOnlyPlayersWithStickersOwner_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.displayOnlyPlayersWithStickersOwned = displayOnlyPlayersWithStickersOwner.Checked;
            Properties.Settings.Default.Save();
            MessageBox.Show("Updating...");
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
    }
}
