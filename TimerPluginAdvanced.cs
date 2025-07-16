using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using UnityEngine;
using Oxide.Game.Rust.Cui;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("Advanced Timer Plugin", "YourName", "1.1.0")]
    [Description("Advanced timer UI plugin with multiple themes and sound effects")]
    public class TimerPluginAdvanced : RustPlugin
    {
        private Timer countdownTimer;
        private Timer animationTimer;
        private int currentSeconds = 30;
        private bool isEventActive = false;
        private const string UI_PANEL_NAME = "AdvancedTimerPanel";
        private Dictionary<ulong, float> playerAnimationStates = new Dictionary<ulong, float>();
        private Dictionary<ulong, int> playerLastNumbers = new Dictionary<ulong, int>();
        private Dictionary<ulong, string> playerThemes = new Dictionary<ulong, string>();

        #region Configuration
        private Configuration config;

        class Configuration
        {
            [JsonProperty("Default Timer Seconds")]
            public int DefaultTimerSeconds = 30;

            [JsonProperty("Max Timer Seconds")]
            public int MaxTimerSeconds = 3600;

            [JsonProperty("Animation Speed")]
            public float AnimationSpeed = 0.08f;

            [JsonProperty("Animation Interval")]
            public float AnimationInterval = 0.03f;

            [JsonProperty("Enable Sound Effects")]
            public bool EnableSoundEffects = true;

            [JsonProperty("Enable Particle Effects")]
            public bool EnableParticleEffects = false;

            [JsonProperty("UI Themes")]
            public Dictionary<string, UITheme> Themes = new Dictionary<string, UITheme>
            {
                ["default"] = new UITheme
                {
                    Name = "Default",
                    TextColor = "1 0.8 0 1",
                    BackgroundColor = "0.05 0.05 0.05 0.95",
                    BorderColor = "1 0.8 0 1",
                    TitleFontSize = 12,
                    NumberFontSize = 28
                },
                ["dark"] = new UITheme
                {
                    Name = "Dark",
                    TextColor = "0.8 0.8 0.8 1",
                    BackgroundColor = "0.02 0.02 0.02 0.98",
                    BorderColor = "0.5 0.5 0.5 1",
                    TitleFontSize = 12,
                    NumberFontSize = 28
                },
                ["neon"] = new UITheme
                {
                    Name = "Neon",
                    TextColor = "0 1 1 1",
                    BackgroundColor = "0.1 0.1 0.2 0.9",
                    BorderColor = "0 1 1 1",
                    TitleFontSize = 12,
                    NumberFontSize = 28
                }
            };

            [JsonProperty("Messages")]
            public Dictionary<string, string> Messages = new Dictionary<string, string>
            {
                ["TimerStarted"] = "Таймер запущен! Ивент начнется через {0} секунд.",
                ["TimerStopped"] = "Таймер остановлен!",
                ["TimerReset"] = "Таймер сброшен!",
                ["EventStarted"] = "🎉 Ивент начался! 🎉",
                ["NoPermission"] = "У вас нет прав для использования этой команды!",
                ["TimerAlreadyRunning"] = "Таймер уже запущен!",
                ["InvalidTime"] = "Неверное значение времени! Используйте число от 1 до 3600.",
                ["UsageSetTime"] = "Использование: /settime <секунды>",
                ["ThemeChanged"] = "Тема изменена на: {0}"
            };
        }

        class UITheme
        {
            [JsonProperty("Name")]
            public string Name { get; set; }

            [JsonProperty("Text Color")]
            public string TextColor { get; set; }

            [JsonProperty("Background Color")]
            public string BackgroundColor { get; set; }

            [JsonProperty("Border Color")]
            public string BorderColor { get; set; }

            [JsonProperty("Title Font Size")]
            public int TitleFontSize { get; set; }

            [JsonProperty("Number Font Size")]
            public int NumberFontSize { get; set; }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                config = Config.ReadObject<Configuration>();
                if (config == null) throw new Exception();
                SaveConfig();
            }
            catch
            {
                LoadDefaultConfig();
            }
        }

        protected override void LoadDefaultConfig()
        {
            config = new Configuration();
        }

        protected override void SaveConfig() => Config.WriteObject(config);
        #endregion

        void Init()
        {
            // Регистрируем команды
            cmd.AddChatCommand("starttimer", this, "CmdStartTimer");
            cmd.AddChatCommand("stoptimer", this, "CmdStopTimer");
            cmd.AddChatCommand("resettimer", this, "CmdResetTimer");
            cmd.AddChatCommand("settime", this, "CmdSetTime");
            cmd.AddChatCommand("timerhelp", this, "CmdHelp");
            cmd.AddChatCommand("theme", this, "CmdTheme");
            cmd.AddChatCommand("themes", this, "CmdThemes");
        }

        void OnServerInitialized()
        {
            currentSeconds = config.DefaultTimerSeconds;
            Puts("Advanced Timer Plugin загружен!");
            Puts("Команды: /starttimer, /stoptimer, /resettimer, /settime <секунды>, /theme <название>, /themes, /timerhelp");
        }

        void Unload()
        {
            if (countdownTimer != null)
            {
                countdownTimer.Destroy();
            }
            
            if (animationTimer != null)
            {
                animationTimer.Destroy();
            }
            
            foreach (var player in BasePlayer.activePlayerList)
            {
                DestroyTimerUI(player);
            }
            
            playerAnimationStates.Clear();
            playerLastNumbers.Clear();
            playerThemes.Clear();
        }

        [ChatCommand("starttimer")]
        void CmdStartTimer(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) 
            {
                player.ChatMessage(config.Messages["NoPermission"]);
                return;
            }

            if (isEventActive)
            {
                player.ChatMessage(config.Messages["TimerAlreadyRunning"]);
                return;
            }

            StartTimer();
            player.ChatMessage(string.Format(config.Messages["TimerStarted"], currentSeconds));
        }

        [ChatCommand("stoptimer")]
        void CmdStopTimer(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) 
            {
                player.ChatMessage(config.Messages["NoPermission"]);
                return;
            }

            StopTimer();
            player.ChatMessage(config.Messages["TimerStopped"]);
        }

        [ChatCommand("resettimer")]
        void CmdResetTimer(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) 
            {
                player.ChatMessage(config.Messages["NoPermission"]);
                return;
            }

            ResetTimer();
            player.ChatMessage(config.Messages["TimerReset"]);
        }

        [ChatCommand("settime")]
        void CmdSetTime(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) 
            {
                player.ChatMessage(config.Messages["NoPermission"]);
                return;
            }

            if (args.Length == 0)
            {
                player.ChatMessage(config.Messages["UsageSetTime"]);
                return;
            }

            if (int.TryParse(args[0], out int newTime) && newTime > 0 && newTime <= config.MaxTimerSeconds)
            {
                currentSeconds = newTime;
                player.ChatMessage("Время установлено на " + newTime + " секунд.");
            }
            else
            {
                player.ChatMessage(config.Messages["InvalidTime"]);
            }
        }

        [ChatCommand("theme")]
        void CmdTheme(BasePlayer player, string command, string[] args)
        {
            if (args.Length == 0)
            {
                string currentTheme = playerThemes.ContainsKey(player.userID) ? playerThemes[player.userID] : "default";
                player.ChatMessage("Текущая тема: " + config.Themes[currentTheme].Name);
                return;
            }

            string themeName = args[0].ToLower();
            if (config.Themes.ContainsKey(themeName))
            {
                playerThemes[player.userID] = themeName;
                if (isEventActive)
                {
                    CreateTimerUI(player);
                }
                player.ChatMessage(string.Format(config.Messages["ThemeChanged"], config.Themes[themeName].Name));
            }
            else
            {
                player.ChatMessage("Доступные темы: " + string.Join(", ", config.Themes.Keys));
            }
        }

        [ChatCommand("themes")]
        void CmdThemes(BasePlayer player, string command, string[] args)
        {
            player.ChatMessage("=== Доступные темы ===");
            foreach (var theme in config.Themes)
            {
                player.ChatMessage(theme.Key + " - " + theme.Value.Name);
            }
        }

        [ChatCommand("timerhelp")]
        void CmdHelp(BasePlayer player, string command, string[] args)
        {
            player.ChatMessage("=== Команды таймера ===");
            player.ChatMessage("/starttimer - Запустить таймер");
            player.ChatMessage("/stoptimer - Остановить таймер");
            player.ChatMessage("/resettimer - Сбросить таймер");
            player.ChatMessage("/settime <секунды> - Установить время");
            player.ChatMessage("/theme <название> - Изменить тему");
            player.ChatMessage("/themes - Показать доступные темы");
            player.ChatMessage("/timerhelp - Показать эту справку");
        }

        void StartTimer()
        {
            isEventActive = true;
            
            // Показываем UI всем игрокам
            foreach (var player in BasePlayer.activePlayerList)
            {
                CreateTimerUI(player);
                playerAnimationStates[player.userID] = 0f;
                playerLastNumbers[player.userID] = currentSeconds;
            }

            // Запускаем таймер
            countdownTimer = timer.Every(1f, () =>
            {
                currentSeconds--;
                
                if (currentSeconds <= 0)
                {
                    StopTimer();
                    BroadcastToAll(config.Messages["EventStarted"]);
                    
                    // Звуковой эффект при завершении
                    if (config.EnableSoundEffects)
                    {
                        foreach (var player in BasePlayer.activePlayerList)
                        {
                            player.Command("play", "assets/prefabs/misc/easter/easteregg.prefab");
                        }
                    }
                    return;
                }

                // Звуковой эффект для последних 5 секунд
                if (config.EnableSoundEffects && currentSeconds <= 5)
                {
                    foreach (var player in BasePlayer.activePlayerList)
                    {
                        player.Command("play", "assets/prefabs/misc/easter/easteregg.prefab");
                    }
                }

                // Запускаем анимацию прокрутки
                StartScrollAnimation();
            });

            // Запускаем анимацию для начального числа
            StartScrollAnimation();
        }

        void StartScrollAnimation()
        {
            if (animationTimer != null)
            {
                animationTimer.Destroy();
            }

            // Сбрасываем состояние анимации для всех игроков
            foreach (var player in BasePlayer.activePlayerList)
            {
                playerAnimationStates[player.userID] = 0f;
            }

            // Запускаем анимацию прокрутки
            animationTimer = timer.Every(config.AnimationInterval, () =>
            {
                bool animationComplete = true;
                
                foreach (var player in BasePlayer.activePlayerList)
                {
                    if (player == null || !player.IsConnected) continue;

                    float animationState = playerAnimationStates[player.userID];
                    animationState += config.AnimationSpeed;
                    
                    if (animationState >= 1f)
                    {
                        animationState = 1f;
                        UpdateTimerDisplay(player);
                    }
                    else
                    {
                        animationComplete = false;
                    }
                    
                    playerAnimationStates[player.userID] = animationState;
                    UpdateScrollAnimation(player, animationState);
                }

                if (animationComplete)
                {
                    animationTimer.Destroy();
                    animationTimer = null;
                }
            });
        }

        void UpdateScrollAnimation(BasePlayer player, float progress)
        {
            int lastNumber = playerLastNumbers.ContainsKey(player.userID) ? playerLastNumbers[player.userID] : currentSeconds + 1;
            string themeName = playerThemes.ContainsKey(player.userID) ? playerThemes[player.userID] : "default";
            var theme = config.Themes[themeName];
            
            // Создаем эффект прокрутки вниз с плавным переходом
            float startY = 1.2f;
            float endY = 0f;
            float currentY = Mathf.Lerp(startY, endY, progress);
            
            // Удаляем старые анимированные элементы
            CuiHelper.DestroyUi(player, "ScrollingNumber");
            CuiHelper.DestroyUi(player, "OldNumber");
            
            var container = new CuiElementContainer();
            
            // Старое число (уходит вверх с затуханием)
            if (progress < 0.6f)
            {
                float oldY = Mathf.Lerp(0f, -0.8f, progress * 1.67f);
                float alpha = Mathf.Lerp(1f, 0f, progress * 1.67f);
                container.Add(new CuiLabel
                {
                    Text = { Text = lastNumber.ToString(), FontSize = theme.NumberFontSize, Align = TextAnchor.MiddleCenter, Color = $"{theme.TextColor.Split(' ')[0]} {theme.TextColor.Split(' ')[1]} {theme.TextColor.Split(' ')[2]} {alpha}" },
                    RectTransform = { AnchorMin = "0 " + oldY, AnchorMax = "1 " + (oldY + 1f) }
                }, "NumberContainer", "OldNumber");
            }
            
            // Новое число (приходит снизу)
            float newAlpha = Mathf.Lerp(0f, 1f, Mathf.Clamp01(progress * 2f));
            container.Add(new CuiLabel
            {
                Text = { Text = currentSeconds.ToString(), FontSize = theme.NumberFontSize, Align = TextAnchor.MiddleCenter, Color = $"{theme.TextColor.Split(' ')[0]} {theme.TextColor.Split(' ')[1]} {theme.TextColor.Split(' ')[2]} {newAlpha}" },
                RectTransform = { AnchorMin = "0 " + currentY, AnchorMax = "1 " + (currentY + 1f) }
            }, "NumberContainer", "ScrollingNumber");

            CuiHelper.AddUi(player, container);
        }

        void UpdateTimerDisplay(BasePlayer player)
        {
            // Финальное обновление без анимации
            CuiHelper.DestroyUi(player, "ScrollingNumber");
            CuiHelper.DestroyUi(player, "OldNumber");
            
            string themeName = playerThemes.ContainsKey(player.userID) ? playerThemes[player.userID] : "default";
            var theme = config.Themes[themeName];
            
            var container = new CuiElementContainer();
            container.Add(new CuiLabel
            {
                Text = { Text = currentSeconds.ToString(), FontSize = theme.NumberFontSize, Align = TextAnchor.MiddleCenter, Color = theme.TextColor },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
            }, "NumberContainer", "CurrentNumber");

            CuiHelper.AddUi(player, container);
            
            // Обновляем последнее число для игрока
            playerLastNumbers[player.userID] = currentSeconds;
        }

        void StopTimer()
        {
            isEventActive = false;
            
            if (countdownTimer != null)
            {
                countdownTimer.Destroy();
                countdownTimer = null;
            }
            
            if (animationTimer != null)
            {
                animationTimer.Destroy();
                animationTimer = null;
            }

            foreach (var player in BasePlayer.activePlayerList)
            {
                DestroyTimerUI(player);
            }
            
            playerAnimationStates.Clear();
            playerLastNumbers.Clear();
        }

        void ResetTimer()
        {
            StopTimer();
            currentSeconds = config.DefaultTimerSeconds;
        }

        void CreateTimerUI(BasePlayer player)
        {
            string themeName = playerThemes.ContainsKey(player.userID) ? playerThemes[player.userID] : "default";
            var theme = config.Themes[themeName];
            
            var container = new CuiElementContainer();
            
            // Основная панель с градиентом и тенью
            container.Add(new CuiPanel
            {
                Image = { Color = theme.BackgroundColor, Material = "assets/content/ui/uibackgroundblur-ingame.mat" },
                RectTransform = { AnchorMin = "0.5 0.92", AnchorMax = "0.5 0.98" },
                CursorEnabled = false
            }, "Overlay", UI_PANEL_NAME);

            // Верхняя граница с градиентом
            container.Add(new CuiPanel
            {
                Image = { Color = theme.BorderColor },
                RectTransform = { AnchorMin = "0 0.95", AnchorMax = "1 1" }
            }, UI_PANEL_NAME);

            // Нижняя граница
            container.Add(new CuiPanel
            {
                Image = { Color = $"{theme.BorderColor.Split(' ')[0]} {theme.BorderColor.Split(' ')[1]} {theme.BorderColor.Split(' ')[2]} 0.3" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.02" }
            }, UI_PANEL_NAME);

            // Текст "Ивент начнется через"
            container.Add(new CuiLabel
            {
                Text = { Text = "Ивент начнется через", FontSize = theme.TitleFontSize, Align = TextAnchor.MiddleCenter, Color = $"{theme.TextColor.Split(' ')[0]} {theme.TextColor.Split(' ')[1]} {theme.TextColor.Split(' ')[2]} 0.9" },
                RectTransform = { AnchorMin = "0 0.75", AnchorMax = "1 0.95" }
            }, UI_PANEL_NAME);

            // Контейнер для анимированного числа
            container.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.75" },
                CursorEnabled = false
            }, UI_PANEL_NAME, "NumberContainer");

            // Начальное число
            container.Add(new CuiLabel
            {
                Text = { Text = currentSeconds.ToString(), FontSize = theme.NumberFontSize, Align = TextAnchor.MiddleCenter, Color = theme.TextColor },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
            }, "NumberContainer", "CurrentNumber");

            CuiHelper.DestroyUi(player, UI_PANEL_NAME);
            CuiHelper.AddUi(player, container);
        }

        void DestroyTimerUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, UI_PANEL_NAME);
            playerAnimationStates.Remove(player.userID);
            playerLastNumbers.Remove(player.userID);
        }

        void OnPlayerConnected(BasePlayer player)
        {
            if (isEventActive)
            {
                timer.Once(1f, () =>
                {
                    if (player != null && player.IsConnected)
                    {
                        CreateTimerUI(player);
                        playerAnimationStates[player.userID] = 0f;
                        playerLastNumbers[player.userID] = currentSeconds;
                    }
                });
            }
        }

        void OnPlayerDisconnected(BasePlayer player)
        {
            DestroyTimerUI(player);
            playerThemes.Remove(player.userID);
        }

        void BroadcastToAll(string message)
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                player.ChatMessage(message);
            }
        }
    }
}