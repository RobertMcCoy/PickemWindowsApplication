using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace PickemTest
{
    public partial class Settings : Form
    {
        MainForm parentForm;

        public Settings(MainForm mf) //This just updates the boxes with the current settings, a very simple constructor. The parentform is passed so values can be updated directly from this form
        {
            InitializeComponent();
            getListOfEventIds(); //This will test each of the sites, if it does not provide a successful connection 
            if (!Properties.Settings.Default.steamID64.Equals("") && !Properties.Settings.Default.steamNameID.Equals(""))
            {
                steamIdBox.Text = Properties.Settings.Default.steamID64;
                steamName.Text = Properties.Settings.Default.steamNameID;
                steamCommunityIdBox.Text = Properties.Settings.Default.steamID;
            }
            comboBox1.Text = Convert.ToString(Properties.Settings.Default.tournamentID);
            if (comboBox1.Text == "9")
            {
                if (!Properties.Settings.Default.authenticationCode9.Equals(""))
                {
                    authCodeBox.Text = Properties.Settings.Default.authenticationCode9;
                }
            }
            else if (comboBox1.Text == "10")
            {
                if (!Properties.Settings.Default.authenticationCode10.Equals(""))
                {
                    authCodeBox.Text = Properties.Settings.Default.authenticationCode10;
                }
            }
            else if (comboBox1.Text == "11")
            {
                if (!Properties.Settings.Default.authenticationCode11.Equals(""))
                {
                    authCodeBox.Text = Properties.Settings.Default.authenticationCode11;
                }
            }
            else if (comboBox1.Text == "12")
            {
                if (!Properties.Settings.Default.authenticationCode12.Equals(""))
                {
                    authCodeBox.Text = Properties.Settings.Default.authenticationCode12;
                }
            }
            else if (comboBox1.Text == "13")
            {
                if (!Properties.Settings.Default.authenticationCode13.Equals(""))
                {
                    authCodeBox.Text = Properties.Settings.Default.authenticationCode13;
                }
            }
            else if (comboBox1.Text == "14")
            {
                if (!Properties.Settings.Default.authenticationCode14.Equals(""))
                {
                    authCodeBox.Text = Properties.Settings.Default.authenticationCode14;
                }
            }
            else if (comboBox1.Text == "15")
            {
                if (!Properties.Settings.Default.authenticationCode15.Equals(""))
                {
                    authCodeBox.Text = Properties.Settings.Default.authenticationCode15;
                }
            }
            parentForm = mf;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            authCodeBox.Text = "";
            selectedEventLbl.Text = "";
            string tournamentLayout = Properties.Settings.Default.tournamentLayout + comboBox1.Text;
            selectedEventLbl.Text = retrieveEventName(tournamentLayout);
            Properties.Settings.Default.tournamentName = selectedEventLbl.Text;

            if (selectedEventLbl.Text.Equals("Error"))
            {
                comboBox1.SelectedItem = Properties.Settings.Default.tournamentID;
            }
            else
            {
                if (comboBox1.SelectedItem.Equals("9"))
                {
                    authCodeBox.Text = Properties.Settings.Default.authenticationCode9;
                }
                else if (comboBox1.SelectedItem.Equals("10"))
                {
                    authCodeBox.Text = Properties.Settings.Default.authenticationCode10;
                }
                else if (comboBox1.SelectedItem.Equals("11"))
                {
                    authCodeBox.Text = Properties.Settings.Default.authenticationCode11;
                }
                else if (comboBox1.SelectedItem.Equals("12"))
                {
                    authCodeBox.Text = Properties.Settings.Default.authenticationCode12;
                }
                else if (comboBox1.SelectedItem.Equals("13"))
                {
                    authCodeBox.Text = Properties.Settings.Default.authenticationCode13;
                }
                else if (comboBox1.SelectedItem.Equals("14"))
                {
                    authCodeBox.Text = Properties.Settings.Default.authenticationCode14;
                }
                else if (comboBox1.SelectedItem.Equals("15"))
                {
                    authCodeBox.Text = Properties.Settings.Default.authenticationCode15;
                }
            }
        }

        private string retrieveEventName(string tournamentLayout) 
        {
            Layout_ResultWrapper deserializedLayoutResults;
            ServicePointManager.DefaultConnectionLimit = 10;
            try
            {
                HttpWebRequest tournamentInfoGET = (HttpWebRequest)HttpWebRequest.Create(tournamentLayout);
                tournamentInfoGET.Method = "GET";
                HttpWebResponse response = (HttpWebResponse)tournamentInfoGET.GetResponse();
                Stream tournamentStream = response.GetResponseStream();
                StreamReader tournamentReader = new StreamReader(tournamentStream);
                StringBuilder sb = new StringBuilder();

                while (tournamentReader.EndOfStream != true)
                {
                    sb.Append(tournamentReader.ReadLine());
                }

                deserializedLayoutResults = JsonConvert.DeserializeObject<Layout_ResultWrapper>(sb.ToString());
                tournamentReader.Close();
                tournamentStream.Close();
                return deserializedLayoutResults.result.name;
            }
            catch (Exception exc)
            {
                MessageBox.Show("ERROR GECKO:\n\nThere was an issue retrieving tournament information in settings: " + exc.ToString());
                return "Error";
            }
        }

        private void saveBtn_Click(object sender, EventArgs e) //Save all settings at once, should probably be broken down into individual change saves, but I forgot that was possible before doing this portion of the program :)
        {
            Properties.Settings.Default.tournamentID = Convert.ToInt32(comboBox1.SelectedItem);
            if (selectedEventLbl.Text.Equals("Error"))
            {
                MessageBox.Show("There is an error with the tournament ID that you have selected. Please try again.");
            }
            else
            {
                if (comboBox1.SelectedItem.Equals("9"))
                {
                    Properties.Settings.Default.authenticationCode9 = authCodeBox.Text;
                    if (!Properties.Settings.Default.steamID64.Equals("") && !Properties.Settings.Default.steamNameID.Equals("") && !Properties.Settings.Default.authenticationCode9.Equals(""))
                    {
                        Properties.Settings.Default.tournamentPredictions = "&event=" + Properties.Settings.Default.tournamentID + "&steamid=" + Properties.Settings.Default.steamID64 + "&steamidkey=" + Properties.Settings.Default.authenticationCode9;
                        Properties.Settings.Default.tournamentPickemPredictions = "&event=" + Properties.Settings.Default.tournamentID + "&steamid=" + Properties.Settings.Default.steamID64 + "&steamidkey=" + Properties.Settings.Default.authenticationCode9;
                    }
                }
                else if (comboBox1.SelectedItem.Equals("10"))
                {
                    Properties.Settings.Default.authenticationCode10 = authCodeBox.Text;
                    if (!Properties.Settings.Default.steamID64.Equals("") && !Properties.Settings.Default.steamNameID.Equals("") && !Properties.Settings.Default.authenticationCode10.Equals(""))
                    {
                        Properties.Settings.Default.tournamentPredictions = "&event=" + Properties.Settings.Default.tournamentID + "&steamid=" + Properties.Settings.Default.steamID64 + "&steamidkey=" + Properties.Settings.Default.authenticationCode10;
                        Properties.Settings.Default.tournamentPickemPredictions = "&event=" + Properties.Settings.Default.tournamentID + "&steamid=" + Properties.Settings.Default.steamID64 + "&steamidkey=" + Properties.Settings.Default.authenticationCode10;
                    }
                }
                else if (comboBox1.SelectedItem.Equals("11"))
                {
                    Properties.Settings.Default.authenticationCode11 = authCodeBox.Text;
                    if (!Properties.Settings.Default.steamID64.Equals("") && !Properties.Settings.Default.steamNameID.Equals("") && !Properties.Settings.Default.authenticationCode11.Equals(""))
                    {
                        Properties.Settings.Default.tournamentPredictions = "&event=" + Properties.Settings.Default.tournamentID + "&steamid=" + Properties.Settings.Default.steamID64 + "&steamidkey=" + Properties.Settings.Default.authenticationCode11;
                        Properties.Settings.Default.tournamentPickemPredictions = "&event=" + Properties.Settings.Default.tournamentID + "&steamid=" + Properties.Settings.Default.steamID64 + "&steamidkey=" + Properties.Settings.Default.authenticationCode11;
                    }
                }
                else if (comboBox1.SelectedItem.Equals("12"))
                {
                    Properties.Settings.Default.authenticationCode12 = authCodeBox.Text;
                    if (!Properties.Settings.Default.steamID64.Equals("") && !Properties.Settings.Default.steamNameID.Equals("") && !Properties.Settings.Default.authenticationCode12.Equals(""))
                    {
                        Properties.Settings.Default.tournamentPredictions = "&event=" + Properties.Settings.Default.tournamentID + "&steamid=" + Properties.Settings.Default.steamID64 + "&steamidkey=" + Properties.Settings.Default.authenticationCode12;
                        Properties.Settings.Default.tournamentPickemPredictions = "&event=" + Properties.Settings.Default.tournamentID + "&steamid=" + Properties.Settings.Default.steamID64 + "&steamidkey=" + Properties.Settings.Default.authenticationCode12;
                    }
                }
                else if (comboBox1.SelectedItem.Equals("13"))
                {
                    Properties.Settings.Default.authenticationCode13 = authCodeBox.Text;
                    if (!Properties.Settings.Default.steamID64.Equals("") && !Properties.Settings.Default.steamNameID.Equals("") && !Properties.Settings.Default.authenticationCode13.Equals(""))
                    {
                        Properties.Settings.Default.tournamentPredictions = "&event=" + Properties.Settings.Default.tournamentID + "&steamid=" + Properties.Settings.Default.steamID64 + "&steamidkey=" + Properties.Settings.Default.authenticationCode13;
                        Properties.Settings.Default.tournamentPickemPredictions = "&event=" + Properties.Settings.Default.tournamentID + "&steamid=" + Properties.Settings.Default.steamID64 + "&steamidkey=" + Properties.Settings.Default.authenticationCode13;
                    }
                }
                else if (comboBox1.SelectedItem.Equals("14"))
                {
                    Properties.Settings.Default.authenticationCode14 = authCodeBox.Text;
                    if (!Properties.Settings.Default.steamID64.Equals("") && !Properties.Settings.Default.steamNameID.Equals("") && !Properties.Settings.Default.authenticationCode14.Equals(""))
                    {
                        Properties.Settings.Default.tournamentPredictions = "&event=" + Properties.Settings.Default.tournamentID + "&steamid=" + Properties.Settings.Default.steamID64 + "&steamidkey=" + Properties.Settings.Default.authenticationCode14;
                        Properties.Settings.Default.tournamentPickemPredictions = "&event=" + Properties.Settings.Default.tournamentID + "&steamid=" + Properties.Settings.Default.steamID64 + "&steamidkey=" + Properties.Settings.Default.authenticationCode14;
                    }
                }
                else if (comboBox1.SelectedItem.Equals("15"))
                {
                    Properties.Settings.Default.authenticationCode15 = authCodeBox.Text;
                    if (!Properties.Settings.Default.steamID64.Equals("") && !Properties.Settings.Default.steamNameID.Equals("") && !Properties.Settings.Default.authenticationCode15.Equals(""))
                    {
                        Properties.Settings.Default.tournamentPredictions = "&event=" + Properties.Settings.Default.tournamentID + "&steamid=" + Properties.Settings.Default.steamID64 + "&steamidkey=" + Properties.Settings.Default.authenticationCode15;
                        Properties.Settings.Default.tournamentPickemPredictions = "&event=" + Properties.Settings.Default.tournamentID + "&steamid=" + Properties.Settings.Default.steamID64 + "&steamidkey=" + Properties.Settings.Default.authenticationCode15;
                    }
                }
            }
            Properties.Settings.Default.Save();
            parentForm.updateAppearance();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) //Provides a link to get the Authentication Code for the individual user
        {
            System.Diagnostics.Process.Start("https://help.steampowered.com/en/wizard/HelpWithGameIssue/?appid=730&issueid=128");
            linkLabel1.LinkVisited = true;
        }

        private void validateBtn_Click(object sender, EventArgs e) //Very convoluted way of getting Steam ID's, Username, and Steam64ID. Could probably be done in a better way, but this way allows a user to enter anything they might know and get a return on their information
        {
            if (!steamCommunityIdBox.Text.Equals("") || steamCommunityIdBox.Text != string.Empty)
            {
                try
                {
                    WebRequest steamIDRequest = WebRequest.Create("http://www.steamcommunity.com/id/" + steamCommunityIdBox.Text + "?xml=1");
                    Stream steamIdStream = steamIDRequest.GetResponse().GetResponseStream();
                    StreamReader steamIdReader = new StreamReader(steamIdStream);
                    string nextLine = string.Empty;
                    while ((nextLine = steamIdReader.ReadLine()) != null)
                    {
                        if (nextLine.Contains("<steamID64>"))
                        {
                            Properties.Settings.Default.steamID64 = nextLine.Substring(nextLine.IndexOf(">") + 1, (nextLine.LastIndexOf("<") - (nextLine.IndexOf(">") + 1)));
                            steamIdBox.Text = Properties.Settings.Default.steamID64;

                        }
                        if (nextLine.Contains("steamID><!"))
                        {
                            Properties.Settings.Default.steamNameID = nextLine.Substring(nextLine.IndexOf("[") + 7, (nextLine.IndexOf("]") - (nextLine.IndexOf("[") + 7)));
                            steamName.Text = Properties.Settings.Default.steamNameID;
                        }
                        if (!steamName.Text.Equals("") && !steamIdBox.Text.Equals(""))
                        {
                            Properties.Settings.Default.steamID = steamCommunityIdBox.Text;
                        }
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show("ERROR LEMUR:\n\nError retrieving Steam information: " + exc.ToString());
                    if (!Properties.Settings.Default.steamID.Equals("") && !Properties.Settings.Default.steamID64.Equals("") && !Properties.Settings.Default.steamNameID.Equals(""))
                    {
                        steamName.Text = "";
                        steamIdBox.Text = "";
                        steamCommunityIdBox.Text = "";
                        steamCommunityIdBox.Focus();
                    }
                }
            }
            else if (!steamIdBox.Text.Equals("") || steamIdBox.Text != string.Empty)
            {
                try
                {
                    WebRequest steamIDRequest = WebRequest.Create("http://www.steamcommunity.com/profiles/" + steamIdBox.Text + "?xml=1"); //Returns XML format of the profiles from the Steam64ID
                    Stream steamIdStream = steamIDRequest.GetResponse().GetResponseStream();
                    StreamReader steamIdReader = new StreamReader(steamIdStream);
                    string nextLine = string.Empty; 
                    while ((nextLine = steamIdReader.ReadLine()) != null)
                    {
                        if (nextLine.Contains("<customURL><!"))
                        {
                            Properties.Settings.Default.steamID = nextLine.Substring(nextLine.IndexOf("[") + 7, (nextLine.IndexOf("]") - (nextLine.IndexOf("[") + 7)));
                            steamCommunityIdBox.Text = Properties.Settings.Default.steamID;

                        }
                        if (nextLine.Contains("steamID><!"))
                        {
                            Properties.Settings.Default.steamNameID = nextLine.Substring(nextLine.IndexOf("[") + 7, (nextLine.IndexOf("]") - (nextLine.IndexOf("[") + 7)));
                            steamName.Text = Properties.Settings.Default.steamNameID;
                        }
                        if (!steamName.Text.Equals("") && !steamIdBox.Text.Equals(""))
                        {
                            Properties.Settings.Default.steamID64 = steamIdBox.Text;
                        }
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show("ERROR KANGRAOO:\n\nError retrieving Steam information: " + exc.ToString());
                    if (!Properties.Settings.Default.steamID.Equals("") && !Properties.Settings.Default.steamID64.Equals("") && !Properties.Settings.Default.steamNameID.Equals(""))
                    {
                        steamName.Text = "";
                        steamIdBox.Text = "";
                        steamCommunityIdBox.Text = "";
                        steamIdBox.Focus();
                    }
                }
            }
        }

        private void getListOfEventIds()
        {
            List<String> comboBoxToAdd = new List<String>();
            for (int i = 9; i <= 12; i++) //Loop through events 9-10, 9 is the first available (Columbus 2016) (12 should last until ~April of 2017)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Properties.Settings.Default.tournamentLayout + i);
                    request.Method = "GET";
                    request.Timeout = 1000;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode != HttpStatusCode.NotFound || response.StatusCode != HttpStatusCode.BadRequest)
                    {
                        comboBoxToAdd.Add(i.ToString()); //If the site returns a success code, add it to the list of selectable events
                    }
                    response.Close();
                }
                catch
                {
                    continue;
                }
            }
            comboBox1.DataSource = comboBoxToAdd;
        }
    }
}
