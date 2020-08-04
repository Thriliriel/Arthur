using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GeneralEvent
{
    //event information
    //unique ID
    public int informationID;
    //timestamp of the last remembrance
    public System.DateTime eventTime;
    //type of event (example: know new person, interaction, etc)
    public string eventType;
    //what is the event itself (example, what is the interaction exactly?)
    public string information;
    //emotion of this event
    public string emotion;
    //connected nodes, from memory
    public List<MemoryClass> nodes;
    //A positive polarity means that positive answers are expected ("i am good", "yes", and so on...)
    public float polarity;

    public GeneralEvent() { }

    public GeneralEvent(System.DateTime newMT, string newInformationType, string newInformation, int newInformationID, 
        string newEmotion)
    {
        eventTime = newMT;
        eventType = newInformationType;
        information = newInformation;
        informationID = newInformationID;
        emotion = newEmotion;
        nodes = new List<MemoryClass>();
    }
}
