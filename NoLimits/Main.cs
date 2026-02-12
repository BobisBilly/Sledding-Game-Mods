using MelonLoader;
using System.Reflection;
using UnityEngine;
using Il2CppTMPro;
using Il2Cpp;

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
			string[] deleteObjectsList = ["Audio/(Source) Snowstorm Wind", " --------------- DEMO STUFF ---------------/Map Boundary Controller [ DEMO ]", " --------------- DEMO STUFF ---------------/DEMO FENCE", "Snowstorm Snow", "------------------ SYSTEMS ------------------"];
			string[] liftObjectsList = ["Ski Lift (3)", "Ski Lift (1)", "Ski Lift (7)", "Ski Lift (4)", "Ski Lift (2)", "Ski Lift (5)"];

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
						}
						else
						{
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

			foreach (var objPath in deleteObjectsList)
			{
				var objParentPath = "";

				int lastSlash = objPath.LastIndexOf('/');

				if (lastSlash != -1)
				{
					objParentPath = objPath[..lastSlash];
				}

				GameObject objParent = GameObject.Find(objParentPath);

				Transform objParentTransform = objParent?.transform;

				GameObject currentObject = null;
				var objName = objPath[(lastSlash + 1)..];

				if (objParentTransform != null)
				{
					var objTransform = objParentTransform.Find(objName);
					if (objTransform != null) currentObject = objTransform.gameObject;
				}
				else
				{
					currentObject = GameObject.Find(objName) ?? Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == objName);
				}

				if (currentObject != null)
				{
					UnityEngine.Object.Destroy(currentObject);
				}
				else
				{
					LoggerInstance.BigError($"Unable to find {objPath}, Could be from it not being loaded yet or from it not existing, please tell BobisBilly on Discord so he can fix it.");
				}
			}

			foreach (var skiLift in liftObjectsList)
			{
				var skiLiftsParent = GameObject.Find("World/Ski Lifts");

				if (skiLiftsParent != null)
				{
					var currentObject = skiLiftsParent.transform.Find(skiLift).gameObject;

					if (currentObject != null)
					{
						var skiLiftScript = currentObject.GetComponent<SkiLift>();

						if (skiLiftScript != null)
						{
							skiLiftScript.outOfOrder = false;
						}
						else
						{
							LoggerInstance.BigError($"World/Ski Lifts/{skiLift} SkiLift script couldn't be found, please tell BobisBilly on Discord so he can fix it.");
						}
					}
					else
					{
						LoggerInstance.BigError($"Unable to find World/Ski Lifts/{skiLift}, Could be from it not being loaded yet or from it not existing, please tell BobisBilly on Discord so he can fix it.");
					}
				}
			}

			base.OnLateInitializeMelon();
		}
	}
}