using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OutlookProfileEditor
{
    public partial class Editor : Form
    {
        private static string OFFICE_VERSION = "15.0";
        private static string SUBKEY_PATH = @"Software\Microsoft\Office\" + OFFICE_VERSION + @"\Outlook";
        private static string GITHUB_REPO = "https://github.com/sjkingo/OutlookProfileEditor";

        public Editor()
        {
            InitializeComponent();
            officeVersionLabel.Text = OFFICE_VERSION;
            PopulateProfiles();
        }

        private void RegistryError()
        {
            MessageBox.Show("Failed to load from the registry at " + SUBKEY_PATH + ": is Office " + OFFICE_VERSION + " installed?", 
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            System.Environment.Exit(1);
        }

        private void ResetForm()
        {
            PopulateProfiles();
            profileName.Text = "";
        }

        private void PopulateProfiles()
        {
            var profiles = Registry.CurrentUser.OpenSubKey(SUBKEY_PATH + @"\Profiles", true);
            if (profiles == null)
            {
                RegistryError();
            }
            profileNameSelector.Items.Clear();
            foreach (string profile in profiles.GetSubKeyNames())
            {
                profileNameSelector.Items.Add(profile);
            }
            profileNameSelector.Text = "Please select a profile...";
        }

        private string GetDefaultProfile()
        {
            var outlook = Registry.CurrentUser.OpenSubKey(SUBKEY_PATH, true);
            if (outlook == null)
            {
                RegistryError();
            }
            return (string)outlook.GetValue("DefaultProfile");
        }

        private void SetDefaultProfile(string profileName)
        {
            var outlook = Registry.CurrentUser.OpenSubKey(SUBKEY_PATH, true);
            if (outlook == null)
            {
                RegistryError();
            }
            outlook.SetValue("DefaultProfile", profileName, RegistryValueKind.String);
            outlook.Close();
        }

        private void profileNameSelector_SelectedValueChanged(object sender, EventArgs e)
        {
            string name = profileNameSelector.SelectedItem.ToString();
            profileName.Text = name;
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            string selectedProfileName = profileNameSelector.SelectedItem.ToString();
            string newProfileName = profileName.Text;
            var outlook = Registry.CurrentUser.OpenSubKey(SUBKEY_PATH, true);

            // Name is different, update the profile subkey by renaming it
            if (!selectedProfileName.Equals(newProfileName))
            {
                var profiles = Registry.CurrentUser.OpenSubKey(SUBKEY_PATH + @"\Profiles", true);
                RenameRegistry.RenameSubKey(profiles, selectedProfileName, newProfileName);

                // must set default to new name
                if (outlook.GetValue("DefaultProfile").Equals(selectedProfileName))
                {
                    outlook.SetValue("DefaultProfile", newProfileName);
                }

                // change selected item and clear the form
                ResetForm();
                profileNameSelector.SelectedValue = newProfileName;
                profileNameSelector.Text = newProfileName;
            }
        }

        private void changelogLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(GITHUB_REPO + @"/blob/master/CHANGELOG.md");
        }

        private void githubLink_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(GITHUB_REPO);
        }
    }
}
