using System;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game;
using VRageMath;

namespace SimpleInventorySort
{
    static class Communication
    {
        static public void Message(String text)
        {
            MyAPIGateway.Utilities.ShowMessage("[Inventory Sort]", text);
            //MyVisualScriptLogicProvider.SendChatMessageColored(text, Color.Green, "[Inventory Sort]", playerId, "Green");
        }

        static public void Notification(String text, int disappearTimeMS = 2000, string font = "White")
        {
            MyAPIGateway.Utilities.ShowNotification(text, disappearTimeMS, font);
        }
    }
}
