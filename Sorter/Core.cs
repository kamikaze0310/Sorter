using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Common;
using Sandbox.ModAPI;
using Sandbox.Definitions;
using Sandbox.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;

namespace SimpleInventorySort
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class Core : MySessionComponentBase
    {
        // Declarations
        private static string version = "v0.1.0.21";
        private static bool m_debug = false;

        private bool m_initialized = false;
        private List<CommandHandlerBase> m_chatHandlers = new List<CommandHandlerBase>();
        private List<SimulationProcessorBase> m_simHandlers = new List<SimulationProcessorBase>();

        // Properties
        public static bool Debug
        {
            get { return m_debug; }
            set { m_debug = value; }
        }

        // Initializers
        private void Initialize()
        {
            // Load Settings
            Settings.Instance.Load();

            // Chat Line Event
            AddMessageHandler();

            // Chat Handlers
            m_chatHandlers.Add(new CommandToggle());
            m_chatHandlers.Add(new CommandDebug());
            m_chatHandlers.Add(new CommandFaction());
            m_chatHandlers.Add(new CommandManual());
            m_chatHandlers.Add(new CommandInterval());
            m_chatHandlers.Add(new CommandSettings());
            m_chatHandlers.Add(new CommandQueue());
            m_chatHandlers.Add(new CommandEntities());
            m_chatHandlers.Add(new CommandInventory());

            if (MyAPIGateway.Multiplayer.MultiplayerActive && !MyAPIGateway.Multiplayer.IsServer)
                return;

            // Simulation Handlers
            m_simHandlers.Add(new SimulationSort());

            // Setup Grid Tracker
            //CubeGridTracker.SetupGridTracking();
            //CubeGridTracker.TriggerRebuild();
            Inventory.TriggerRebuild();

            Logging.Instance.WriteLine(String.Format("Script Initializeda: {0}", version));

            //MyPerGameSettings.BallFriendlyPhysics = true;            
        }

        // Utility
        public void HandleMessageEntered(string messageText, ref bool sendToOthers)
        {
            try
            {
                if (messageText.StartsWith("/sort") ||
                messageText.StartsWith("/sort toggle") ||
                messageText.StartsWith("/sort chattoggle") ||
                messageText.StartsWith("/sort interval"))
                
                {
                    long clientId = MyAPIGateway.Session.LocalHumanPlayer.IdentityId;
                    messageText += "\n" + clientId.ToString();
                    sendToOthers = false;
                    var sendData = MyAPIGateway.Utilities.SerializeToBinary(messageText);
                    MyAPIGateway.Multiplayer.SendMessageToServer(4350, sendData);
                }

                /*if (messageText[0] != '/')
                    return;

                string[] commandParts = messageText.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (commandParts[0].ToLower() != "/sort")
                    return;

                sendToOthers = false;
                IMyPlayer player = MyAPIGateway.Session.LocalHumanPlayer;
                if (player.PromoteLevel != MyPromoteLevel.Owner && player.PromoteLevel != MyPromoteLevel.Admin)
                {
                    Communication.Message("Only admins can use these chat commands.");
                    return;
                }

                var sendData = MyAPIGateway.Utilities.SerializeToBinary(messageText);
                MyAPIGateway.Multiplayer.SendMessageToServer(4350, sendData);*/

                /*string[] commandParts = messageText.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (commandParts[0].ToLower() != "/sort")
                    return;

                sendToOthers = false;
                int paramCount = commandParts.Length - 1;
                if (paramCount < 1 || (paramCount == 1 && commandParts[1].ToLower() == "help"))
                {
                    List<String> commands = new List<string>();
                    foreach (CommandHandlerBase chatHandler in m_chatHandlers)
                    {
                        String commandBase = chatHandler.GetCommandText().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).First();
                        if (!commands.Contains(commandBase))
                            commands.Add(commandBase);
                    }

                    String commandList = String.Join(", ", commands);
                    String info = String.Format("Simple Inventory Sort {0}.  Available Commands: {1}", version, commandList);

                    //if (MyAPIGateway.Multiplayer.MultiplayerActive)
                    //    info += string.Format("\nSort commands no longer function in multiplayer.  Sorting happens every 30 seconds, and run on the server due to the new Netcode.");

                    Communication.Message(info);
                    return;
                }

                foreach (CommandHandlerBase chatHandler in m_chatHandlers)
                {
                    int commandCount = 0;
                    if (chatHandler.CanHandle(commandParts.Skip(1).ToArray(), ref commandCount))
                    {
                        chatHandler.HandleCommand(commandParts.Skip(commandCount + 1).ToArray());
                        return;
                    }
                }*/
            }
            catch (Exception ex)
            {
                //Logging.Instance.WriteLine(String.Format("HandleMessageEntered(): {0}", ex.ToString()));
            }
        }

        public void MessageHandler(byte[] data)
        {
            try
            {
                var messageText = MyAPIGateway.Utilities.SerializeFromBinary<string>(data);
                var split = messageText.Split('\n');

                long playerId = 0;
                IMyPlayer client = null;
                if (!long.TryParse(split[1], out playerId)) return;

                List<IMyPlayer> players = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(players);

                foreach (var player in players)
                {
                    if (player.IdentityId != playerId) continue;
                    client = player;
                    break;
                }
                if (client == null) return;

                if (split[0] == "/sort")
                {
                    if (!Settings.Instance.ChatToggle)
                    {
                        MyVisualScriptLogicProvider.SendChatMessageColored("Chat sort is disabled", Color.Red, "Sorter", playerId, "Red");
                        return;
                    }

                    

                    
                    var entity = client.Controller?.ControlledEntity?.Entity;
                    if (entity == null)
                    {
                        MyVisualScriptLogicProvider.SendChatMessageColored("You need to be in a seat to sort", Color.Red, "Sorter", playerId, "Red");
                        return;
                    }

                    var block = entity as IMyTerminalBlock;
                    if (block == null)
                    {
                        MyVisualScriptLogicProvider.SendChatMessageColored("You need to be in a seat to sort", Color.Red, "Sorter", playerId, "Red");
                        return;
                    }

                    var grid = block.CubeGrid;
                    if (grid == null) return;

                    Inventory.ChatSortInventory(grid);
                    return;
                }

                if (split[0] == "/sort chattoggle")
                {
                    if (client.PromoteLevel != MyPromoteLevel.Admin && client.PromoteLevel != MyPromoteLevel.Owner)
                    {
                        MyVisualScriptLogicProvider.SendChatMessageColored("Your Unauthorized Access Attempt Has Been Logged.", Color.Red, "Sorter", playerId, "Red");
                        return;
                    }

                    Settings.Instance.ChatToggle = !Settings.Instance.ChatToggle;
                    MyVisualScriptLogicProvider.SendChatMessageColored($"Chat Toggle Sorter is now {Settings.Instance.ChatToggle}", Color.Red, "Sorter", playerId, "Red");
                    return;
                }

                if (split[0] == "/sort toggle")
                {
                    if (client.PromoteLevel != MyPromoteLevel.Admin && client.PromoteLevel != MyPromoteLevel.Owner)
                    {
                        MyVisualScriptLogicProvider.SendChatMessageColored("Your Unauthorized Access Attempt Has Been Logged.", Color.Red, "Sorter", playerId, "Red");
                        return;
                    }

                    Settings.Instance.Enabled = !Settings.Instance.Enabled;
                    MyVisualScriptLogicProvider.SendChatMessageColored($"Auto Sorter is now {Settings.Instance.Enabled}", Color.Red, "Sorter", playerId, "Red");
                    return;
                }

                if(split[0].Contains("/sort interval"))
                {
                    if (client.PromoteLevel != MyPromoteLevel.Admin && client.PromoteLevel != MyPromoteLevel.Owner)
                    {
                        MyVisualScriptLogicProvider.SendChatMessageColored("Your Unauthorized Access Attempt Has Been Logged.", Color.Red, "Sorter", playerId, "Red");
                        return;
                    }

                    var split2 = split[0].Split('.');
                    if (split2.Length < 2) return;

                    int interval = 5;
                    if (!int.TryParse(split[1], out interval)) return;
                    Settings.Instance.Interval = interval;
                    MyVisualScriptLogicProvider.SendChatMessageColored($"Auto Sorter Interval is now {Settings.Instance.Interval} seconds", Color.Red, "Sorter", playerId, "Red");
                    return;
                }

                /*string[] commandParts = messageText.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (commandParts[0].ToLower() != "/sort")
                    return;

                //sendToOthers = false;
                int paramCount = commandParts.Length - 1;
                if (paramCount < 1 || (paramCount == 1 && commandParts[1].ToLower() == "help"))
                {
                    List<String> commands = new List<string>();
                    foreach (CommandHandlerBase chatHandler in m_chatHandlers)
                    {
                        String commandBase = chatHandler.GetCommandText().Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).First();
                        if (!commands.Contains(commandBase))
                            commands.Add(commandBase);
                    }

                    String commandList = String.Join(", ", commands);
                    String info = String.Format("Simple Inventory Sort {0}.  Available Commands: {1}", version, commandList);

                    //if (MyAPIGateway.Multiplayer.MultiplayerActive)
                    //    info += string.Format("\nSort commands no longer function in multiplayer.  Sorting happens every 30 seconds, and run on the server due to the new Netcode.");

                    Communication.Message(info);
                    return;
                }

                foreach (CommandHandlerBase chatHandler in m_chatHandlers)
                {
                    int commandCount = 0;
                    if (chatHandler.CanHandle(commandParts.Skip(1).ToArray(), ref commandCount))
                    {
                        chatHandler.HandleCommand(commandParts.Skip(commandCount + 1).ToArray());
                        return;
                    }
                }*/
            }
            catch (Exception ex)
            {
                //Logging.Instance.WriteLine(String.Format("HandleMessageEntered(): {0}", ex.ToString()));
            }
        }

        public void AddMessageHandler()
        {
            MyAPIGateway.Utilities.MessageEntered += HandleMessageEntered;
            MyAPIGateway.Multiplayer.RegisterMessageHandler(4350, MessageHandler);
        }

        public void RemoveMessageHandler()
        {
            MyAPIGateway.Utilities.MessageEntered -= HandleMessageEntered;
            MyAPIGateway.Multiplayer.UnregisterMessageHandler(4350, MessageHandler);
        }

        // Overrides
        public override void UpdateBeforeSimulation()
        {
            try
            {
                if (MyAPIGateway.Utilities == null)
                    return;

                // Run the init
                if (!m_initialized)
                {
                    m_initialized = true;
                    Initialize();
                }

                if (MyAPIGateway.Multiplayer.MultiplayerActive && !MyAPIGateway.Multiplayer.IsServer)
                    return;

                // Run the sim handlers
                foreach (SimulationProcessorBase simHandler in m_simHandlers)
                {
                    simHandler.Handle();
                }
            }
            catch (Exception ex)
            {
                Logging.Instance.WriteLine(String.Format("UpdateBeforeSimulation(): {0}", ex.ToString()));
            }
        }

        protected override void UnloadData()
        {
            try
            {
                RemoveMessageHandler();
                if (Logging.Instance != null)
                    Logging.Instance.Close();
            }
            catch { }

            base.UnloadData();
        }
    }
}
