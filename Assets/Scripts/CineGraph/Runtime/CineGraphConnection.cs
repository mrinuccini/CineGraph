namespace CineGraph
{
    /// <summary>
    ///     The connection between two nodes
    /// </summary>
    [System.Serializable]
    public class CineGraphConnection
    {
        public CineGraphConnectionPort inputPort;
        public CineGraphConnectionPort outputPort; 

        public CineGraphConnection(CineGraphConnectionPort input, CineGraphConnectionPort output)
        {
            inputPort = input; 
            outputPort = output;
        }

        public CineGraphConnection(string inputPortId, int inputIndex, string outputPortId, int outputIndex)
        {
            inputPort = new CineGraphConnectionPort(inputPortId, inputIndex);
            outputPort = new CineGraphConnectionPort(outputPortId, outputIndex);
        }
	}

    /// <summary>
    ///     The port of one node
    /// </summary>
	[System.Serializable]
	public class CineGraphConnectionPort
    {
        public string nodeId;
        public int portIndex;

        public CineGraphConnectionPort(string id, int index)
        {
            nodeId = id;
            portIndex = index;
        }
	}
}
