using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using UnityEngine;
using Oxide.Game.Rust.Cui;

namespace Oxide.Plugins
{
    [Info("Timer Plugin Examples", "YourName", "1.0.0")]
    [Description("Примеры расширенного использования таймера")]
    public class TimerPluginExamples : RustPlugin
    {
        // Пример интеграции с другими плагинами
        private TimerPluginEnhanced timerPlugin;

        void OnServerInitialized()
        {
            // Получаем ссылку на основной плагин таймера
            timerPlugin = plugins.Find("TimerPluginEnhanced") as TimerPluginEnhanced;
            
            if (timerPlugin == null)
            {
                Puts("TimerPluginEnhanced не найден! Убедитесь, что он загружен.");
                return;
            }

            Puts("Timer Plugin Examples загружен!");
        }

        // Пример: Автоматический запуск таймера при определенных условиях
        void OnPlayerDeath(BasePlayer player, HitInfo info)
        {
            // Запускаем таймер когда игрок умирает
            if (player != null && info?.InitiatorPlayer != null)
            {
                timer.Once(2f, () =>
                {
                    // Запускаем таймер на 10 секунд для респауна
                    StartRespawnTimer(player);
                });
            }
        }

        void StartRespawnTimer(BasePlayer player)
        {
            if (player == null || !player.IsConnected) return;

            // Создаем специальный UI для респауна
            CreateRespawnUI(player);
            
            // Запускаем обратный отсчет
            int respawnTime = 10;
            Timer respawnTimer = timer.Every(1f, () =>
            {
                respawnTime--;
                
                if (respawnTime <= 0)
                {
                    // Респаун игрока
                    player.Respawn();
                    DestroyRespawnUI(player);
                    respawnTimer.Destroy();
                    return;
                }

                UpdateRespawnUI(player, respawnTime);
            });
        }

        void CreateRespawnUI(BasePlayer player)
        {
            var container = new CuiElementContainer();
            
            // Панель респауна
            container.Add(new CuiPanel
            {
                Image = { Color = "0.2 0.2 0.2 0.9" },
                RectTransform = { AnchorMin = "0.4 0.4", AnchorMax = "0.6 0.6" },
                CursorEnabled = false
            }, "Overlay", "RespawnPanel");

            // Заголовок
            container.Add(new CuiLabel
            {
                Text = { Text = "Респаун через", FontSize = 16, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0 0.7", AnchorMax = "1 1" }
            }, "RespawnPanel");

            // Таймер
            container.Add(new CuiLabel
            {
                Text = { Text = "10", FontSize = 32, Align = TextAnchor.MiddleCenter, Color = "1 0.5 0 1" },
                RectTransform = { AnchorMin = "0 0.2", AnchorMax = "1 0.7" }
            }, "RespawnPanel", "RespawnTimer");

            CuiHelper.DestroyUi(player, "RespawnPanel");
            CuiHelper.AddUi(player, container);
        }

        void UpdateRespawnUI(BasePlayer player, int time)
        {
            CuiHelper.DestroyUi(player, "RespawnTimer");
            
            var container = new CuiElementContainer();
            container.Add(new CuiLabel
            {
                Text = { Text = time.ToString(), FontSize = 32, Align = TextAnchor.MiddleCenter, Color = "1 0.5 0 1" },
                RectTransform = { AnchorMin = "0 0.2", AnchorMax = "1 0.7" }
            }, "RespawnPanel", "RespawnTimer");

            CuiHelper.AddUi(player, container);
        }

        void DestroyRespawnUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, "RespawnPanel");
        }

        // Пример: Таймер для ивентов
        [ChatCommand("event")]
        void CmdStartEvent(BasePlayer player, string command, string[] args)
        {
            if (!player.IsAdmin) return;

            if (args.Length == 0)
            {
                player.ChatMessage("Использование: /event <тип> (raid, airdrop, heli)");
                return;
            }

            string eventType = args[0].ToLower();
            int eventTime = 60; // По умолчанию 60 секунд

            switch (eventType)
            {
                case "raid":
                    eventTime = 300; // 5 минут
                    StartRaidEvent(player, eventTime);
                    break;
                case "airdrop":
                    eventTime = 180; // 3 минуты
                    StartAirdropEvent(player, eventTime);
                    break;
                case "heli":
                    eventTime = 240; // 4 минуты
                    StartHeliEvent(player, eventTime);
                    break;
                default:
                    player.ChatMessage("Неизвестный тип ивента: " + eventType);
                    break;
            }
        }

