﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Modding;
using UnityEngine;
using Newtonsoft.Json;

namespace PlayerHealth
{
    public class PlayerHealth:Mod
    {
        public override string GetVersion() => "v1.0.3";

        internal static PlayerHealth Instance;

        private static readonly string MyPath = Application.persistentDataPath + "/HKMP Player Health/";
        public static readonly string MyFile = MyPath + "config.json";

        internal static Dictionary<string, Texture2D> images = new Dictionary<string, Texture2D>();

        public GlobalModSettings settings = new GlobalModSettings();
        public override ModSettings GlobalSettings 
        {
            get => settings;
            set => settings = (GlobalModSettings) value;
        }

        public override void Initialize()
        {
            Instance ??= this;

            //make sure file has some data atleast
            var dummydata = new Settings
            {
                AllPlayers = {""},
                DisplayedPlayers = ("","")
            };

            if (!Directory.Exists(MyPath)) Directory.CreateDirectory(MyPath);
            
            if (!File.Exists(MyFile)) File.WriteAllText(MyFile,JsonConvert.SerializeObject(dummydata));
            
            string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (string res in resourceNames)
            {
                if (!res.StartsWith("PlayerHealth.Resources.")) continue;
                try
                {
                    Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(res);
                    byte[] buffer = new byte[imageStream.Length];
                    imageStream.Read(buffer, 0, buffer.Length);

                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(buffer.ToArray());
                        
                    //make it 2/3rds the original size
                    tex = Resize(tex, 81,132);
                    string[] split = res.Split('.');
                    string internalName = split[split.Length - 2];
                    
                    images.Add(internalName, tex);

                    Log("Loaded image: " + internalName);
                }
                catch (Exception e)
                {
                    Log("Failed to load image: " + res + "\n" + e.ToString());
                }
            }
            
            //the main update function thats going to be used for this mod to work
            GameManager.instance.gameObject.AddComponent<HealthBars>();

            ModHooks.Instance.SavegameLoadHook += _ => AddMyComponent();
            ModHooks.Instance.NewGameHook += AddMyComponent;
            On.QuitToMenu.Start += DeleteText;
        }

        private IEnumerator DeleteText(On.QuitToMenu.orig_Start orig, QuitToMenu self)
        {
            //hpbar gtes auto removed when gamemanager gets yeeted but this doesnt
            HealthBars.RemoveText();
            yield return orig(self);
        }

        private void AddMyComponent()
        {
            var MyComponent = GameManager.instance.gameObject.GetComponent<HealthBars>();
            if (MyComponent == null) GameManager.instance.gameObject.AddComponent<HealthBars>();
        }

        private static Texture2D Resize(Texture2D texture2D,int targetX, int targetY)
        {
            RenderTexture rt = new RenderTexture(targetX, targetY, 24);
            RenderTexture.active = rt;
            Graphics.Blit(texture2D,rt);
            Texture2D result = new Texture2D(targetX, targetY);
            result.ReadPixels(new Rect(0,0,targetX,targetY),0,0);
            result.Apply();
            return result;
        }
    }
}