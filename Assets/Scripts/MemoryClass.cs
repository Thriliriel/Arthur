using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MemoryClass
{
    //memory information
    //unique ID
    public int informationID;
    //timestamp of the last remembrance
    public System.DateTime memoryTime;
    //type of information (5W1H)
    public string informationType;
    //information. If text, goes here. If image or audio, assumes the path where they are saved
    public string information;
    //activation, for memory decay and importance definition [0,1]
    public float activation;
    //weight (importance) of the memory [0,1]
    public float weight;
    //properties of the node
    /*public Dictionary<string, string> properties;
    //connected nodes
    public List<MemoryClass> nodes;*/

    public MemoryClass() {
        //nodes = new List<MemoryClass>();
        //properties = new Dictionary<string, string>();
    }

    public MemoryClass(System.DateTime newMT, string newInformationType, string newInformation, int newInformationID)
    {
        memoryTime = newMT;
        informationType = newInformationType;
        information = newInformation;
        activation = 1;
        informationID = newInformationID;
        weight = 0;
        //nodes = new List<MemoryClass>();
    }

    public MemoryClass(System.DateTime newMT, string newInformationType, string newInformation, int newInformationID, float newWeight)
    {
        memoryTime = newMT;
        informationType = newInformationType;
        information = newInformation;
        activation = 1;
        informationID = newInformationID;
        weight = newWeight;
        //nodes = new List<MemoryClass>();
    }
}
