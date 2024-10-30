using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CineGraph.Editor
{
	public struct SearchContextElement
	{
		public object target { get; private set; }
		public string menuItem { get; private set; }

		public SearchContextElement(object target, string menuItem)
		{
			this.target = target;
			this.menuItem = menuItem;
		}
	}

	public class CineGraphWindowSearchProvider : ScriptableObject, ISearchWindowProvider
	{
		public CineGraphEditorView graph;
		public VisualElement target;

		public static List<SearchContextElement> elements;
		
		public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
		{
			List<SearchTreeEntry> tree = new List<SearchTreeEntry>();

			tree.Add(new SearchTreeGroupEntry(new GUIContent("Nodes"), 0));

			elements = new();
			
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			
			foreach (Assembly assembly in assemblies)
			{
				foreach(Type type in assembly.GetTypes())
				{
					if (type.CustomAttributes.ToList() == null) continue;
					
					var attribute = type.GetCustomAttribute(typeof(NodeInfoAttribute));

					if (attribute == null) continue;

					NodeInfoAttribute att = attribute as NodeInfoAttribute;
					var node = Activator.CreateInstance(type);

					if (string.IsNullOrEmpty(att.menuItem)) continue;
					
					elements.Add(new(node, att.menuItem));
				}
			}
			
			elements.Sort((entry1, entry2) =>
			{
				string[] splits1 = entry1.menuItem.Split("/");
				string[] splits2 = entry2.menuItem.Split("/");

				for (int i = 0; i < splits1.Length; i++)
				{
					if (i >= splits2.Length) return 1;

					int value = splits1[i].CompareTo(splits2[i]);

					if (value == 0) continue;

					if(splits1.Length != splits2.Length && (i == splits1.Length - 1 || i == splits2.Length - 1))
						return splits1.Length < splits2.Length ? 1 : -1;

					return value;
				}

				return 0;
			});

			List<string> groups = new();
			
			foreach(SearchContextElement element in elements)
			{
				string[] entryTitle = element.menuItem.Split("/");

				string groupName = "";

				for (int i = 0;i < entryTitle.Length - 1;i++)
				{
					groupName += entryTitle[i];
					
					if (!groups.Contains(groupName))
					{
						tree.Add(new SearchTreeGroupEntry(new GUIContent(entryTitle[i]), i + 1));
						groups.Add(groupName);
					}

					groupName += "/";
				}

				SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(entryTitle.Last()));
				entry.level = entryTitle.Length;
				entry.userData = element;
				tree.Add(entry);
			}

			return tree;
		}

		public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
		{
			var windowMousePosition = graph.ChangeCoordinatesTo(graph, context.screenMousePosition - graph.window.position.position);
			var graphMousePosition = graph.contentViewContainer.WorldToLocal(windowMousePosition);

			SearchContextElement element = (SearchContextElement)SearchTreeEntry.userData;

			CineGraphNode node = (CineGraphNode)element.target;
			node.SetPosition(new Rect(graphMousePosition, new Vector2()));
			graph.AddNode(node);

			return true;
		}
	}
}
