using System.Collections.Generic;
using System.Threading;
using System.IO;
using System;
using TrueGearSDK;


namespace MyTrueGear
{
    public class TrueGearMod
    {
        private static TrueGearPlayer _player = null;

        private static ManualResetEvent leftHandFireSprinklerMRE = new ManualResetEvent(false);
        private static ManualResetEvent rightHandFireSprinklerMRE = new ManualResetEvent(false);
        private static ManualResetEvent leftHandAddFlavourMRE = new ManualResetEvent(false);
        private static ManualResetEvent rightHandAddFlavourMRE = new ManualResetEvent(false);


        public void LeftHandFireSprinkler()
        {
            while(true)
            {
                leftHandFireSprinklerMRE.WaitOne();
                _player.SendPlay("LeftHandFireSprinkler");
                Thread.Sleep(110);
            }            
        }

        public void RightHandFireSprinkler()
        {
            while (true)
            {
                rightHandFireSprinklerMRE.WaitOne();
                _player.SendPlay("RightHandFireSprinkler");
                Thread.Sleep(110);
            }
        }

        public void LeftHandAddFlavour()
        {
            while(true)
            {
                leftHandAddFlavourMRE.WaitOne();
                _player.SendPlay("LeftHandAddFlavour");
                Thread.Sleep(80);
            }            
        }

        public void RightHandAddFlavour()
        {
            while (true)
            {
                rightHandAddFlavourMRE.WaitOne();
                _player.SendPlay("RightHandAddFlavour");
                Thread.Sleep(80);
            }
        }

        public TrueGearMod() 
        {
            _player = new TrueGearPlayer("1358140","CookingVR");
            //RegisterFilesFromDisk();
            _player.Start();


            new Thread(new ThreadStart(this.LeftHandFireSprinkler)).Start();
            new Thread(new ThreadStart(this.RightHandFireSprinkler)).Start();
            new Thread(new ThreadStart(this.LeftHandAddFlavour)).Start();
            new Thread(new ThreadStart(this.RightHandAddFlavour)).Start();
        }    

        //private void RegisterFilesFromDisk()
        //{
        //    FileInfo[] files = new DirectoryInfo(".//BepInEx//plugins//TrueGear")   //  (".//BepInEx//plugins//TrueGear")
        //            .GetFiles("*.asset_json", SearchOption.AllDirectories);

        //    for (int i = 0; i < files.Length; i++)
        //    {
        //        string name = files[i].Name;
        //        string fullName = files[i].FullName;
        //        if (name == "." || name == "..")
        //        {
        //            continue;
        //        }
        //        string jsonStr = File.ReadAllText(fullName);
        //        JSONNode jSONNode = JSON.Parse(jsonStr);
        //        EffectObject _curAssetObj = EffectObject.ToObject(jSONNode.AsObject);
        //        string uuidName = Path.GetFileNameWithoutExtension(fullName);
        //        _curAssetObj.uuid = uuidName;
        //        _curAssetObj.name = uuidName;
        //        _player.SetupRegister(uuidName, jsonStr);
        //    }
        //}    

        public void Play(string Event)
        { 
            _player.SendPlay(Event);
        }


        public void StartLeftHandFireSprinkler()
        {
            leftHandFireSprinklerMRE.Set();
        }

        public void StartRightHandFireSprinkler()
        {
            rightHandFireSprinklerMRE.Set();
        }

        public void StopFireSprinkler()
        {
            leftHandFireSprinklerMRE.Reset();
            rightHandFireSprinklerMRE.Reset();
        }

        public void StartLeftHandAddFlavour()
        {
            leftHandAddFlavourMRE.Set();
        }

        public void StopLeftHandAddFlavour()
        {
            leftHandAddFlavourMRE.Reset();
        }

        public void StartRightHandAddFlavour()
        {
            rightHandAddFlavourMRE.Set();
        }

        public void StopRightHandAddFlavour()
        {
            rightHandAddFlavourMRE.Reset();
        }
    }
}
