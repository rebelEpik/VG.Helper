using System.Globalization;
using System.Text.Json;
using System;
using System.Windows.Forms.VisualStyles;
using VG.Helper.Data;
using System.Resources;

namespace VG.Helper
{

    public partial class MainWindow : Form
    {
        private FileSystemWatcher watcher;
        public string SavePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\LocalLow\Bat Roost Games\VanguardGalaxy\Saves";
        public MainWindow()
        {
            InitializeComponent();
            var doc = JsonDocument.Parse(Utilities.Utilities.ExtractJsonFromSave(GetCurrentSave(SavePath)));

            watcher = new FileSystemWatcher(SavePath, "*.save")
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };

            watcher.Changed += OnSaveFileChanged;
            watcher.Created += OnSaveFileChanged;
            watcher.EnableRaisingEvents = true;

            closeToolStripMenuItem.Click += (s, e) => this.BeginInvoke(new MethodInvoker(() => Application.Exit()));


            updateGUI(doc);
        }

        private void OnSaveFileChanged(object sender, FileSystemEventArgs e)
        {
            var doc = JsonDocument.Parse(Utilities.Utilities.ExtractJsonFromSave(GetCurrentSave(SavePath)));
            this.Invoke(new Action(() => updateGUI(doc)));
        }

        private void updateGUI(JsonDocument document)
        {
            toolStripStatusLabel1.Text = "Loading commander...";
            Commander commander = JsonSerializer.Deserialize<Commander>(document.RootElement.GetProperty("Player").GetProperty("commander").GetRawText());
            commander.credits = document.RootElement.GetProperty("Player").GetProperty("credits").GetString();
            toolStripStatusLabel1.Text = "Commander loaded";

            List<FactionData> factions = JsonSerializer.Deserialize<List<FactionData>>(document.RootElement.GetProperty("Player").GetProperty("factionData").GetRawText());
            List<Spaceship> spaceships = new List<Spaceship>();
            var ships = document.RootElement.GetProperty("Player").GetProperty("spaceShips").EnumerateArray();

            foreach (var ship in ships)
            {
                Spaceship spaceship = new Spaceship
                {
                    guid = Guid.Parse(ship.GetProperty("guid").ToString()),
                    shipClass = ship.GetProperty("shipClass").ToString(),
                    currentHullHP = ship.GetProperty("currentHullHP").GetRawText(),
                    currentArmorHP = ship.GetProperty("currentArmorHP").GetRawText(),
                    currentShieldHP = ship.GetProperty("currentShieldHP").GetRawText(),
                    maxHullHP = ship.GetProperty("maxHullHP").GetRawText(),
                    maxArmorHP = ship.GetProperty("maxArmorHP").GetRawText(),
                    maxShieldHP = ship.GetProperty("maxShieldHP").GetRawText(),
                    traveling = ship.GetProperty("travelling").GetBoolean(),
                    travelSpeed = ship.GetProperty("travelSpeed").GetRawText(),
                    cargoCapacity = ship.GetProperty("cargo").GetProperty("capacity").GetRawText(),
                };
                spaceships.Add(spaceship);

                if (spaceship.guid.ToString().Replace("{", "").Replace("}", "") == document.RootElement.GetProperty("Player").GetProperty("currentSpaceShip").ToString())
                {
                    label21.Text = spaceship.shipClass;
                    if (spaceship.currentShieldHP != null)
                    {
                        shieldProgressBar.Value = (int)(Convert.ToDouble(spaceship.currentShieldHP) / Convert.ToDouble(spaceship.maxShieldHP) * 100);
                        shieldProgressBar.CustomText = Convert.ToDouble(spaceship.currentShieldHP).ToString("N0") + " / " + (Convert.ToDouble(spaceship.maxShieldHP)).ToString("N0");
                    }
                    if (spaceship.currentArmorHP != null)
                    {
                        armorProgressBar.Value = (int)(Convert.ToDouble(spaceship.currentArmorHP) / Convert.ToDouble(spaceship.maxArmorHP) * 100);
                        armorProgressBar.CustomText = Convert.ToDouble(spaceship.currentArmorHP).ToString("N0") + " / " + (Convert.ToDouble(spaceship.maxArmorHP)).ToString("N0");
                    }
                    if(spaceship.currentHullHP != null)
                    {
                        hullProgressBar.Value = (int)(Convert.ToDouble(spaceship.currentHullHP) / Convert.ToDouble(spaceship.maxHullHP) * 100);
                        hullProgressBar.CustomText = Convert.ToDouble(spaceship.currentHullHP).ToString("N0") + " / " + (Convert.ToDouble(spaceship.maxHullHP)).ToString("N0");
                    }



                }

            }
            var autopilotStats = document.RootElement.GetProperty("Player").GetProperty("autopilotSessionStats");


            
            foreach (var stats in autopilotStats.EnumerateArray()
                                   .OrderByDescending(s => s.GetProperty("startTime").GetDouble()))
            {
                AutopilotStats apStats = new AutopilotStats()
                {
                    shipName = stats.GetProperty("shipName").GetString(),
                    startTime = stats.GetProperty("startTime").GetDouble(),
                    endTime = stats.GetProperty("endTime").GetDouble()
                };

                foreach (var stat in stats.GetProperty("stats").EnumerateObject())
                {
                    apStats.stats.Add(new Stats
                    {
                        statName = stat.Name,
                        statValue = stat.Value.GetRawText()
                    });
                }

                // Format duration
                string duration = TimeSpan.FromSeconds(apStats.endTime - apStats.startTime).ToString(@"hh\:mm\:ss");

                // Visible text: shipName + duration
                string nodeText = $"{apStats.shipName} {duration}";

                // Use startTime as a unique key (store in Tag)
                bool exists = apStatsTreeView.Nodes
                    .Cast<TreeNode>()
                    .Any(n => n.Tag is double tag && tag == apStats.startTime);

                if (!exists)
                {
                    TreeNode sessionNode = new TreeNode(nodeText) { Tag = apStats.startTime };

                    foreach (var stat in apStats.stats)
                    {
                        sessionNode.Nodes.Add($"{stat.statName}: {stat.statValue}");
                    }

                    // Find insertion index (keep descending order)
                    int insertIndex = 0;
                    foreach (TreeNode n in apStatsTreeView.Nodes)
                    {
                        if ((double)n.Tag >= apStats.startTime)
                        {
                            insertIndex++;
                        }
                        else break;
                    }

                    apStatsTreeView.Nodes.Insert(insertIndex, sessionNode);
                }


            }


            var currentAPStats = document.RootElement.GetProperty("Player").GetProperty("currentAutopilotSessionStats");
            if (currentAPStats.ValueKind != JsonValueKind.Null)
            {
                string shipName = currentAPStats.GetProperty("shipName").GetString();
                double startTime = currentAPStats.GetProperty("startTime").GetDouble();

                var placeholderNode = currentAPStatsTreeView.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == "No session started");
                if (placeholderNode != null)
                {
                    currentAPStatsTreeView.Nodes.Remove(placeholderNode);
                }



                TreeNode existingNode = currentAPStatsTreeView.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Tag is double tag && tag == startTime);
                if (existingNode == null)
                {
                    // Add new root node
                    TreeNode rootNodeCurrentStats = currentAPStatsTreeView.Nodes.Add(shipName);
                    rootNodeCurrentStats.Tag = startTime;

                    foreach (var stat in currentAPStats.GetProperty("stats").EnumerateObject())
                    {
                        rootNodeCurrentStats.Nodes.Add($"{stat.Name}: {stat.Value.GetRawText()}");
                    }
                }
                else
                {
                    // Update existing node’s children
                    existingNode.Nodes.Clear();
                    foreach (var stat in currentAPStats.GetProperty("stats").EnumerateObject())
                    {
                        existingNode.Nodes.Add($"{stat.Name}: {stat.Value.GetRawText()}");
                    }
                }

            }
            else
            {
                // Show placeholder if no session
                currentAPStatsTreeView.Nodes.Clear();
                currentAPStatsTreeView.Nodes.Add("No session started");
            }
            currentAPStatsTreeView.ExpandAll();


            var homeStation = document.RootElement.GetProperty("Player").GetProperty("homeStation").GetRawText();
            var sectors = document.RootElement.GetProperty("Player").GetProperty("map").GetProperty("sectors");

            foreach (var sector in sectors.EnumerateArray())
            {
                if (sector.TryGetProperty("systems", out var systems))
                {
                    foreach (var system in systems.EnumerateArray())
                    {
                        if(system.GetProperty("guid").GetRawText() == document.RootElement.GetProperty("Player").GetProperty("currentSystem").GetRawText())
                        {
                            currentSystemDisplayLabel.Text = system.GetProperty("name").ToString();
                            sectorLabel.Text = sector.GetProperty("name").ToString();
                        }
                        if (system.TryGetProperty("pointsOfInterest", out var poInterests))
                        {
                            foreach (var poi in poInterests.EnumerateArray())
                            {
                                if (poi.GetProperty("guid").GetRawText() == homeStation.ToString())
                                {
                                    homeStationLabel.Text = poi.GetProperty("name").ToString();
                                }
                            }
                        }
                    }
                }


            }
            //rep
            foreach (var faction in factions)
            {
                if (faction.Faction1 == "Player" || faction.Faction2 == "Player")
                {
                    commander.reputation.Add(faction);
                }
            }


            var materials = document.RootElement.GetProperty("Player").GetProperty("refinedStorage");
            var refStore = materials.EnumerateArray();
            var refinedStorage = refStore.ToArray();

            RefinedStorage refined = new RefinedStorage();
            refined.titanium = refinedStorage[0].ToString();
            refined.oxide = refinedStorage[1].ToString();
            refined.silicon = refinedStorage[2].ToString();
            refined.tungsten = refinedStorage[3].ToString();
            refined.carbon = refinedStorage[4].ToString();
            refined.iridium = refinedStorage[5].ToString();
            refined.platinum = refinedStorage[6].ToString();
            refined.astatine = refinedStorage[7].ToString();

            nameLabel.Text = commander.Name + " (" + commander.callsign + ")";
            levelLabel.Text = commander.level.ToString();
            xpLabel.Text = commander.experience.ToString("N0", CultureInfo.InvariantCulture);
            bonusSPLabel.Text = commander.bonusSkillPoints.ToString();
            creditsLabel.Text = Convert.ToInt32(commander.credits).ToString("N0", CultureInfo.InvariantCulture);


            poRankLabel.Text = document.RootElement.GetProperty("Player").GetProperty("patrolRank").GetRawText();
            boRankLabel.Text = document.RootElement.GetProperty("Player").GetProperty("bountyRank").GetRawText();

            titaniumLabel.Text = Math.Round(Convert.ToDecimal(refined.titanium)).ToString();
            oxideLabel.Text = Math.Round(Convert.ToDecimal(refined.oxide)).ToString();
            siliconLabel.Text = Math.Round(Convert.ToDecimal(refined.silicon)).ToString();
            tungstenLabel.Text = Math.Round(Convert.ToDecimal(refined.tungsten)).ToString();
            carbonLabel.Text = Math.Round(Convert.ToDecimal(refined.carbon)).ToString();
            iridiumLabel.Text = Math.Round(Convert.ToDecimal(refined.iridium)).ToString();
            platinumLabel.Text = Math.Round(Convert.ToDecimal(refined.platinum)).ToString();
            astatineLabel.Text = Math.Round(Convert.ToDecimal(refined.astatine)).ToString();

            LoadImageDynamic(commander.icon, pictureBox1);

            listBox1.Items.Clear();
            foreach (var faction in commander.reputation)
            {
                listBox1.Items.Add(faction); // calls faction.ToString()
            }


            //Autopilot Section
            pHomeStationLabel.Text = document.RootElement.GetProperty("Player").GetProperty("autopilotSettings").GetProperty("prioritizeHomestation").GetRawText();
            rMissionsLabel.Text = document.RootElement.GetProperty("Player").GetProperty("autopilotSettings").GetProperty("runMissions").GetRawText();
            pMissionsLabel.Text = document.RootElement.GetProperty("Player").GetProperty("autopilotSettings").GetProperty("preferMissions").GetRawText();
            mJumpsLabel.Text = document.RootElement.GetProperty("Player").GetProperty("autopilotSettings").GetProperty("maxJumps").GetRawText();
        }


        private void AddJsonElement(TreeNode parentNode, JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        TreeNode childNode = new TreeNode(property.Name);
                        parentNode.Nodes.Add(childNode);
                        AddJsonElement(childNode, property.Value);
                    }
                    break;

                case JsonValueKind.Array:
                    int index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        TreeNode childNode = new TreeNode($"[{index}]");
                        parentNode.Nodes.Add(childNode);
                        AddJsonElement(childNode, item);
                        index++;
                    }
                    break;

                case JsonValueKind.String:
                    parentNode.Nodes.Add(new TreeNode($"\"{element.GetString()}\""));
                    break;

                case JsonValueKind.Number:
                    parentNode.Nodes.Add(new TreeNode(element.GetRawText()));
                    break;

                case JsonValueKind.True:
                case JsonValueKind.False:
                    parentNode.Nodes.Add(new TreeNode(element.GetRawText()));
                    break;

                case JsonValueKind.Null:
                    parentNode.Nodes.Add(new TreeNode("null"));
                    break;
            }
        }

        public void LoadImageDynamic(string resourceName, PictureBox pictureBox)
        {
            // Get the resource manager for your project's Resources
            var rm = Properties.Resources.ResourceManager;

            // Lookup the resource by string name
            object obj = rm.GetObject(resourceName);

            if (obj is Image img)
            {
                pictureBox.Image = img;
                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage; // optional
            }
            else
            {
                throw new ArgumentException($"Resource '{resourceName}' not found or not an image.");
            }
        }



        private string GetCurrentSave(string savePath)
        {
            if (string.IsNullOrWhiteSpace(savePath))
                throw new ArgumentException("Save path cannot be null or empty.", nameof(savePath));

            if (!Directory.Exists(savePath))
                throw new DirectoryNotFoundException($"Save directory not found: {savePath}");

            // Get all .save files in the directory
            var files = Directory.GetFiles(savePath, "*.save");
            if (files.Length == 0)
                return null; // No save files found

            // Find the most recently created or modified file
            string latestFile = null;
            DateTime latestTime = DateTime.MinValue;

            foreach (var file in files)
            {
                DateTime writeTime = File.GetLastWriteTime(file);
                DateTime createTime = File.GetCreationTime(file);
                DateTime candidateTime = writeTime > createTime ? writeTime : createTime;

                if (candidateTime > latestTime)
                {
                    latestTime = candidateTime;
                    latestFile = file;
                }
            }

            return latestFile;
        }

    }
}
