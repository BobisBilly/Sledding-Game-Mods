using MelonLoader;
using System.Reflection;
using UnityEngine;
using Il2CppTMPro;

namespace NoLimits
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

		public override void OnLateInitializeMelon()
		{
			string[] editObjectsList = ["World/Safe Zone", "World/Lodge/Fireplace Stuff/Cube", "World/Lodge Platform/Sign/Canvas/Text (TMP)"];
            string[] deleteObjectsList = ["Audio/(Source) Snowstorm Wind", " --------------- DEMO STUFF ---------------/Map Boundary Controller [ DEMO ]", " --------------- DEMO STUFF ---------------/DEMO FENCE", "Snowstorm Snow"];

			for (int i = 0; i < editObjectsList.Length; i++)
			{
				GameObject currentObject = GameObject.Find(editObjectsList[i]);

				if (currentObject != null)
				{
					if (currentObject.name == "Text (TMP)")
					{
						var textComponent = currentObject.GetComponent<TMP_Text>();

						if (textComponent != null)
						{
							textComponent.text = "Thanks for using No Limits!";
						} else {
                            LoggerInstance.BigError($"{editObjectsList[i]} textComponent couldn't be found, please tell BobisBilly on Discord so he can fix it.");
                        }
					}
					else
					{
						currentObject.SetActive(false);
					}
				}
				else
				{
					LoggerInstance.BigError($"Unable to find {editObjectsList[i]}, Could be from it not being loaded yet or from it not existing, please tell BobisBilly on Discord so he can fix it.");
				}
			}

            for (int i = 0; i < deleteObjectsList.Length; i++) {
                GameObject currentObject = GameObject.Find(deleteObjectsList[i]);

                if (currentObject != null) {
                    UnityEngine.Object.Destroy(currentObject);
                } else {
                    LoggerInstance.BigError($"Unable to find {editObjectsList[i]}, Could be from it not being loaded yet or from it not existing, please tell BobisBilly on Discord so he can fix it.");
                }
            }

			base.OnLateInitializeMelon();
		}
	}
}