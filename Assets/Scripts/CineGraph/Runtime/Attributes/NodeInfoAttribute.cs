using System;

namespace CineGraph
{
    /// <summary>
    ///     Defines a Node
    /// </summary>
    public class NodeInfoAttribute : Attribute
    {
        private string m_nodeTitle;
        private string m_menuItem;
        private bool m_hasFlowInput;
		private int m_numFlowOutput;
        private string m_customStyleSheet;
        private bool m_allowMultiples;

		public string title => m_nodeTitle;
        public string menuItem => m_menuItem;
        public bool hasFlowInput => m_hasFlowInput;
        public int numFlowOutput => m_numFlowOutput;
        public string customStyleSheet => m_customStyleSheet;
        public bool allowMultiples => m_allowMultiples;

        public NodeInfoAttribute(string title, string menuItem = " ", bool hasFlowInput=true, int numFlowOutput=1, string customStyleSheet=null, bool allowMultiples=true)
        {
            m_nodeTitle = title;
            m_menuItem = menuItem;
            m_hasFlowInput = hasFlowInput;
            m_numFlowOutput = numFlowOutput;
            m_customStyleSheet = customStyleSheet;
            m_allowMultiples = allowMultiples;
        }
    }
}
