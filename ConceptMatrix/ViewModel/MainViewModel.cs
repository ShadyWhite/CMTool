﻿using ConceptMatrix.Utility;
using ConceptMatrix.Views;
using MaterialDesignThemes.Wpf;
using SaintCoinach;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;

namespace ConceptMatrix.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private static Mediator mediator;

        private static BackgroundWorker worker;
        public Mem MemLib = new Mem();
        public static int gameProcId = 0;
        public static ThreadWriting ThreadTime;
        public static RotationView RotTime;
        public static CharacterDetailsView5 ViewTime5;
        public static CharacterDetailsView4 ViewTime4;
        public static CharacterDetailsView3 ViewTime3;
        public static CharacterDetailsView2 ViewTime2;
        public static CharacterDetailsView ViewTime;
        public static AboutView AboutTime;
        public static MainWindow MainTime;
        private static CharacterDetailsViewModel characterDetails;

		public event PropertyChangedEventHandler PropertyChanged;

		public CharacterDetailsViewModel CharacterDetails { get => characterDetails; set => characterDetails = value; }

        public PackIconKind AOTToggleStatus { get; set; } = PackIconKind.ToggleSwitchOffOutline;
		public Brush ToggleForeground { get; set; } = new SolidColorBrush(Color.FromArgb(0x75, 0xFF, 0xFF, 0xFF));

		public MainViewModel()
        {
            if (!MainWindow.HasRead)
            {
                if (!App.IsValidGamePath(Properties.Settings.Default.GamePath))
                    return;
                ARealmReversed realm = null;
                if (SaveSettings.Default.Language == "en") realm = new ARealmReversed(Properties.Settings.Default.GamePath, SaintCoinach.Ex.Language.English);
                else if (SaveSettings.Default.Language == "ja") realm = new ARealmReversed(Properties.Settings.Default.GamePath, SaintCoinach.Ex.Language.Japanese);
                else if (SaveSettings.Default.Language == "de") realm = new ARealmReversed(Properties.Settings.Default.GamePath, SaintCoinach.Ex.Language.German);
                else if (SaveSettings.Default.Language == "fr") realm = new ARealmReversed(Properties.Settings.Default.GamePath, SaintCoinach.Ex.Language.French);
                else if (SaveSettings.Default.Language == "ko")
                {
                    if (File.Exists(Path.Combine(Properties.Settings.Default.GamePath, "boot", "FFXIV_Boot.exe")))
                    {
                        realm = new ARealmReversed(Properties.Settings.Default.GamePath, SaintCoinach.Ex.Language.Korean);
                    }
                    else realm = new ARealmReversed(Properties.Settings.Default.GamePath, SaintCoinach.Ex.Language.English);
                }
                else if (SaveSettings.Default.Language == "zh")
                {
                    if (File.Exists(Path.Combine(Properties.Settings.Default.GamePath, "FFXIVBoot.exe")))
                    {
                        realm = new ARealmReversed(Properties.Settings.Default.GamePath, SaintCoinach.Ex.Language.ChineseSimplified);
                    }
                    else realm = new ARealmReversed(Properties.Settings.Default.GamePath, SaintCoinach.Ex.Language.English);
                }
                else realm = new ARealmReversed(Properties.Settings.Default.GamePath, SaintCoinach.Ex.Language.English);
                Initialize(realm);
                MainWindow.HasRead = true;
            }
            mediator = new Mediator();
            MemoryManager.Instance.MemLib.OpenProcess(gameProcId);
            LoadSettings();
            worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.WorkerSupportsCancellation = true;
            worker.RunWorkerAsync();
            characterDetails = new CharacterDetailsViewModel(mediator);
            CharacterDetailsViewModel.baseAddr = MemoryManager.Add(MemoryManager.Instance.BaseAddress, "8");
            Task.Delay(40).Wait();
            ThreadTime = new ThreadWriting(); // Thread Writing
        }
        public static void ShutDownStuff()
        {
            worker.CancelAsync();
            ThreadTime.worker.CancelAsync();
            characterDetails = null;
            mediator = null;
            ThreadTime = null;
        }
        private void Initialize(ARealmReversed realm)
        {
            if (!realm.IsCurrentVersion)
            {
                try
                {
                    if (File.Exists("SaintCoinach.History.zip"))
                        File.Delete("SaintCoinach.History.zip");
                    if (SaveSettings.Default.Language == "en") realm = new ARealmReversed(Properties.Settings.Default.GamePath, SaintCoinach.Ex.Language.English);
                    else if (SaveSettings.Default.Language == "ja") realm = new ARealmReversed(Properties.Settings.Default.GamePath, SaintCoinach.Ex.Language.Japanese);
                    else if (SaveSettings.Default.Language == "de") realm = new ARealmReversed(Properties.Settings.Default.GamePath, SaintCoinach.Ex.Language.German);
                    else if (SaveSettings.Default.Language == "fr") realm = new ARealmReversed(Properties.Settings.Default.GamePath, SaintCoinach.Ex.Language.French);
                    else realm = new ARealmReversed(Properties.Settings.Default.GamePath, SaintCoinach.Ex.Language.English);
                }
                catch
                {
                    System.Windows.MessageBox.Show("Unable to delete SaintCoinach.History.zip! Please don't open zip or use it.", "Oh wow!");
                }
            }
            realm.Packs.GetPack(new SaintCoinach.IO.PackIdentifier("exd", SaintCoinach.IO.PackIdentifier.DefaultExpansion, 0)).KeepInMemory = true;
            MainWindow.Realm = realm;
            CharacterDetailsView._exdProvider.RaceList();
            CharacterDetailsView._exdProvider.TribeList();
            CharacterDetailsView._exdProvider.DyeList();
            CharacterDetailsView._exdProvider.MonsterList();
            ExdCsvReader.MonsterX = CharacterDetailsView._exdProvider.Monsters.Values.ToArray();
            for (int i = 0; i < CharacterDetailsView._exdProvider.Dyes.Count; i++)
            {
                ViewTime2.HeadDye.Items.Add(CharacterDetailsView._exdProvider.Dyes[i].Name);
                ViewTime2.ChestBox.Items.Add(CharacterDetailsView._exdProvider.Dyes[i].Name);
                ViewTime2.ArmBox.Items.Add(CharacterDetailsView._exdProvider.Dyes[i].Name);
                ViewTime2.MHBox.Items.Add(CharacterDetailsView._exdProvider.Dyes[i].Name);
                ViewTime2.OHBox.Items.Add(CharacterDetailsView._exdProvider.Dyes[i].Name);
                ViewTime2.LegBox.Items.Add(CharacterDetailsView._exdProvider.Dyes[i].Name);
                ViewTime2.FeetBox.Items.Add(CharacterDetailsView._exdProvider.Dyes[i].Name);
            }
            foreach (ExdCsvReader.Monster xD in ExdCsvReader.MonsterX)
            {
                if (xD.Real == true)
                {
                    ViewTime.SpecialControl.ModelBox.Items.Add(new ExdCsvReader.Monster
                    {
                        Index = Convert.ToInt32(xD.Index),
                        Name = xD.Name.ToString()
                    });
                }
            }
            for (int i = 0; i < CharacterDetailsView._exdProvider.Races.Count; i++)
            {
                ViewTime.RaceBox.Items.Add(CharacterDetailsView._exdProvider.Races[i].Name);
            }
            for (int i = 0; i < CharacterDetailsView._exdProvider.Tribes.Count; i++)
            {
                ViewTime.ClanBox.Items.Add(CharacterDetailsView._exdProvider.Tribes[i].Name);
            }
            var StatusSheet = MainWindow.Realm.GameData.GetSheet<SaintCoinach.Xiv.Status>();
            HashSet<byte> Sets = new HashSet<byte>();
            foreach (SaintCoinach.Xiv.Status status in StatusSheet)
            {
                if(status.Key==0)
                {
                    ViewTime4.StatusEffectBox2.Items.Add(new ComboBoxItem() { Content = "None", Tag = 0 });
                }
                if (Sets.Contains(status.VFX) || status.VFX <= 0) continue;
                Sets.Add(status.VFX);
                string name = status.Name.ToString();
                if (name.Length <= 0) name = "None";
                ViewTime4.StatusEffectBox2.Items.Add(new ComboBoxItem() { Content = name, Tag = status.Key });
            }
            var TitleSheet = MainWindow.Realm.GameData.GetSheet<SaintCoinach.Xiv.Title>();
            foreach (SaintCoinach.Xiv.Title title in TitleSheet)
            {
                string Title = title.Feminine;
                if (Title.Length <= 0) Title = "No Title";
                ViewTime.TitleBox.Items.Add(Title);
            }
        }
       private void LoadSettings()
        {
            // create an xml serializer
            var serializer = new XmlSerializer(typeof(Settings), "");
            // create namespace to remove it for the serializer
            var ns = new XmlSerializerNamespaces();
            // add blank namespaces
            ns.Add("", "");
            using (var reader = new StreamReader(@"./OffsetSettings.xml"))
            {
                try
                {
                    Settings.Instance = (Settings)serializer.Deserialize(reader);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            if (CheckUpdate()==true)
            {
                System.Windows.MessageBox.Show("We successfully updated offsets automatically for you! Please Restart the program or Press Find New Process on the top right of the application if this doesn't work!", "Oh wow!");
            }
        }
        private bool CheckUpdate()
        {
            try
            {
                string xmlStr;
                ServicePointManager.SecurityProtocol = (ServicePointManager.SecurityProtocol & SecurityProtocolType.Ssl3) | (SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12);
                using (var HAH = new WebClient())
                {
                    xmlStr = HAH.DownloadString(@"https://raw.githubusercontent.com/imchillin/CMTool/master/ConceptMatrix/OffsetSettings.xml");
                }
                var serializer = new XmlSerializer(typeof(Settings), "");
                var xmlDoc = new System.Xml.XmlDocument();
                xmlDoc.LoadXml(xmlStr);
                var offset = (Settings)serializer.Deserialize(new StringReader(xmlDoc.InnerXml));
                if (!string.Equals(Settings.Instance.LastUpdated, offset.LastUpdated))
                {
                    File.WriteAllText(@"./OffsetSettings.xml", xmlDoc.InnerXml);
                    Settings.Instance = offset;
                    return true;
                }
                else return false;
            }
            catch
            {
                return false;
            }
        }
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // no fancy tricks here boi
            MemoryManager.Instance.BaseAddress = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.AoBOffset, NumberStyles.HexNumber));
            MemoryManager.Instance.TargetAddress = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.TargetOffset, NumberStyles.HexNumber));
            MemoryManager.Instance.CameraAddress = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.CameraOffset, NumberStyles.HexNumber));
            MemoryManager.Instance.GposeAddress = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.GposeOffset, NumberStyles.HexNumber));
            MemoryManager.Instance.GposeEntityOffset = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.GposeEntityOffset, NumberStyles.HexNumber));
            MemoryManager.Instance.GposeCheckAddress = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.GposeCheckOffset, NumberStyles.HexNumber));
            MemoryManager.Instance.GposeCheck2Address = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.GposeCheck2Offset, NumberStyles.HexNumber));
            MemoryManager.Instance.TimeAddress = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.TimeOffset, NumberStyles.HexNumber));
            MemoryManager.Instance.WeatherAddress = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.WeatherOffset, NumberStyles.HexNumber));
            MemoryManager.Instance.TerritoryAddress = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.TerritoryOffset, NumberStyles.HexNumber));
            MemoryManager.Instance.MusicOffset = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.MusicOffset, NumberStyles.HexNumber));
            MemoryManager.Instance.GposeFilters = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.GposeFilters, NumberStyles.HexNumber));
            MemoryManager.Instance.SkeletonAddress = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.SkeletonOffset, NumberStyles.HexNumber));
            MemoryManager.Instance.SkeletonAddress2 = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.SkeletonOffset2, NumberStyles.HexNumber));
            MemoryManager.Instance.SkeletonAddress3 = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.SkeletonOffset3, NumberStyles.HexNumber));
            MemoryManager.Instance.PhysicsAddress = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.PhysicsOffset, NumberStyles.HexNumber));
            MemoryManager.Instance.PhysicsAddress2 = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.PhysicsOffset2, NumberStyles.HexNumber));
            MemoryManager.Instance.CharacterRenderAddress = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.CharacterRenderOffset, NumberStyles.HexNumber));
            MemoryManager.Instance.CharacterRenderAddress2 = MemoryManager.Instance.GetBaseAddress(int.Parse(Settings.Instance.CharacterRenderOffset2, NumberStyles.HexNumber));
            while (true)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                }
                // sleep for 50 ms
                Thread.Sleep(50);
                // check if our memory manager is set /saving
                if (!MainWindow.CurrentlySaving) mediator.SendWork();
            }
        }

		public void ToggleStatus(bool on)
		{
			ToggleForeground = on ? new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)) : new SolidColorBrush(Color.FromArgb(0x75, 0xFF, 0xFF, 0xFF));
			AOTToggleStatus = on ? PackIconKind.ToggleSwitch : PackIconKind.ToggleSwitchOffOutline;
		}
    }
}
