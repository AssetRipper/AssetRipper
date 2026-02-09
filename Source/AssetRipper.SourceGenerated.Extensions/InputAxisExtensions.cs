using AssetRipper.SourceGenerated.Subclasses.InputAxis;

namespace AssetRipper.SourceGenerated.Extensions;

public static class InputAxisExtensions
{
	public enum InputAxisType
	{
		/// <summary>
		/// Actually it is any button, not just keyboard or mouse
		/// </summary>
		KeyOrMouseButton = 0,
		MouseMovement = 1,
		JoystickAxis = 2,
	}
	public enum JoystickType
	{
		AllJoysticks = 0,
		Joystick1 = 1,
		Joystick2 = 2,
		Joystick3 = 3,
		Joystick4 = 4,
		Joystick5 = 5,
		Joystick6 = 6,
		Joystick7 = 7,
		Joystick8 = 8,
		Joystick9 = 9,
		Joystick10 = 10,
		Joystick11 = 11,
		Joystick12 = 12,
		Joystick13 = 13,
		Joystick14 = 14,
		Joystick15 = 15,
		Joystick16 = 16,
	}
	public enum InputAxesDirection
	{
		X = 0,
		Y = 1,
		ScrollWheel = 2,
		_4 = 3,
		_5 = 4,
		_6 = 5,
		_7 = 6,
		_8 = 7,
		_9 = 8,
		_10 = 9,
		_11 = 10,
		_12 = 11,
		_13 = 12,
		_14 = 13,
		_15 = 14,
		_16 = 15,
		_17 = 16,
		_18 = 17,
		_19 = 18,
		_20 = 19,
		_21 = 20,
		_22 = 21,
		_23 = 22,
		_24 = 23,
		_25 = 24,
		_26 = 25,
		_27 = 26,
		_28 = 27,
	}
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
		input.Name = name;
		input.PositiveButton = positive;
		input.AltPositiveButton = altPositive;
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
