using System;
using UnityEngine;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Game.Rust;
using Newtonsoft.Json;
using Oxide.Game.Rust.Cui;
using System.Collections.Generic;
using System.Globalization;


namespace Oxide.Plugins
{
    [Info("Unit Server[UI]", "Mixfair", "1.1")]
    [Description("UserInteface class for building cui")]
    class UIBase : RustPlugin
    {

        

        private void Init() 
        { 
            
        }

        void Unload()
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                foreach(var ui_inst in CUIBuild.getItems()){
                    CuiHelper.DestroyUi(player, ui_inst);
                }
            } 
        }

        [ChatCommand("ui_build")]
        private void initPanel(BasePlayer player, string cmd, string[] args){
            
            UIMenu menu = new UIMenu(player);
  
            var peace = new CUIPeace{ 
                panelName = "Menu",
                horizontal = ".1 .2"
            };

        }   

        public class CUIPeace{
            public string panelName;
            public string color;

            public string horizontal
            { 
                set
                {
                    double x1,y1;
                    string[] coord = value.Split(' ');
                    Double.TryParse(coord[0], out x1);
                    Double.TryParse(coord[1], out y1);
                    
                } 
                get { return "string"; } 
            }
            public string vertical{ set; get; }
            public bool cursor = false;

            static public string ToGeneral(double x1, double x2, double y1, double y2){
                string res;
                double temp1, temp2, width;
                
                width = 1 - (y2+y1);
                temp1 = y1 + (x1*width);
                temp2 = y2 - (x2*width);

                res = temp1.ToString("G", CultureInfo.InvariantCulture) + " " + temp2.ToString("G", CultureInfo.InvariantCulture);
                
                return res;
            }

        }

        public class CUIBuild {
            
            public static List<string> ui_data = new List<string>();
            public static string material = "Assets/Icons/IconMaterial.mat";
            public static string font = "robotocondensed-regular.ttf";

            public static void addItem(string key){ 
                string val;
                if (!ui_data.Exists(e => e.EndsWith(key)))
                    ui_data.Add(key);
            }

            public static List<string> getItems(){
                return ui_data;
            }

            public static void setMaterial(string mat){
                material = mat;
                if (mat=="default") material = "Assets/Icons/IconMaterial.mat";
            }

            static public CuiElementContainer CreateContainer(string panelName, string color = null, string aMin = null, string aMax = null, bool cursor = false){
                CuiElementContainer Element = new CuiElementContainer();

                if (String.IsNullOrEmpty(color)) return Element; 
                CuiPanel Container = new CuiPanel
                {
                    Image = {Color = color, Material = material},
                    RectTransform = {AnchorMin = aMin, AnchorMax = aMax},
                    CursorEnabled = cursor
                };

                Element.Add(Container, "Hud", panelName); 
 
                addItem(panelName);

                return Element;
            }

            static public void CreatePanel(ref CuiElementContainer container, string panelName, string color, string aMin, string aMax, bool cursor = false)
            {   

                container.Add(new CuiPanel
                {
                    Image = { Color = color, Material = material },
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                    CursorEnabled = cursor 
                },
                panelName);

            } 

            static public void CreateLabel(ref CuiElementContainer container, string panel, string color, string text, int size, string aMin, string aMax, TextAnchor align = TextAnchor.MiddleCenter)
            {
                container.Add(new CuiLabel
                {
                    Text = {Font = font, Color = color, FontSize = size, Align = align, FadeIn = 1.0f, Text = text },
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax }
                },
                panel);
            }

            static public void CreateButton(ref CuiElementContainer container, string panel, string color, string text, int size, string aMin, string aMax, string command, TextAnchor align = TextAnchor.MiddleCenter)
            {
                container.Add(new CuiButton
                {
                    Button = { Color = color, Command = command, FadeIn = 1.0f },
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                    Text = { Font = font, Text = text, FontSize = size, Align = align },
                    
                },
                panel);
            }

            static public void LoadImage(ref CuiElementContainer container, string panel, string img, string aMin, string aMax)
            {
                if (img.StartsWith("http") || img.StartsWith("www"))
                {
                    container.Add(new CuiElement
                    {
                        Parent = panel,
                        Components =
                    {
                        new CuiRawImageComponent {Url = img},
                        new CuiRectTransformComponent {AnchorMin = aMin, AnchorMax = aMax }
                    }
                    });
                }
                else
                    container.Add(new CuiElement
                    {
                        Parent = panel,
                        Components =
                    {
                        new CuiRawImageComponent {Png = img },
                        new CuiRectTransformComponent {AnchorMin = aMin, AnchorMax = aMax }
                    }
                    });
            }

            


        }

        public class UIMenu : UIBase {
            
            BasePlayer target;
            CuiElementContainer maincont;
            string ui_name;
            private static Dictionary<string, string> UIColors = new Dictionary<string, string>
            {
                {"black_alpha", ".05 .05 .05 .9" },
                {"black", ".15 .15 .15 .95" },
                {"orange", ".81 .27 .19 .95" },
                {"white_pink", ".964 .92 .88 1" }
                
            };

            public UIMenu(BasePlayer player){
                ui_name = "menu";
                target = player;

                buildOverlay();
                buildInfo();
 
                CUIBuild.CreateLabel(ref maincont, "menu", ".8 .8 .5 1", "string text", 14, ".2 .4", ".5 .5");
                
                CUIBuild.CreateButton(ref maincont, "menu", ".8 .8 .5 1", "button", 14, ".45 .45", ".5 .5", "chat.say command",TextAnchor.UpperLeft);
                CUIBuild.LoadImage(ref maincont, "menu", "http://i.imgur.com/xxQnE1R.png", ".45 .45", ".65 .65");
    
                CuiHelper.AddUi(target, maincont);   
    
                //Debug.Log(maincont.ToString() );  

            } 

            public void buildOverlay(){
                
                CUIBuild.setMaterial("assets/content/ui/uibackgroundblur-ingamemenu.mat");
                maincont = CUIBuild.CreateContainer("menu", ".1 .1 .1 .5", "0 0", "1 1", true);
                CUIBuild.setMaterial("default"); 
                CUIBuild.CreatePanel(ref maincont, ui_name, UIColors["black_alpha"], ".151 .365", ".36 .85");
                CUIBuild.CreatePanel(ref maincont, ui_name, UIColors["black_alpha"], ".151 .21", ".36 .35");
                CUIBuild.CreatePanel(ref maincont, ui_name, UIColors["black_alpha"], ".37 .21", ".84 .85");
 
            }

            public void buildInfo(){
                

                CUIBuild.CreatePanel(ref maincont, ui_name,  UIColors["black"], ".151 .81", ".36 .85");
                CUIBuild.CreatePanel(ref maincont, ui_name,  UIColors["black"], ".151 .365", ".36 .41");
                CUIBuild.CreatePanel(ref maincont, ui_name,  UIColors["orange"], ".151 .41", ".36 .5");
                CUIBuild.LoadImage(ref maincont, ui_name, "http://www.rigormortis.be/wp-content/uploads/rust-icon-512.png", ".194 .53", ".33 .78");
                CUIBuild.CreateLabel(ref maincont, ui_name, UIColors["white_pink"], target.ToString(), 18,  ".153 .47", ".36 .5");


            }

        }

    }
    
}