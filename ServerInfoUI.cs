using Oxide.Game.Rust.Cui;
using Oxide.Core;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("ServerInfoUI", "ChatGPT", "1.0.0")]
    [Description("Displays a server info UI")] 
    public class ServerInfoUI : RustPlugin
    {
        private const string UiName = "Dota/info";
        private const string BackgroundName = "Dota/info_bg";

        [ChatCommand("info")]
        private void CmdInfo(BasePlayer player, string command, string[] args)
        {
            ShowUI(player);
        }

        private void Unload()
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                CuiHelper.DestroyUi(player, UiName);
                CuiHelper.DestroyUi(player, BackgroundName);
            }
        }

        private void ShowUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, UiName);
            CuiHelper.DestroyUi(player, BackgroundName);
            var container = new CuiElementContainer();

            container.Add(new CuiElement
            {
                Name = BackgroundName,
                Parent = "Overlay",
                Components =
                {
                    new CuiImageComponent { Color = "0 0 0 0.62" },
                    new CuiRectTransformComponent { AnchorMin = "0 0", AnchorMax = "1 1" }
                }
            });

            container.Add(new CuiElement
            {
                Name = UiName,
                Parent = "Overlay",
                Components = {
                    new CuiImageComponent { Color = "0 0 0 0" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "272 -554.667", OffsetMax = "1008 -165.333" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiImageComponent { Color = "0.7647059 0.7647059 0.7647059 0.5299999713897705" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -40", OffsetMax = "660.667 -0" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiImageComponent { Color = "0.8156863 0.7764706 0.7411765 1" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "660.667 -40", OffsetMax = "736 -0" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiTextComponent { Text = "Главная", Align = TextAnchor.MiddleCenter, Color = "0.8156863 0.7764706 0.7411765 1", FontSize = 23, Font = "Roboto-Bold.ttf" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "172 -80", OffsetMax = "272 -50" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiTextComponent { Text = "Информация", Align = TextAnchor.MiddleCenter, Color = "0.8156863 0.7764706 0.7411765 1", FontSize = 15, Font = "Roboto-Bold.ttf" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "5.333 -32.667", OffsetMax = "111.333 -7.333" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiTextComponent { Text = "ЗАКРЫТЬ", Align = TextAnchor.MiddleCenter, Color = "0 0 0 1", FontSize = 13, Font = "Roboto-Bold.ttf" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "666 -30.667", OffsetMax = "730.667 -9.333" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiTextComponent { Text = "ВЫ ИГРАЕТЕ НА СЕРВЕРЕ ", Align = TextAnchor.MiddleCenter, Color = "0.8156863 0.7764706 0.7411765 1", FontSize = 14, Font = "Roboto-Bold.ttf" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "172 -108.667", OffsetMax = "353.333 -84.667" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiTextComponent { Text = "PUBLIC RUST", Align = TextAnchor.MiddleCenter, Color = "0.8156863 0.7764706 0.7411765 1", FontSize = 19, Font = "Roboto-Bold.ttf" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "360 -105.333", OffsetMax = "489.333 -87.333" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiImageComponent { Color = "0.53333336 0.59607846 0.42745098 1" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "178 -147.333", OffsetMax = "346.667 -132" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiImageComponent { Color = "0.6 0.83137256 0.92156863 1" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "346.667 -147.333", OffsetMax = "362 -132" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiImageComponent { Color = "0.7647059 0.7647059 0.7647059 0.5299999713897705" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "357.333 -147.333", OffsetMax = "726 -132" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiRawImageComponent { Color = "1 1 1 1", Url = "https://figma-alpha-api.s3.us-west-2.amazonaws.com/images/afdf08ef-eeb2-43b5-8f4b-873be702604d" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "4.667 -77.333", OffsetMax = "27.333 -54.667" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiImageComponent { Color = "0.7647059 0.7647059 0.7647059 0.5299999713897705" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -126.667", OffsetMax = "160.667 -90.667" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiImageComponent { Color = "0.7647059 0.7647059 0.7647059 0.5299999713897705" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -83.333", OffsetMax = "160.667 -47.333" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiImageComponent { Color = "0.7647059 0.7647059 0.7647059 0.5299999713897705" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -214", OffsetMax = "160.667 -178" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiImageComponent { Color = "0.7647059 0.7647059 0.7647059 0.5299999713897705" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -257.333", OffsetMax = "160.667 -221.333" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiImageComponent { Color = "0.7647059 0.7647059 0.7647059 0.5299999713897705" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "0 -170.667", OffsetMax = "160.667 -134.667" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiRawImageComponent { Color = "1 1 1 1", Url = "https://figma-alpha-api.s3.us-west-2.amazonaws.com/images/239c2cf3-4447-468b-bf8f-137321831e2c" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "6 -118.667", OffsetMax = "25.333 -99.333" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiRawImageComponent { Color = "1 1 1 1", Url = "https://figma-alpha-api.s3.us-west-2.amazonaws.com/images/ec358f6f-03d5-4cbf-94a3-8c18a41d5461" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "8.667 -203.333", OffsetMax = "22.667 -188.667" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiRawImageComponent { Color = "1 1 1 1", Url = "https://figma-alpha-api.s3.us-west-2.amazonaws.com/images/14570839-3789-4ea1-a745-fff552589947" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "6 -250.667", OffsetMax = "26 -230" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiRawImageComponent { Color = "1 1 1 1", Url = "https://figma-alpha-api.s3.us-west-2.amazonaws.com/images/e954edc1-503e-4ca3-be49-e78ac566d525" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "6 -163.333", OffsetMax = "25.333 -141.333" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiImageComponent { Color = "0.53333336 0.59607846 0.42745098 1" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "179.333 -168", OffsetMax = "183.333 -164" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiImageComponent { Color = "0.6 0.83137256 0.92156863 1" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "338.667 -168", OffsetMax = "342.667 -164" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiTextComponent { Text = "Онлайн: 50", Align = TextAnchor.MiddleCenter, Color = "0.5254902 0.5019608 0.4745098 1", FontSize = 14, Font = "Roboto-Bold.ttf" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "187.333 -175.333", OffsetMax = "266 -158.667" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiTextComponent { Text = "Заходят:3", Align = TextAnchor.MiddleCenter, Color = "0.5254902 0.5019608 0.4745098 1", FontSize = 14, Font = "Roboto-Bold.ttf" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "347.333 -175.333", OffsetMax = "426 -158.667" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiImageComponent { Color = "0.7647059 0.7647059 0.7647059 0.5299999713897705" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "178 -212", OffsetMax = "576 -187.333" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiTextComponent { Text = "Следующий вайп", Align = TextAnchor.MiddleCenter, Color = "0.8156863 0.7764706 0.7411765 1", FontSize = 12, Font = "Roboto-Bold.ttf" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "185.333 -208.667", OffsetMax = "292 -191.333" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiImageComponent { Color = "0.7647059 0.7647059 0.7647059 0.5299999713897705" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "178.667 -242", OffsetMax = "576.667 -217.333" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiTextComponent { Text = "Предыдущий вайп", Align = TextAnchor.MiddleCenter, Color = "0.8156863 0.7764706 0.7411765 1", FontSize = 10, Font = "Roboto-Bold.ttf" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "186 -238.667", OffsetMax = "292.667 -221.333" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiImageComponent { Color = "0.033653848 0 0 0.5299999713897705" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "581.333 -212", OffsetMax = "726 -187.333" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiImageComponent { Color = "0.033653848 0 0 0.5299999713897705" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "581.333 -242", OffsetMax = "726 -217.333" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiTextComponent { Text = "22 июня в 18: 03", Align = TextAnchor.MiddleCenter, Color = "0.8156863 0.7764706 0.7411765 1", FontSize = 13, Font = "Roboto-Bold.ttf" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "602.667 -209.333", OffsetMax = "707.333 -191.333" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiTextComponent { Text = "22 июня в 18: 03", Align = TextAnchor.MiddleCenter, Color = "0.8156863 0.7764706 0.7411765 1", FontSize = 13, Font = "Roboto-Bold.ttf" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "602.667 -238.667", OffsetMax = "707.333 -220.667" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiImageComponent { Color = "0.7647059 0.7647059 0.7647059 0.5299999713897705" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "168 -389.333", OffsetMax = "736 -47.333" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiTextComponent { Text = "Главная", Align = TextAnchor.MiddleCenter, Color = "1 1 1 1", FontSize = 13, Font = "Roboto-Bold.ttf" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "43.333 -73.333", OffsetMax = "94 -58" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiTextComponent { Text = "Главная", Align = TextAnchor.MiddleCenter, Color = "1 1 1 1", FontSize = 13, Font = "Roboto-Bold.ttf" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "43.333 -116.667", OffsetMax = "94 -101.333" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiTextComponent { Text = "Главная", Align = TextAnchor.MiddleCenter, Color = "1 1 1 1", FontSize = 13, Font = "Roboto-Bold.ttf" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "43.333 -160", OffsetMax = "94 -144.667" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiTextComponent { Text = "Главная", Align = TextAnchor.MiddleCenter, Color = "1 1 1 1", FontSize = 13, Font = "Roboto-Bold.ttf" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "43.333 -203.333", OffsetMax = "94 -188" }
                }
            });

            container.Add(new CuiElement
            {
                Parent = UiName,
                Components = {
                    new CuiTextComponent { Text = "Главная", Align = TextAnchor.MiddleCenter, Color = "1 1 1 1", FontSize = 13, Font = "Roboto-Bold.ttf" },
                    new CuiRectTransformComponent { AnchorMin = "0 1", AnchorMax = "0 1", OffsetMin = "43.333 -248", OffsetMax = "94 -232.667" }
                }
            });

            CuiHelper.AddUi(player, container);
        }
    }
}
