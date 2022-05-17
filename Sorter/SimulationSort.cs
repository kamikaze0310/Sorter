using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.Game;
using System.Threading.Tasks;
using Sandbox.ModAPI;
using System.Timers;
using VRage;
using VRageMath;
using VRage.Game.ModAPI;

namespace SimpleInventorySort
{
    public class SimulationSort : SimulationProcessorBase
    {
        private DateTime m_lastUpdate;
        private DateTime lastUpdate;
        private DateTime inventoryUpdate;
        private Timer m_sortTimer;
        private bool m_init = false;

        public SimulationSort()
        {
        }

        private void SetupSort()
        {
            Inventory.QueueReady = false;
            m_lastUpdate = DateTime.Now;
            lastUpdate = DateTime.Now;
            inventoryUpdate = DateTime.Now;
            m_sortTimer = new Timer();

            int intervalTime = 5;
            if (MyAPIGateway.Multiplayer.MultiplayerActive)
                intervalTime = Math.Max(5, Settings.Instance.Interval);
            else
                intervalTime = Math.Max(5, Settings.Instance.Interval);

            m_sortTimer.Interval = intervalTime * 1000;
            m_sortTimer.AutoReset = false;
            m_sortTimer.Elapsed += TimerElapsed;
            m_sortTimer.Enabled = true;
        }

        /// <summary>
        /// In order to try to stop from causing the game thread to pause on large sorts, we are going to use a Timer as a ThreadPool.  This may be
        /// extremely unsafe, but the entire sort mostly consists of reads, and any issue on read should safely exception and our state will be
        /// stable as we're just queuing "writes" for the game thread.  If this causes a lot of issues, I'll easily move it all back into the game
        /// thread, but I'd like to see this work.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!Settings.Instance.Enabled) return;
                if (!Inventory.QueueReady && !Inventory.QueueInventories)
                {
                    m_sortTimer.Enabled = false;

                    if (Settings.Instance.Enabled)
                    {
                        //if (DateTime.Now - CubeGridTracker.LastRebuild > TimeSpan.FromSeconds(60))
                        //CubeGridTracker.TriggerRebuild();

                        // This will need to rebuild every update for now since CustomNameChanged event is
                        // bugged.  This should increase performance once fixed.
                        //if (DateTime.Now - Inventory.LastRebuild > TimeSpan.FromSeconds(30))
                        //if (DateTime.Now - Inventory.LastRebuild > TimeSpan.FromSeconds(2))
                        //Inventory.TriggerRebuild();

                        //if (DateTime.Now - Conveyor.LastRebuild > TimeSpan.FromSeconds(30))
                        //Conveyor.TriggerRebuild();

                        // No longer sort on the client if we're in multiplayer.  
                        if (MyAPIGateway.Multiplayer.MultiplayerActive && !MyAPIGateway.Multiplayer.IsServer)
                            return;

                        //MyVisualScriptLogicProvider.ShowNotificationToAll($"Run Sort", 3000, "Green");
                        Inventory.NewSortInventory();

                        /*if (MyAPIGateway.Multiplayer.MultiplayerActive && MyAPIGateway.Multiplayer.IsServer)
                        {
                            
                            List<IMyPlayer> players = new List<IMyPlayer>();
                            MyAPIGateway.Multiplayer.Players.GetPlayers(players);

                            foreach(var item in players)
                            {
                                Inventory.SortInventory(item.PlayerID);
                            }
                            
                        
                        }*/
                        /*else
                        {
                            Inventory.NewSortInventory();
                        }*/
                        //MyVisualScriptLogicProvider.ShowNotification($"Ran Sort", 5000);

                        //Inventory.SortInventory();
                    }
                }
            }
            finally
            {
                int intervalTime = 5;
                if (MyAPIGateway.Multiplayer.MultiplayerActive)
                    intervalTime = Math.Max(5, Settings.Instance.Interval);
                else
                    intervalTime = Math.Max(5, Settings.Instance.Interval);

                m_sortTimer.Interval = intervalTime * 1000;
                m_sortTimer.Enabled = true;
            }
        }

        public override void Handle()
        {
            //if (MyAPIGateway.Session.Player == null)
            //	return;

            if (MyAPIGateway.Session == null)
                return;

            if (MyAPIGateway.Multiplayer.MultiplayerActive && !MyAPIGateway.Multiplayer.IsServer)
                return;

            try
            {
                if (!m_init)
                {
                    m_init = true;
                    SetupSort();
                }

                /*  This is old method before using the timer thread
                if (DateTime.Now - m_lastUpdate > TimeSpan.FromSeconds(2))
                {
                    m_lastUpdate = DateTime.Now;

                    if (Core.Enabled)
                    {
                        /*
                        if (DateTime.Now - CubeGridTracker.LastRebuild > TimeSpan.FromSeconds(60))
                            CubeGridTracker.TriggerRebuild();

                        // This will need to rebuild every update for now since CustomNameChanged event is
                        // bugged.  This should increase performance once fixed.
                        if (DateTime.Now - Inventory.LastRebuild > TimeSpan.FromSeconds(30))
                        if (DateTime.Now - Inventory.LastRebuild > TimeSpan.FromSeconds(2))
                            Inventory.TriggerRebuild();

                        if (DateTime.Now - Conveyor.LastRebuild > TimeSpan.FromSeconds(30))
                            Conveyor.TriggerRebuild();
                    }
                }
                */
                if (!Settings.Instance.Enabled && !Settings.Instance.ChatToggle) return;

                if (DateTime.Now - m_lastUpdate > TimeSpan.FromMilliseconds(2))
                {
                    m_lastUpdate = DateTime.Now;
                    if (Inventory.QueueReady)
                        Inventory.ProcessQueue();
                }

                if (DateTime.Now - lastUpdate > TimeSpan.FromMilliseconds(500))
                {
                    lastUpdate = DateTime.Now;
                    if (Inventory.QueueReady) return;
                    if (Inventory.QueueEntities == 0) return;
                    if (Inventory.QueueInventories) return;
                    Inventory.FindInventories();
                }

                if (DateTime.Now - inventoryUpdate > TimeSpan.FromMilliseconds(300))
                {
                    inventoryUpdate = DateTime.Now;
                    if (Inventory.QueueReady) return;
                    if (!Inventory.QueueInventories) return;
                    Inventory.CompareInventories();
                }
            }
            catch (Exception ex)
            {
                Logging.Instance.WriteLine(String.Format("Handle(): {0}", ex.ToString()));
            }
            finally
            {
                if (Inventory.QueueReady && Inventory.QueueCount < 1)
                    Inventory.QueueReady = false;
            }

            base.Handle();
        }
    }
}
