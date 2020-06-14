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
                aMax = ".7 .6",
                aMin = ".1 .4",
                horizontal = ".4 .4",
                vertical = ".4 .4"
            };

        }   

        public class CUIPeace{
            
            public string panelName;
            public string color;
            public double aMaxh;
            public double aMinh;
            public double aMaxv;
            public double aMinv;
            
            public bool cursor = false;
            
            public string command;
            public TextAnchor align = TextAnchor.MiddleCenter;
            
            public string horizontal
            { 
                set
                {
                    double x1,x2,outx1,outx2;
                    string[] coord = value.Split(' ');
                    Double.TryParse(coord[0], out x1);
                    Double.TryParse(coord[1], out x2);

                    ToGeneral( x1, x2, aMinh, aMaxh, out outx1, out outx2);

                    aMinh = outx1;
                    aMaxh = outx2;
                } 
                get 
                {
                    string res = aMinh.ToString("G", CultureInfo.InvariantCulture) + " " + aMinv.ToString("G", CultureInfo.InvariantCulture);

                    return res;
                } 
            }

            public string vertical
            {                 
                set
                {
                    double y1,y2,outy1,outy2;
                    string[] coord = value.Split(' ');
                    Double.TryParse(coord[0], out y1);
                    Double.TryParse(coord[1], out y2);

                    ToGeneral( y1, y2, aMinv, aMaxv, out outy1, out outy2);

                    aMinv = outy1;
                    aMaxv = outy2;
                } 
                get { 

                    string res = aMaxh.ToString("G", CultureInfo.InvariantCulture) + " " + aMaxv.ToString("G", CultureInfo.InvariantCulture);
                    
                    return res; 
                
                }  
            }

            public string aMax
            { 
                set
                {
                    double y1,y2;
                    string[] coord = value.Split(' ');
                    Double.TryParse(coord[0], out y1);
                    Double.TryParse(coord[1], out y2);

                    aMaxh = y1;
                    aMaxv = y2;
                } 
                get 
                {
                    string res = aMaxh.ToString("G", CultureInfo.InvariantCulture) + " " + aMaxv.ToString("G", CultureInfo.InvariantCulture);

                    return res;
                } 
            }

            public string aMin
            { 
                set
                {
                    double y1,y2;
                    string[] coord = value.Split(' ');
                    Double.TryParse(coord[0], out y1);
                    Double.TryParse(coord[1], out y2);

                    aMinh = y1;
                    aMinv = y2;
                } 
                get 
                { 
                    
                    string res = aMinh.ToString("G", CultureInfo.InvariantCulture) + " " + aMinv.ToString("G", CultureInfo.InvariantCulture);

                    return res;
                } 
            }

            static public void ToGeneral(double x1, double x2, double y1, double y2, out double outx1, out double outx2){
                string res;
                double temp1, temp2, width;
                
                width = 1 - (y2+y1);
                temp1 = y1 + (x1*width);
                temp2 = y2 - (x2*width);

                outx1 = temp1;
                outx2 = temp2;
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

            static public void CreatePanel(CUIPeace panel, ref CuiElementContainer container)
            {   

                container.Add(new CuiPanel
                {
                    Image = { Color = panel.color, Material = material },
                    RectTransform = { AnchorMin = panel.horizontal, AnchorMax = panel.vertical },
                    CursorEnabled = panel.cursor 
                },
                panel.panelName);

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

            public string aMinInfo = ".151 .365";
            public string aMaxInfo = ".36 .85";

            public string aMinTp = ".151 .21";
            public string aMaxTp = ".36 .35";

            public string aMinBody = ".37 .21";
            public string aMaxBody = ".84 .85";

            public UIMenu(BasePlayer player){
                ui_name = "menu";
                target = player;

                buildOverlay();
                buildInfo();
 
                //CUIBuild.CreateLabel(ref maincont, "menu", ".8 .8 .5 1", "string text", 14, ".2 .4", ".5 .5");
                //
                //CUIBuild.CreateButton(ref maincont, "menu", ".8 .8 .5 1", "button", 14, ".45 .45", ".5 .5", "chat.say command",TextAnchor.UpperLeft);
                //CUIBuild.LoadImage(ref maincont, "menu", "http://i.imgur.com/xxQnE1R.png", ".45 .45", ".65 .65");
    
                CuiHelper.AddUi(target, maincont);   
    
                //Debug.Log(maincont.ToString() );  

            } 

            public void buildOverlay(){

                CUIBuild.setMaterial("assets/content/ui/uibackgroundblur-ingamemenu.mat");
                maincont = CUIBuild.CreateContainer("menu", ".1 .1 .1 .5", "0 0", "1 1", true);
                CUIBuild.setMaterial("default"); 

                var panel = new CUIPeace{
                    panelName = ui_name, 
                    color = UIColors["black_alpha"],
                    aMax = aMaxInfo,
                    aMin = aMinInfo
                };

                CUIBuild.CreatePanel(panel, ref maincont);
                
                panel.aMin = aMinTp;
                panel.aMax = aMaxTp;

                CUIBuild.CreatePanel(panel, ref maincont);

                panel.aMin = aMinBody;
                panel.aMax = aMaxBody; 

                CUIBuild.CreatePanel(panel, ref maincont);
 
            }

            public void buildInfo(){ 

                var panel = new CUIPeace{ 
                    panelName = ui_name,
                    color = UIColors["black"],
                    aMax = ".7 .7",
                    aMin = ".1 .1",
                    horizontal = ".1 .7",
                    vertical = ".1 .7"
                };  

                Debug.Log(panel.aMax);
                Debug.Log(panel.aMin);

                CUIBuild.CreatePanel(panel, ref maincont);
                //CUIBuild.CreatePanel(ref maincont, ui_name,  UIColors["black"], ".151 .365", ".36 .41");
                //CUIBuild.CreatePanel(ref maincont, ui_name,  UIColors["orange"], ".151 .41", ".36 .5");
                //CUIBuild.LoadImage(ref maincont, ui_name, "http://www.rigormortis.be/wp-content/uploads/rust-icon-512.png", ".194 .53", ".33 .78");
                //CUIBuild.CreateLabel(ref maincont, ui_name, UIColors["white_pink"], target.ToString(), 18,  ".153 .47", ".36 .5");


            }

        }

    }
    
}