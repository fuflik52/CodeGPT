using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using UnityEngine;
using Oxide.Game.Rust.Cui;

namespace Oxide.Plugins
{
    [Info("Scroll Timer Plugin", "YourName", "1.0.2")]
    [Description("Timer UI plugin with smooth scroll animation for Rust")]
    public class TimerPluginScroll : RustPlugin
    {
        private Timer countdownTimer;
        private Timer animationTimer;
        private int currentSeconds = 30;
        private bool isEventActive = false;
        private const string UI_PANEL_NAME = "ScrollTimerPanel";
        private Dictionary<ulong, float> playerAnimationStates = new Dictionary<ulong, float>();
        private Dictionary<ulong, int> playerLastNumbers = new Dictionary<ulong, int>();

        void Init()
        {
            // Регистрируем команды
            cmd.AddChatCommand("starttimer", this, "CmdStartTimer");
            cmd.AddChatCommand("stoptimer", this, "CmdStopTimer");
            cmd.AddChatCommand("resettimer", this, "CmdResetTimer");
            cmd.AddChatCommand("settime", this, "CmdSetTime");
            cmd.AddChatCommand("timerhelp", this, "CmdHelp");
        }

        void OnServerInitialized()
        {
            Puts("Scroll Timer Plugin загружен!");
            Puts("Команды: /starttimer, /stoptimer, /resettimer, /settime <секунды>, /timerhelp");
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
        }

        [ChatCommand("starttimer")]
        void CmdStartTimer(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) 
            {
                player.ChatMessage("У вас нет прав для использования этой команды!");
                return;
            }

            if (isEventActive)
            {
                player.ChatMessage("Таймер уже запущен!");
                return;
            }

            StartTimer();
            player.ChatMessage("Таймер запущен! Ивент начнется через " + currentSeconds + " секунд.");
        }

        [ChatCommand("stoptimer")]
        void CmdStopTimer(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) 
            {
                player.ChatMessage("У вас нет прав для использования этой команды!");
                return;
            }

            StopTimer();
            player.ChatMessage("Таймер остановлен!");
        }

        [ChatCommand("resettimer")]
        void CmdResetTimer(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) 
            {
                player.ChatMessage("У вас нет прав для использования этой команды!");
                return;
            }

            ResetTimer();
            player.ChatMessage("Таймер сброшен!");
        }

        [ChatCommand("settime")]
        void CmdSetTime(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) 
            {
                player.ChatMessage("У вас нет прав для использования этой команды!");
                return;
            }

            if (args.Length == 0)
            {
                player.ChatMessage("Использование: /settime <секунды>");
                return;
            }

            if (int.TryParse(args[0], out int newTime) && newTime > 0 && newTime <= 3600)
            {
                currentSeconds = newTime;
                player.ChatMessage("Время установлено на " + newTime + " секунд.");
            }
            else
            {
                player.ChatMessage("Неверное значение времени! Используйте число от 1 до 3600.");
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
                    BroadcastToAll("🎉 Ивент начался! 🎉");
                    return;
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
            animationTimer = timer.Every(0.03f, () =>
            {
                bool animationComplete = true;
                
                foreach (var player in BasePlayer.activePlayerList)
                {
                    if (player == null || !player.IsConnected) continue;

                    float animationState = playerAnimationStates[player.userID];
                    animationState += 0.08f; // Скорость анимации
                    
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
                    Text = { Text = lastNumber.ToString(), FontSize = 28, Align = TextAnchor.MiddleCenter, Color = $"1 0.8 0 {alpha}" },
                    RectTransform = { AnchorMin = "0 " + oldY, AnchorMax = "1 " + (oldY + 1f) }
                }, "NumberContainer", "OldNumber");
            }
            
            // Новое число (приходит снизу)
            float newAlpha = Mathf.Lerp(0f, 1f, Mathf.Clamp01(progress * 2f));
            container.Add(new CuiLabel
            {
                Text = { Text = currentSeconds.ToString(), FontSize = 28, Align = TextAnchor.MiddleCenter, Color = $"1 0.8 0 {newAlpha}" },
                RectTransform = { AnchorMin = "0 " + currentY, AnchorMax = "1 " + (currentY + 1f) }
            }, "NumberContainer", "ScrollingNumber");

            CuiHelper.AddUi(player, container);
        }

        void UpdateTimerDisplay(BasePlayer player)
        {
            // Финальное обновление без анимации
            CuiHelper.DestroyUi(player, "ScrollingNumber");
            CuiHelper.DestroyUi(player, "OldNumber");
            
            var container = new CuiElementContainer();
            container.Add(new CuiLabel
            {
                Text = { Text = currentSeconds.ToString(), FontSize = 28, Align = TextAnchor.MiddleCenter, Color = "1 0.8 0 1" },
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
            currentSeconds = 30;
        }

        void CreateTimerUI(BasePlayer player)
        {
            var container = new CuiElementContainer();
            
            // Основная панель с градиентом и тенью
            container.Add(new CuiPanel
            {
                Image = { Color = "0.05 0.05 0.05 0.95", Material = "assets/content/ui/uibackgroundblur-ingame.mat" },
                RectTransform = { AnchorMin = "0.5 0.92", AnchorMax = "0.5 0.98" },
                CursorEnabled = false
            }, "Overlay", UI_PANEL_NAME);

            // Верхняя граница с градиентом
            container.Add(new CuiPanel
            {
                Image = { Color = "1 0.8 0 1" },
                RectTransform = { AnchorMin = "0 0.95", AnchorMax = "1 1" }
            }, UI_PANEL_NAME);

            // Нижняя граница
            container.Add(new CuiPanel
            {
                Image = { Color = "1 0.8 0 0.3" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.02" }
            }, UI_PANEL_NAME);

            // Текст "Ивент начнется через"
            container.Add(new CuiLabel
            {
                Text = { Text = "Ивент начнется через", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = "1 1 1 0.9" },
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
                Text = { Text = currentSeconds.ToString(), FontSize = 28, Align = TextAnchor.MiddleCenter, Color = "1 0.8 0 1" },
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