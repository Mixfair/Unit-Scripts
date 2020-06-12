using System;
using UnityEngine;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Game.Rust;
using Newtonsoft.Json;
using Oxide.Game.Rust.Cui;
using System.Collections.Generic;


namespace Oxide.Plugins
{
    [Info("Unit Server[UI]", "Mixfair", "1.0")]
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
            //CUIBuild ui = new CUIBuild("menu");
            CUIBuild.setMaterial("assets/content/ui/uibackgroundblur-ingamemenu.mat");
            CuiElementContainer cont = CUIBuild.CreateContainer("menu", ".1 .1 .1 .5", "0 0", "1 1", true);
            //CUIBuild.setMaterial("default"); 
            CUIBuild.CreatePanel(ref cont, "menu",  ".5 .5 .5 1", ".2 .2", ".6 .6");
            
            CuiHelper.AddUi(player, cont);  
 
            Debug.Log(cont.ToString() );  
            foreach(var el in CUIBuild.ui_data){ 
                    Debug.Log(el);
            } 
        }  

        public class CUIBuild {
            
            public static List<string> ui_data = new List<string>();
            public static string material = "Assets/Icons/IconMaterial.mat";

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
                Debug.Log(material);
                container.Add(new CuiPanel
                {
                    Image = { Color = color, Material = material },
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                    CursorEnabled = cursor 
                },
                panelName);

            } 

        }


        public class UI
        {
            static public CuiElementContainer CreateElementContainer(string panelName, string color, string aMin, string aMax, bool cursor = false)
            {
                var NewElement = new CuiElementContainer()
            {
                {
                    new CuiPanel
                    {
                        Image = {Color = color},
                        RectTransform = {AnchorMin = aMin, AnchorMax = aMax},
                        CursorEnabled = cursor
                    },
                    new CuiElement().Parent,
                    panelName
                }
            };
                return NewElement;
            }
            
            static public CuiElementContainer CreateOverlayContainer(string panelName, string color, string aMin, string aMax, bool cursor = false)
            {
                var NewElement = new CuiElementContainer()
            {
                {
                    new CuiPanel
                    {
                        Image = {Color = color},
                        RectTransform = {AnchorMin = aMin, AnchorMax = aMax},
                        CursorEnabled = cursor
                    },
                    new CuiElement().Parent = "Overlay",
                    panelName
                }
            };
                return NewElement;
            }

            static public void CreatePanel(ref CuiElementContainer container, string panel, string color, string aMin, string aMax, bool cursor = false)
            {   

                container.Add(new CuiPanel
                {
                    Image = { Color = color, Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat" },
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                    CursorEnabled = cursor
                },
                panel);

            } 
            static public void CreateLabel(ref CuiElementContainer container, string panel, string color, string text, int size, string aMin, string aMax, TextAnchor align = TextAnchor.MiddleCenter)
            {
                container.Add(new CuiLabel
                {
                    Text = {Font = "robotocondensed-regular.ttf", Color = color, FontSize = size, Align = align, FadeIn = 1.0f, Text = text },
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
                    Text = { Font = "robotocondensed-regular.ttf", Text = text, FontSize = size, Align = align },
                    
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

            static public void CreateTextOutline(ref CuiElementContainer element, string panel, string colorText, string colorOutline, string text, int size, string aMin, string aMax, TextAnchor align = TextAnchor.MiddleCenter)
            {
                element.Add(new CuiElement
                {
                    Parent = panel,
                    Components =
                    {
                        new CuiTextComponent{Font = "robotocondensed-regular.ttf", Color = colorText, FontSize = size, Align = align, Text = text },
                        new CuiOutlineComponent {Distance = "1 1", Color = colorOutline},
                        new CuiRectTransformComponent {AnchorMax = aMax, AnchorMin = aMin }
                    }
                });
            }
        }
    }
    
}