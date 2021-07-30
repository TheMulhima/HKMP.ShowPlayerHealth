using System;
using System.Collections.Generic;
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
        private const int DarkShade = 3, MediumShade = 3;
        private const float HorizontalSpacing = 2f, VerticalSpacing = 20f;
        private const float width = 300f , height = 100f;
        private const int FontSize = 36;
        
        private static string Player1, Player2; //The 2 players that are gonna be displayed

        private static CanvasText Player1Text, Player2Text;
        private static GameObject canvas;

        private void Update()
        {
            var settings = PlayerHealth.Instance.settings;
            //save data
            if (InputtedKey(settings.Save_Data_To_File))
            {
                PlayerHealth.Instance.Log("Trying to save data");

                var newList = GetPlayerList().Select(player => player.GetName()).ToList();

                var newData = new Settings
                {
                    AllPlayers = newList,
                    DisplayedPlayers = ("", "")
                };

                File.WriteAllText(PlayerHealth.MyFile, JsonConvert.SerializeObject(newData));
            }

            //load data
            if (InputtedKey(settings.Load_Data_From_File))
            {
                var newData = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(PlayerHealth.MyFile));

                var playerNameList = GetPlayerList().Select(player => player.GetName()).ToList();

                var Player1_test = newData.DisplayedPlayers.Item1;
                var Player2_test = newData.DisplayedPlayers.Item2;

                bool isPlayer1Real = false, isPlayer2Real = false;
                
                if(playerNameList.Contains(Player1_test)) isPlayer1Real = true;
                if(playerNameList.Contains(Player2_test)) isPlayer2Real = true;

                //if player given  not in list then text is made blank
                Player1 = isPlayer1Real ? Player1_test : "";
                Player2 = isPlayer2Real ? Player2_test : "";
                
                UpdateNameText();
            }

            //remove text
            if (InputtedKey(settings.Remove_Text))
            {
                RemoveText();
            }
        }

        private static bool InputtedKey(string key) => Input.GetKeyDown((KeyCode) Enum.Parse(typeof(KeyCode), key, true));

        private static List<IPlayer> GetPlayerList()
        {
            IHkmpApi hkmpApi;
            try
            {
                hkmpApi = Hkmp.Hkmp.GetApi();
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return hkmpApi.GetGameManager().GetPlayerManager().GetPlayers();
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
            
            var playerList = GetPlayerList();

            //to make compiler not scream at me saying it might not be initialized;
            int Player1Health = 0, Player2Health = 0;

            foreach (var player in playerList)
            {
                if (player.GetName() == Player1)
                {
                    Player1Health = int.Parse(player.GetHealth().ToString());
                }

                if (player.GetName() == Player2)
                {
                    Player2Health = int.Parse(player.GetHealth().ToString());
                }
            }

            //if player name is blank then looks at local players health
            Player1Health = Player1 == "" ? PlayerData.instance.health : Player1Health;
            Player2Health = Player2 == "" ? PlayerData.instance.health : Player2Health;
            
            float current_spot_left = 0f;
            float current_spot_right = Screen.width - PlayerHealth.TextureDimentions.Item1; //item1 is width
            float amountToMove = PlayerHealth.TextureDimentions.Item1 / 2f + HorizontalSpacing;
            Texture2D tex;

            for (int i = 1; i <= Player1Health; i++)
            {
                tex = PlayerHealth.images[$"left_{FindDrawTex(i)}"];
                GUI.DrawTexture(new Rect(current_spot_left, VerticalSpacing, tex.width, tex.height), tex);
                current_spot_left += amountToMove;
            }

            for (int i = 1; i <= Player2Health; i++)
            {
                tex = PlayerHealth.images[$"right_{FindDrawTex(i)}"];
                GUI.DrawTexture(new Rect(current_spot_right, VerticalSpacing, tex.width, tex.height), tex);
                current_spot_right -= amountToMove;
            }
        }

        private static string FindDrawTex(int i)
        {
            return i switch
            {
                <= DarkShade => "DarkShade",
                <= DarkShade + MediumShade => "MediumShade",
                _ => "LightShade",
            };
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

        //code taken from jngo's multiplayer mod
        private static void MakeNewPanels()
        {
            //make sure no NRE's
            if (canvas != null && Player1Text != null && Player2Text != null) return;
            
            canvas ??= new GameObject("HKMP PlayerHealth Canvas");
            canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(Screen.width, Screen.height);
            DontDestroyOnLoad(canvas);

            float yPos = PlayerHealth.TextureDimentions.Item2 + VerticalSpacing;//item 2 is height

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
