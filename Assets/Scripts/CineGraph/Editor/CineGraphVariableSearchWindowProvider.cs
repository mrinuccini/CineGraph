using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace CineGraph.Editor
{
    public struct VariableSearchContextElement
    {
        public string Target { get; private set; }
        public string Path { get; private set; }

        public VariableSearchContextElement(string target, string path)
        {
            Target = target;
            Path = path;
        }
    }
    
    public class CineGraphVariableSearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
	    public static List<VariableSearchContextElement> s_elements;
	    public readonly CineGraphEditorView graph;
	    private readonly Action<string> m_callback;
	    private readonly Type m_variableType;

	    public CineGraphVariableSearchWindowProvider(CineGraphEditorView view, Action<string> callback, Type filterType)
	    {
		    graph = view;
		    m_callback = callback;
		    m_variableType = filterType;
	    }
	    
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> tree = new List<SearchTreeEntry> { new SearchTreeGroupEntry(new GUIContent("Variables"), 0) };

            s_elements = new List<VariableSearchContextElement>();

            CineGraphAsset asset = graph.GraphAsset;

            foreach (var variable in asset.Variables)
            {
	            if (variable.GetType() != m_variableType) continue;
	            
	            s_elements.Add(new VariableSearchContextElement(variable.GUID, variable.Name));
            }
            
			s_elements.Sort((entry1, entry2) => string.Compare(entry1.Path, entry2.Path, StringComparison.Ordinal));

			List<string> groups = new();
			
			foreach(VariableSearchContextElement element in s_elements)
			{
				var entry = new SearchTreeEntry(new GUIContent(element.Path))
				{
					level = 1,
					userData = element
				};
				tree.Add(entry);
			}

			return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
	        m_callback?.Invoke(((VariableSearchContextElement)searchTreeEntry.userData).Target);
	        return true;
        }
    }
}
