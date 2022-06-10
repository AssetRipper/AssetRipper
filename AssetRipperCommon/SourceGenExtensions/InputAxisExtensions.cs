using AssetRipper.Core.Classes.InputManager;
using AssetRipper.SourceGenerated.Subclasses.InputAxis;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class InputAxisExtensions
	{
		public static InputAxisType GetType(this IInputAxis input)
		{
			return (InputAxisType)input.Type;
		}

		public static InputAxesDirection GetAxis(this IInputAxis input)
		{
			return (InputAxesDirection)input.Axis;
		}

		public static JoystickType GetJoyNum(this IInputAxis input)
		{
			return (JoystickType)input.JoyNum;
		}

		public static void Initialize(this IInputAxis input, string name, string positive, string altPositive)
		{
			input.Name.String = name;
			input.PositiveButton.String = positive;
			input.AltPositiveButton.String = altPositive;
			input.Gravity = 1000.0f;
			input.Dead = 0.001f;
			input.Sensitivity = 1000.0f;
			input.Snap = false;
			input.Invert = false;
			input.Type = (int)InputAxisType.KeyOrMouseButton;
			input.Axis = (int)InputAxesDirection.X;
			input.JoyNum = (int)JoystickType.AllJoysticks;
		}
	}
}
