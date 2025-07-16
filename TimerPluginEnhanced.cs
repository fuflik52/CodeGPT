using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using UnityEngine;
using Oxide.Game.Rust.Cui;

namespace Oxide.Plugins
{
    [Info("Enhanced Timer Plugin", "YourName", "1.0.1")]
    [Description("Enhanced timer UI plugin with smooth scroll animation")]
    public class TimerPluginEnhanced : RustPlugin
    {
        private Timer countdownTimer;
        private Timer animationTimer;
        private int currentSeconds = 30;
        private bool isEventActive = false;
        private const string UI_PANEL_NAME = "EnhancedTimerPanel";
        private Dictionary<ulong, float> playerAnimationStates = new Dictionary<ulong, float>();

        void Init()
        {
            // Регистрируем команды
            cmd.AddChatCommand("starttimer", this, "CmdStartTimer");
            cmd.AddChatCommand("stoptimer", this, "CmdStopTimer");
            cmd.AddChatCommand("resettimer", this, "CmdResetTimer");
            cmd.AddChatCommand("settime", this, "CmdSetTime");
        }

        void OnServerInitialized()
        {
            Puts("Enhanced Timer Plugin загружен!");
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
        }

        [ChatCommand("starttimer")]
        void CmdStartTimer(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

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
            if (!player.IsAdmin) return;

            StopTimer();
            player.ChatMessage("Таймер остановлен!");
        }

        [ChatCommand("resettimer")]
        void CmdResetTimer(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            ResetTimer();
            player.ChatMessage("Таймер сброшен!");
        }

        [ChatCommand("settime")]
        void CmdSetTime(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (args.Length == 0)
            {
                player.ChatMessage("Использование: /settime <секунды>");
                return;
            }

            if (int.TryParse(args[0], out int newTime) && newTime > 0)
            {
                currentSeconds = newTime;
                player.ChatMessage("Время установлено на " + newTime + " секунд.");
            }
            else
            {
                player.ChatMessage("Неверное значение времени!");
            }
        }

        void StartTimer()
        {
            isEventActive = true;
            
            // Показываем UI всем игрокам
            foreach (var player in BasePlayer.activePlayerList)
            {
                CreateTimerUI(player);
                playerAnimationStates[player.userID] = 0f;
            }

            // Запускаем таймер
            countdownTimer = timer.Every(1f, () =>
            {
                currentSeconds--;
                
                if (currentSeconds <= 0)
                {
                    StopTimer();
                    BroadcastToAll("Ивент начался!");
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
            animationTimer = timer.Every(0.05f, () =>
            {
                bool animationComplete = true;
                
                foreach (var player in BasePlayer.activePlayerList)
                {
                    if (player == null || !player.IsConnected) continue;

                    float animationState = playerAnimationStates[player.userID];
                    animationState += 0.1f; // Скорость анимации
                    
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
            // Создаем эффект прокрутки вниз
            float startY = 1f;
            float endY = 0f;
            float currentY = Mathf.Lerp(startY, endY, progress);
            
            // Удаляем старые анимированные элементы
            CuiHelper.DestroyUi(player, "ScrollingNumber");
            CuiHelper.DestroyUi(player, "OldNumber");
            
            var container = new CuiElementContainer();
            
            // Старое число (уходит вверх)
            if (progress < 0.5f)
            {
                float oldY = Mathf.Lerp(0f, -0.5f, progress * 2f);
                container.Add(new CuiLabel
                {
                    Text = { Text = (currentSeconds + 1).ToString(), FontSize = 24, Align = TextAnchor.MiddleCenter, Color = "1 0.8 0 0.5" },
                    RectTransform = { AnchorMin = "0 " + oldY, AnchorMax = "1 " + (oldY + 1f) }
                }, "NumberContainer", "OldNumber");
            }
            
            // Новое число (приходит снизу)
            container.Add(new CuiLabel
            {
                Text = { Text = currentSeconds.ToString(), FontSize = 24, Align = TextAnchor.MiddleCenter, Color = "1 0.8 0 1" },
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
                Text = { Text = currentSeconds.ToString(), FontSize = 24, Align = TextAnchor.MiddleCenter, Color = "1 0.8 0 1" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
            }, "NumberContainer", "CurrentNumber");

            CuiHelper.AddUi(player, container);
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
        }

        void ResetTimer()
        {
            StopTimer();
            currentSeconds = 30;
        }

        void CreateTimerUI(BasePlayer player)
        {
            var container = new CuiElementContainer();
            
            // Основная панель с градиентом
            container.Add(new CuiPanel
            {
                Image = { Color = "0.1 0.1 0.1 0.9", Material = "assets/content/ui/uibackgroundblur-ingame.mat" },
                RectTransform = { AnchorMin = "0.5 0.92", AnchorMax = "0.5 0.98" },
                CursorEnabled = false
            }, "Overlay", UI_PANEL_NAME);

            // Верхняя граница
            container.Add(new CuiPanel
            {
                Image = { Color = "1 0.8 0 1" },
                RectTransform = { AnchorMin = "0 0.95", AnchorMax = "1 1" }
            }, UI_PANEL_NAME);

            // Текст "Ивент начнется через"
            container.Add(new CuiLabel
            {
                Text = { Text = "Ивент начнется через", FontSize = 12, Align = TextAnchor.MiddleCenter, Color = "1 1 1 0.8" },
                RectTransform = { AnchorMin = "0 0.7", AnchorMax = "1 0.95" }
            }, UI_PANEL_NAME);

            // Контейнер для анимированного числа
            container.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.7" },
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