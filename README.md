# CineGraph
## About
CineGraph is a tool that allows to create custom node based editor in the Unity Engine. It offers the base foundation so you can just focus on building nodes and functionnalities. As it's based on Unity GraphView API it can be very easily extended if the base tool doesn't fit your needs.

![CineGraph Example](https://i.imgur.com/w9w4VXd.png, "Example use of CineGraph")

## System Requirements
CineGraph was built on **Unity 2022LTS** but is compatible with any versions that includes the **Graph View API**
We still recommend using **Unity 2022LTS** or later to avoid any bugs.

## Install
1. Download the latest CineGraph version from this github repository.
2. Import the package in your Unity Project
3. That's it ! CineGraph is now ready to go

# Documentation
## Getting started
### Creating a new asset
* Any graph you create will be stored in a CineGraph asset. You can create a CineGraph asset in Unity by right clicking -> Create new -> CineGraph -> Graph. This will create a new CineGraph Asset in your current working directory. 

* You can them customize base data like the graph's name.

### Opening an asset
You can open a CineGraph asset either by :
* Double clicking on the asset in the editor
* Clicking on the "Open" button when inspecting an asset

## Graph Window Overview
The Graph Window is composed of multiple parts
![CineGraph Overview](https://i.imgur.com/4jLrFXZ.png, "Window Overview")
### Blackboard
The blackboard allows to manage the variables that you will create
### Node Creation Window
The node creation window allows you to add any nodes to your graph

## Creating a custom Node
### Basics
You can create a new CineGraph Node by creating a new C# class and importing the CineGraph namespace.
You must then make the class inherit from CineGraphNode and add the NodeInfo Attribute. The NodeInfo attribute requires you give your node a title and preferably a path on the node creation window.
```
using CineGraph;

[NodeInfo("My node", menuItem: "Nodes/My Node")]
class MyNode : CineGraphNode
{
    
}
```
You can then override the OnProcess method like so :
```
public override IEnumerator OnProcess(CineGraphAsset graphInstance, CineGraphRunner runner, CineGraphProcess process)
{
    // Some stuff to do 
        
     yield return base.OnProcess(graphInstance, runner, process);
}
```
Also note how the OnProcess method is a coroutine. The **On Process** method takes several arguments :
* graphInstance : an instance of the CineGraph Asset that is running
* runner : a reference to the script that initiated the CineGraphProcess
* process : a reference to the process this node is running from

### Initisalisation
In case you need to run stuff before the graph actually starts to run you can override the **Initialise** method which is run when the graph gets loaded :
```
public override void Initialise(CineGraphAsset asset, CineGraphRunner runner)
{
    // Initialisation code
}
```
Please note at how there is no reference to the CineGraphProcess in this method. This is because the process hasn't yet been created when this method is runned

### Properties
You also add properties to your nodes that can be modified from the graph GUI. There are two types of property in CineGraph
#### Exposed Properties
An exposed property is a property like in the inspector whom value can be modified trough a field GUI. Here is an example :
```
[ExposedProperty] public bool myProperty;
```
This gives the following result :
![Exposed property result](https://i.imgur.com/d2Mv01Z.png)
#### Exposed port properties
Exposed port properties are properties that are part of the flow and cannot be modified with an inspector like GUI. They are declared as the following :
```
[ExposedPortProperty(portType: PortType.Output, tooltip: "Help")] public bool myPortProperty;
```
Where :
* portType is the direction of the property (Input/Output)
* tooltip is just a text that shows up when the user hover over the property

This gives the following result : 
![Exposed port property result](https://i.imgur.com/GOJSnOi.png)

### Extending the NodeInfo Attribute
The **Node Info** attribute features other parameters as suggested by its definition
```
public NodeInfoAttribute(string title, string menuItem = " ", bool hasFlowInput=true, int numFlowOutput=1, string customStyleSheet=null, bool allowMultiples=true)
```
* **title** : The Name of the node
* **menuItem** : the path to the node on the creation window
* **hasFlowInput** : Does the node allow a flow input ?
* **numFlowOutput**  : How many flow output should the node have ?
* **customStyleSheet** : allows you to link a custom stylesheet for your node
* **allowMultiples** : Should this node be present multiple time in the graph ?

### Styling
#### Including a custom stylesheet
You can include a custom stylesheet trough the NodeInfo Attribute. A node possesses by default multiple classes base on its name and path. Let's suppose :
* **Name** : My Node
* **Path** : Path/To/My Node

The classes will be : "**.my-node**", "**.path**", "**.to**"
#### Extending the Node UI
To extend the node UI you can add the **ICineGraphCustomNodeUI** interface to your node class
Your code should like this : 
```
using CineGraph;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif

[NodeInfo("My node", menuItem: "Nodes/My Node")]
class MyNode : CineGraphNode, ICineGraphCustomNodeUI
{
    #if UNITY_EDITOR
    public void BuildCustomUI(Node node)
    {
        node.contentContainer.Add(new Label("Custom UI example"));
    }
    #endif
}

```
Please note that you must put this in *#if UNITY_EDITOR* to avoid build errors because Unity doesn't include editor stuff.
This gives the result :
![Custom UI result](https://i.imgur.com/51h7nlf.png)
# Running a graph
To run a graph, take any object on your scene and add the **CineGraphRunner** script to it. Then, from any script on your scene reference this instance of CineGraphRunner and call the method **CineGraphRunner.LoadAsset(CineGraphAsset graphAsset)** to load in an asset.

You can then run the asset using multiple methods :
* **Run**
> This runs the graph from the **OnInteractNode**
* **RunFromNode(T)()**
> This runs the graph from the first node of type **T** that was found
* **RunFromNode(string guid)**
> This runs the graph from the node with the specific **guid**

All of these methods return a **CineGraphProcess** instance that allows you to manage the execution in realtime
# Custom Variables 
you can create custom variables that could be instantiated in the blackboard using the CineGraphVariable class
## Creating a new variable type  
To do this, create a new class and make it extend from **CinegraphVariable**. Don't forget to import the CineGraph namespace. Also add the **CineGraphVariable** attribute to your class. Your code should look like this :
```
using CineGraph;

[CineGraphVariable(name: "My Variable", tooltip: "This is my variable")]
public class MyVariableType : CineGraphVariable
{
    public int myData;
    
    public MyVariableType() : base("", "") { }
    public MyVariableType(string name, string guid): base(name, guid) {  }
}
```
Please note that **CineGraphVariable** requires **two** default constructors.

You can now create new instances of your variable from the blackboard. This gives the result :

![Custom variable result](https://i.imgur.com/at4oCgD.png)
## Custom variable GUID
In order to modify data from our variable in the editor we must also create a custom UI. This is fairly similar to the node custom UI process as we need to include the **ICineGraphCustomVariableUI** interface like this :
```
using CineGraph;
using UnityEngine.UIElements;

[CineGraphVariable("My Variable", tooltip: "This is my variable")]
public class MyVariableType : CineGraphVariable, ICineGraphCustomVariableUI
{
    public int myData = 0;
    
    public MyVariableType() : base("", "") { }
    
    public MyVariableType(string name, string guid): base(name, guid) {  }

    public void BuildCustomUI(VisualElement parent)
    {
        var field = new TextField()
        {
            name = "",
            value = myData.ToString()
        };
        
        field.RegisterValueChangedCallback(x =>
        {
            myData = int.Parse(x.newValue);
        });
        
        parent.Add(field);        
    }
}
```
this gives the following result :
![Result](https://i.imgur.com/cs6b5Mr.png)
## Using the variable on the graph
To add the variable to the graph you can middle mouse click on it in the blackboard and this will instantiate it on the graph. You can then use it with **ExposedPortPropety** like this :
```
[NodeInfo("My node", menuItem: "Nodes/My Node")]
class MyNode : CineGraphNode, ICineGraphCustomNodeUI
{
    [ExposedPortProperty(PortType.Input, "tooltip")] public CineGraphVariable variable;
    
    public override IEnumerator OnProcess(CineGraphAsset graphInstance, CineGraphRunner runner, CineGraphProcess process)
    {
        if(variable is MyVariableType type) Debug.Log(type.myData); 
        
        yield return base.OnProcess(graphInstance, runner, process);
    }
}
```
This gives the result :
![result](https://i.imgur.com/xCVRyGw.png)
# Moving Forward
CineGraph has many more methods and  but hopefully this guide gave you enough of a base to get started. Most of the CineGraph methods are documented in the code or self explanatory. Have fun playing with it !

# Roadmap
As this tool was made for a personal game project  I don't think if I'll update it if there  are no breaking bugs.