        void StartRaidEvent(BasePlayer player, int time)
        {
            CreateEventUI("RAID EVENT", time, "1 0.2 0.2 1");
            BroadcastToAll("RAID EVENT начнется через " + time + " секунд!");
            
            Timer eventTimer = timer.Every(1f, () =>
            {
                time--;
                UpdateEventUI(time);
                
                if (time <= 0)
                {
                    BroadcastToAll("RAID EVENT НАЧАЛСЯ! Все двери открыты на 5 минут!");
                    DestroyEventUI();
                    eventTimer.Destroy();
                    
                    // Здесь можно добавить логику ивента
                    // Например, открыть все двери на сервере
                }
            });
        }

        void StartAirdropEvent(BasePlayer player, int time)
        {
            CreateEventUI("AIRDROP EVENT", time, "0.2 0.8 1 1");
            BroadcastToAll("AIRDROP EVENT начнется через " + time + " секунд!");
            
            Timer eventTimer = timer.Every(1f, () =>
            {
                time--;
                UpdateEventUI(time);
                
                if (time <= 0)
                {
                    BroadcastToAll("AIRDROP EVENT НАЧАЛСЯ! Аирдропы падают каждые 30 секунд!");
                    DestroyEventUI();
                    eventTimer.Destroy();
                    
                    // Здесь можно добавить логику аирдропов
                }
            });
        }

        void StartHeliEvent(BasePlayer player, int time)
        {
            CreateEventUI("HELI EVENT", time, "0.8 0.8 0.2 1");
            BroadcastToAll("HELI EVENT начнется через " + time + " секунд!");
            
            Timer eventTimer = timer.Every(1f, () =>
            {
                time--;
                UpdateEventUI(time);
                
                if (time <= 0)
                {
                    BroadcastToAll("HELI EVENT НАЧАЛСЯ! Вертолеты атакуют базы!");
                    DestroyEventUI();
                    eventTimer.Destroy();
                    
                    // Здесь можно добавить логику вертолетов
                }
            });
        }

        void CreateEventUI(string eventName, int time, string color)
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                var container = new CuiElementContainer();
                
                // Основная панель
                container.Add(new CuiPanel
                {
                    Image = { Color = "0.1 0.1 0.1 0.95" },
                    RectTransform = { AnchorMin = "0.3 0.8", AnchorMax = "0.7 0.95" },
                    CursorEnabled = false
                }, "Overlay", "EventPanel");

                // Название ивента
                container.Add(new CuiLabel
                {
                    Text = { Text = eventName, FontSize = 18, Align = TextAnchor.MiddleCenter, Color = color },
                    RectTransform = { AnchorMin = "0 0.6", AnchorMax = "1 1" }
                }, "EventPanel");

                // Таймер
                container.Add(new CuiLabel
                {
                    Text = { Text = time.ToString(), FontSize = 24, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                    RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.6" }
                }, "EventPanel", "EventTimer");

                CuiHelper.DestroyUi(player, "EventPanel");
                CuiHelper.AddUi(player, container);
            }
        }

        void UpdateEventUI(int time)
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                CuiHelper.DestroyUi(player, "EventTimer");
                
                var container = new CuiElementContainer();
                container.Add(new CuiLabel
                {
                    Text = { Text = time.ToString(), FontSize = 24, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                    RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.6" }
                }, "EventPanel", "EventTimer");

                CuiHelper.AddUi(player, container);
            }
        }

        void DestroyEventUI()
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                CuiHelper.DestroyUi(player, "EventPanel");
            }
        }

        void BroadcastToAll(string message)
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                player.ChatMessage(message);
            }
        }

        void Unload()
        {
            // Очищаем все UI при выгрузке
            foreach (var player in BasePlayer.activePlayerList)
            {
                CuiHelper.DestroyUi(player, "RespawnPanel");
                CuiHelper.DestroyUi(player, "EventPanel");
            }
        }
    }
}