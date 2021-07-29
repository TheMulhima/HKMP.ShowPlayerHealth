using System;
using System.IO;
using System.Linq;
using Hkmp.Api;
using Modding;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerHealth
{
    public class HealthBars : MonoBehaviour
    {
        private static int DarkShade = 3, MediumShade = 6;
        private static float HorizontalSpacing = 3f;
        private static float VerticalSpacing = 20f;
        
        public static string Player1, Player2; //The 2 players that are gonna be displayed

        private static CanvasText Player1Text;
        private static CanvasText Player2Text;
        private static GameObject canvas;
        private static float width = 300f;
        private static int FontSize = 36;
        private static float height = 100f;

        private void Update()
        {
            //save data
            if (Input.GetKeyDown((KeyCode) Enum.Parse(typeof(KeyCode), PlayerHealth.Instance.settings.Save_Data_To_File,
                true)))
            {
                PlayerHealth.Instance.Log("Trying to save data");
                IHkmpApi hkmpApi;
                try
                {
                    hkmpApi = Hkmp.Hkmp.GetApi();
                    PlayerHealth.Instance.Log("Got API");
                }
                catch (InvalidOperationException)
                {
                    return;
                }

                var gameManager = hkmpApi.GetGameManager();
                var playerManager = gameManager.GetPlayerManager();
                var playerList = playerManager.GetPlayers();

                var newList = playerList.Select(player => player.GetName()).ToList();
                
                #if DEBUG
                newList.Add("Mulhima");
                newList.Add("Mul");
                newList.Add("PvP Master");
                newList.Add("God Slayer");
                newList.Add("Some Random Dude");
                newList.Add("I've ran out of names lol");
                #endif

                var newData = new Settings
                {
                    AllPlayers = newList,
                    DisplayedPlayers = ("", "")
                };

                File.WriteAllText(PlayerHealth.MyFile, JsonConvert.SerializeObject(newData));
            }

            //load data
            if (Input.GetKeyDown((KeyCode) Enum.Parse(typeof(KeyCode),
                PlayerHealth.Instance.settings.Load_Data_From_File, true)))
            {
                IHkmpApi hkmpApi;
                try
                {
                    hkmpApi = Hkmp.Hkmp.GetApi();
                }
                catch (InvalidOperationException)
                {
                    return;
                }

                var newData = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(PlayerHealth.MyFile));

                var playerList = hkmpApi.GetGameManager().GetPlayerManager().GetPlayers();

                var Player1_test = newData.DisplayedPlayers.Item1;
                var Player2_test = newData.DisplayedPlayers.Item2;

                bool isPlayer1Real = false, isPlayer2Real = false;

                foreach (var player in playerList)
                {
                    if (player.GetName() == Player1)
                    {
                        isPlayer1Real = true;
                    }

                    if (player.GetName() == Player2)
                    {
                        isPlayer2Real = true;
                    }
                }

                if (isPlayer1Real && isPlayer2Real)
                {
                    Player1 = Player1_test;
                    Player2 = Player2_test;
                }
#if DEBUG
                else
                {
                    Player1 = Player1_test;
                    Player2 = Player2_test;
                }
#endif

                UpdateNameText();
            }

            //remove text
            if (Input.GetKeyDown(
                (KeyCode) Enum.Parse(typeof(KeyCode), PlayerHealth.Instance.settings.Remove_Text, true)))
            {
                RemoveText();
            }
        }

        public static void RemoveText()
        {
            Player1 = Player2 = null; //removed hpbar
            Player1Text.UpdateText("");
            Player2Text.UpdateText("");
        }

        public void OnGUI()
        {
            if (Player1 == null && Player2 == null) return;
            

            IHkmpApi hkmpApi;
            try
            {
                hkmpApi = Hkmp.Hkmp.GetApi();
            }
            catch (InvalidOperationException)
            {
                PlayerHealth.Instance.Log("NO HKMP");
                return;
            }

            var playerList = hkmpApi.GetGameManager().GetPlayerManager().GetPlayers();

            //to make compiler not scream at me saying it might not be initialized;
            int Player1Health = 0, Player2Health = 0;

            foreach (var player in playerList)
            {
                if (player.GetName() == Player1)
                {
                    Player1Health = Int32.Parse(player.GetHealth().ToString());
                }

                if (player.GetName() == Player2)
                {
                    Player2Health = Int32.Parse(player.GetHealth().ToString());
                }
            }
#if DEBUG
            Player1Health = Player2Health = PlayerData.instance.health;
#endif

            float current_spot_left = 0f;
            float current_spot_right = Screen.width - PlayerHealth.images["right_1"].width;
            Texture2D tex;

            for (int i = 1; i <= Player1Health; i++)
            {
                int draw = i switch
                {
                    var x when (x >= 1 && x <= DarkShade) => 1,
                    var x when (x >= DarkShade && x <= MediumShade) => 2,
                    _ => 3,
                };
                tex = PlayerHealth.images[$"left_{draw}"];
                GUI.DrawTexture(new Rect(current_spot_left, VerticalSpacing, tex.width, tex.height), tex);
                current_spot_left += tex.width / 2 + HorizontalSpacing;
            }

            for (int i = 1; i <= Player2Health; i++)
            {
                int draw = i switch
                {
                    var x when (x >= 1 && x <= DarkShade) => 1,
                    var x when (x >= DarkShade && x <= MediumShade) => 2,
                    _ => 3,
                };
                tex = PlayerHealth.images[$"right_{draw}"];
                GUI.DrawTexture(new Rect(current_spot_right, VerticalSpacing, tex.width, tex.height), tex);
                current_spot_right -= tex.width / 2 + HorizontalSpacing;
            }
        }

        private void UpdateNameText()
        {
            //make sure it doesnt display anything before stuff is adjusted on file
            //also helps disable the text
            if (Player1 == null && Player2 == null) return;
            
            MakeNewPanels();
            
            Player1Text.UpdateText(Player1);
            Player2Text.UpdateText(Player2);
        }

        public static void MakeNewPanels()
        {
            //make sure no NRE's
            //code taken from jngo's multiplayer mod
            if (canvas != null && Player1Text != null && Player2Text != null) return;
            
            canvas ??= new GameObject("HKMP PlayerHealth Canvas");
            canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(Screen.width, Screen.height);
            DontDestroyOnLoad(canvas);

            float yPos = PlayerHealth.images["right_1"].height + VerticalSpacing;

            Player1Text ??= new CanvasText(
                canvas,
                new Vector2(20f, yPos),
                new Vector2(width, height),
                CanvasUtil.TrajanBold,
                "",
                FontSize,
                alignment: TextAnchor.UpperLeft);

            Player2Text ??= new CanvasText(
                canvas,
                new Vector2(Screen.width - (width + 20f), yPos),
                new Vector2(width, height),
                CanvasUtil.TrajanBold,
                "",
                FontSize,
                alignment: TextAnchor.UpperRight);
        }
    }
}
