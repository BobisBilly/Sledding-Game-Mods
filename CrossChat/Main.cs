using MelonLoader;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;

namespace CrossChat
{
	public class Main : MelonMod
	{
		public override void OnInitializeMelon()
		{
			var modColor = typeof(Main).Assembly.GetCustomAttribute<MelonColorAttribute>();

			string modColorString = (modColor != null)
		? $"\x1b[38;2;{modColor.DrawingColor.R};{modColor.DrawingColor.G};{modColor.DrawingColor.B}m"
		: "\x1b[38;2;0;255;255m";

			LoggerInstance.Msg($"Hello from {modColorString}{Info.Name}\x1b[0m!");
		}

		public override void OnUpdate()
		{
			WSHelper.ProcessMainThreadQueue();
		}
	}

	[HarmonyPatch(typeof(LobbySettingsManager), nameof(LobbySettingsManager.Instance.OnStartServer))]
	public static class OnStartServerPatch
	{
		[HarmonyPostfix]
		public static void PostFix()
		{
			_ = WSHelper.Connect();
		}
	}

	[HarmonyPatch(typeof(LobbySettingsManager), nameof(LobbySettingsManager.Instance.OnStopServer))]
	public static class OnStopServerPatch
	{
		[HarmonyPostfix]
		public static void PostFix()
		{
			_ = WSHelper.Disconnect();
		}
	}

	[HarmonyPatch(typeof(ChatManager), nameof(ChatManager.SendPlayerChatMessage))]
	public static class SendPlayerChatMessagePatch
	{
		[HarmonyPostfix]
		public static void PostFix(string message)
		{
			int scope = ChatManager.Instance.GetIsUsingGlobalChat() ? 0 : 1;

			_ = WSHelper.SendMessage(scope, message);
		}
	}
}