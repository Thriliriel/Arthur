using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[System.Serializable]
public class SmallTalkClass
{
    private int id;
    private string sentence;
    private SmallTalkClass parent;
    private List<SmallTalkClass> child;
    //if the polarity is true, it means positive (and vice-versa).
    //A positive polarity means that positive answers are expected ("i am good", "yes", and so on...)
    private bool polarity;

    public SmallTalkClass(int newId) { id = newId; }

    public SmallTalkClass(int newId, string newsentence, bool newPolarity)
    {
        id = newId;
        sentence = newsentence;
        child = new List<SmallTalkClass>();
        polarity = newPolarity;
    }

    public SmallTalkClass(int newId, string newsentence, SmallTalkClass newParent, List<SmallTalkClass> newChild)
    {
        id = newId;
        sentence = newsentence;
        parent = newParent;
        child = newChild;
    }

    public int GetId()
    {
        return id;
    }

    public void SetId(int newId)
    {
        id = newId;
    }

    public string Getsentence()
    {
        return sentence;
    }

    public bool GetPolarity()
    {
        return polarity;
    }

    public void AddChild(SmallTalkClass newChild)
    {
        newChild.SetParent(this);
        child.Add(newChild);
    }

    public void SetParent(SmallTalkClass newParent)
    {
        parent = newParent;
    }

    public SmallTalkClass GetParent()
    {
        return parent;
    }

    public SmallTalkClass GetChild(int index)
    {
        if (child.Count <= index) return null;
        else return child[index];
    }

    //just go 2 lvls down
    public SmallTalkClass FindSmallTalk(int searchId)
    {
        SmallTalkClass foundIt = null;

        if (id == searchId) foundIt = this;
        else
        {
            foreach(SmallTalkClass son in child)
            {
                if(son.GetId() == searchId)
                {
                    foundIt = son;
                    break;
                }
                else
                {
                    if (son.QntChildren() > 1)
                    {
                        if (son.GetChild(0).GetId() == searchId)
                        {
                            foundIt = son.GetChild(0);
                            break;
                        }
                        else if (son.GetChild(1).GetId() == searchId)
                        {
                            foundIt = son.GetChild(1);
                            break;
                        }
                        else
                        {
                            if (son.GetChild(0).QntChildren() > 1)
                            {
                                if (son.GetChild(0).GetChild(0).GetId() == searchId)
                                {
                                    foundIt = son.GetChild(0).GetChild(0);
                                    break;
                                }
                                else if (son.GetChild(0).GetChild(1).GetId() == searchId)
                                {
                                    foundIt = son.GetChild(0).GetChild(1);
                                    break;
                                }
                            }else if (son.GetChild(1).QntChildren() > 1) {
                                if (son.GetChild(1).GetChild(0).GetId() == searchId)
                                {
                                    foundIt = son.GetChild(1).GetChild(0);
                                    break;
                                }
                                else if (son.GetChild(1).GetChild(1).GetId() == searchId)
                                {
                                    foundIt = son.GetChild(1).GetChild(1);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        return foundIt;
    }

    //go up the tree and check which child this one is (0, 1, 2...)
    public int CheckWhichChild(int rootParent = 0)
    {
        if(parent.GetId() == rootParent)
        {
            int hereItIs = -1;
            for(int i = 0; i < parent.child.Count; i++)
            {
                if(parent.child[i].GetId() == id)
                {
                    hereItIs = i;
                    break;
                }
            }
            return hereItIs;
        }
        else
        {
            return parent.CheckWhichChild();
        }
    }

    //qnt of children
    public int QntChildren()
    {
        return child.Count;
    }
}
