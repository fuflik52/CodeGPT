using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using UnityEngine;
using Oxide.Game.Rust.Cui;

namespace Oxide.Plugins
{
    [Info("Timer Plugin", "YourName", "1.0.0")]
    [Description("Timer UI plugin with countdown animation")]
    public class TimerPlugin : RustPlugin
    {
        private Timer countdownTimer;
        private int currentSeconds = 30;
        private bool isEventActive = false;
        private const string UI_PANEL_NAME = "TimerPanel";

        void Init()
        {
            // Регистрируем команды
            cmd.AddChatCommand("starttimer", this, "CmdStartTimer");
            cmd.AddChatCommand("stoptimer", this, "CmdStopTimer");
            cmd.AddChatCommand("resettimer", this, "CmdResetTimer");
        }

        void OnServerInitialized()
        {
            // Инициализация при запуске сервера
            Puts("Timer Plugin загружен!");
        }

        void Unload()
        {
            // Очистка при выгрузке плагина
            if (countdownTimer != null)
            {
                countdownTimer.Destroy();
            }
            
            // Удаляем UI у всех игроков
            foreach (var player in BasePlayer.activePlayerList)
            {
                DestroyTimerUI(player);
            }
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
            player.ChatMessage("Таймер запущен! Ивент начнется через 30 секунд.");
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

        void StartTimer()
        {
            isEventActive = true;
            currentSeconds = 30;
            
            // Показываем UI всем игрокам
            foreach (var player in BasePlayer.activePlayerList)
            {
                CreateTimerUI(player);
            }

            // Запускаем таймер
            countdownTimer = timer.Every(1f, () =>
            {
                currentSeconds--;
                
                if (currentSeconds <= 0)
                {
                    // Ивент завершен
                    StopTimer();
                    BroadcastToAll("Ивент начался!");
                    return;
                }

                // Обновляем UI у всех игроков
                foreach (var player in BasePlayer.activePlayerList)
                {
                    UpdateTimerUI(player);
                }
            });
        }

        void StopTimer()
        {
            isEventActive = false;
            
            if (countdownTimer != null)
            {
                countdownTimer.Destroy();
                countdownTimer = null;
            }

            // Скрываем UI у всех игроков
            foreach (var player in BasePlayer.activePlayerList)
            {
                DestroyTimerUI(player);
            }
        }

        void ResetTimer()
        {
            StopTimer();
            currentSeconds = 30;
        }

        void CreateTimerUI(BasePlayer player)
        {
            var container = new CuiElementContainer();
            
            // Основная панель
            container.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0.8" },
                RectTransform = { AnchorMin = "0.5 0.95", AnchorMax = "0.5 0.98" },
                CursorEnabled = false
            }, "Overlay", UI_PANEL_NAME);

            // Текст "Ивент начнется через"
            container.Add(new CuiLabel
            {
                Text = { Text = "Ивент начнется через", FontSize = 14, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0 0.6", AnchorMax = "1 1" }
            }, UI_PANEL_NAME);

            // Контейнер для анимированного числа
            container.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.6" },
                CursorEnabled = false
            }, UI_PANEL_NAME, "NumberContainer");

            // Начальное число
            container.Add(new CuiLabel
            {
                Text = { Text = currentSeconds.ToString(), FontSize = 24, Align = TextAnchor.MiddleCenter, Color = "1 0.8 0 1" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
            }, "NumberContainer", "CurrentNumber");

            CuiHelper.DestroyUi(player, UI_PANEL_NAME);
            CuiHelper.AddUi(player, container);
        }

        void UpdateTimerUI(BasePlayer player)
        {
            if (!isEventActive) return;

            // Удаляем старое число
            CuiHelper.DestroyUi(player, "CurrentNumber");

            // Создаем новое число с анимацией прокрутки
            var container = new CuiElementContainer();
            
            // Анимированное число (прокрутка вниз)
            container.Add(new CuiLabel
            {
                Text = { Text = currentSeconds.ToString(), FontSize = 24, Align = TextAnchor.MiddleCenter, Color = "1 0.8 0 1" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
            }, "NumberContainer", "CurrentNumber");

            CuiHelper.AddUi(player, container);

            // Добавляем эффект прокрутки через CSS анимацию
            timer.Once(0.1f, () =>
            {
                if (player == null || !player.IsConnected) return;
                
                var scrollContainer = new CuiElementContainer();
                scrollContainer.Add(new CuiLabel
                {
                    Text = { Text = currentSeconds.ToString(), FontSize = 24, Align = TextAnchor.MiddleCenter, Color = "1 0.8 0 1" },
                    RectTransform = { AnchorMin = "0 -0.2", AnchorMax = "1 0.8" }
                }, "NumberContainer", "ScrollingNumber");

                CuiHelper.AddUi(player, scrollContainer);
            });
        }

        void DestroyTimerUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, UI_PANEL_NAME);
        }

        void OnPlayerConnected(BasePlayer player)
        {
            // Показываем UI новому игроку если таймер активен
            if (isEventActive)
            {
                timer.Once(1f, () =>
                {
                    if (player != null && player.IsConnected)
                    {
                        CreateTimerUI(player);
                    }
                });
            }
        }

        void OnPlayerDisconnected(BasePlayer player)
        {
            // Очищаем UI при отключении игрока
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