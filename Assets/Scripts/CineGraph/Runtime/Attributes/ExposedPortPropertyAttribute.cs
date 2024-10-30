using System;

namespace CineGraph
{
	public enum PortType
	{
		Output = 0,
		Input
	}

	/// <summary>
	///		Allows to show a variable as a port and not as a field
	/// </summary>
	public class ExposedPortPropertyAttribute : Attribute
	{
		public PortType portType;
		public string tooltip;

		public ExposedPortPropertyAttribute(PortType portType, string tooltip)
		{
			this.portType = portType;
			this.tooltip = tooltip;
		}
	}
}
