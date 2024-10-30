using System;

namespace CineGraph
{
	/// <summary>
	///		Allows to defin custom variable types
	/// </summary>
	public class CineGraphVariableAttribute : Attribute
	{
		public string name;
		public string tooltip;

		public CineGraphVariableAttribute(string name, string tooltip) 
		{ 
			this.name = name;
			this.tooltip = tooltip;
		}
	}
}