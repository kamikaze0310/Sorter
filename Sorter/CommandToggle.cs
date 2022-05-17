using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using Sandbox.Common.ObjectBuilders;
using SimpleInventorySort;
using System.Text.RegularExpressions;
using Sandbox.Definitions;
//using Sandbox.Common.ObjectBuilders.Serializer;
using VRage;



namespace SimpleInventorySort
{
    public class CommandToggle : CommandHandlerBase
    {
        public override String GetCommandText()
        {
            return "toggle";
        }

        public override void HandleCommand(String[] words)
        {
            Settings.Instance.Enabled = !Settings.Instance.Enabled;

            if (Settings.Instance.Enabled)
                Communication.Message("Automated Sorting Toggled On.");
            else
                Communication.Message("Automated Sorting Toggled Off.");
        }
    }

    public class CommandQueue : CommandHandlerBase
    {
        public override String GetCommandText()
        {
            return "queue";
        }

        public override void HandleCommand(String[] words)
        {
            Settings.Instance.Enabled = !Settings.Instance.AddToQueue;

            if (Settings.Instance.Enabled)
                Communication.Message("Queue Toggled On.");
            else
                Communication.Message("Queue Toggled Off.");
        }
    }

    public class CommandEntities : CommandHandlerBase
    {
        public override String GetCommandText()
        {
            return "entities";
        }

        public override void HandleCommand(String[] words)
        {
            Settings.Instance.Enabled = !Settings.Instance.GatherEntities;

            if (Settings.Instance.Enabled)
                Communication.Message("Entity Gather Toggled On.");
            else
                Communication.Message("Entity Gather Off.");
        }
    }

    public class CommandInventory : CommandHandlerBase
    {
        public override String GetCommandText()
        {
            return "inventory";
        }

        public override void HandleCommand(String[] words)
        {
            Settings.Instance.Enabled = !Settings.Instance.GetInventories;

            if (Settings.Instance.Enabled)
                Communication.Message("Inventories Toggled On.");
            else
                Communication.Message("Inventories Toggled Off.");
        }
    }
}
