using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[System.Serializable]
public class IceBreakingTreeClass
{
    private int id;
    private string type;
    private string question;
    private IceBreakingTreeClass parent;
    private List<IceBreakingTreeClass> child;
    //if the polarity is true, it means positive (and vice-versa).
    //A positive polarity means that positive answers are expected ("i am good", "yes", and so on...)
    private bool polarity;

    public IceBreakingTreeClass(int newId) { id = newId; }

    public IceBreakingTreeClass(int newId, string newType, string newQuestion, bool newPolarity)
    {
        id = newId;
        type = newType;
        question = newQuestion;
        child = new List<IceBreakingTreeClass>();
        polarity = newPolarity;
    }

    public IceBreakingTreeClass(int newId, string newType, string newQuestion, IceBreakingTreeClass newParent, List<IceBreakingTreeClass> newChild)
    {
        id = newId;
        type = newType;
        question = newQuestion;
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

    public string GetType()
    {
        return type;
    }

    public string GetQuestion()
    {
        return question;
    }

    public bool GetPolarity()
    {
        return polarity;
    }

    public void AddChild(IceBreakingTreeClass newChild)
    {
        newChild.SetParent(this);
        child.Add(newChild);
    }

    public void SetParent(IceBreakingTreeClass newParent)
    {
        parent = newParent;
    }

    public IceBreakingTreeClass GetParent()
    {
        return parent;
    }

    public IceBreakingTreeClass GetChild(int index)
    {
        if (child.Count <= index) return null;
        else return child[index];
    }

    //go down the tree to find the searched id
    /*public IceBreakingTreeClass FindIcebreaker(int searchId, IceBreakingTreeClass caller)
    {
        if (id == searchId) return this;
        else if (child.Count > 0)
        {
            foreach(IceBreakingTreeClass itc in child)
            {
                return caller.FindIcebreaker(itc.GetId(), caller);
            }

            return null;
        }
        else return null;
    }*/

    public IceBreakingTreeClass FindIcebreaker(int searchId)
    {
        IceBreakingTreeClass foundIt = null;

        if (id == searchId) foundIt = this;
        else
        {
            foreach(IceBreakingTreeClass chdn in child)
            {
                if (chdn.GetId() == searchId)
                {
                    foundIt = chdn;
                    break;
                }
                //else, where its children also
                else
                {
                    IceBreakingTreeClass it = chdn;
                    while (it.child.Count > 0)
                    {
                        it = it.child[0];
                        if (it.GetId() == searchId)
                        {
                            foundIt = it;
                            break;
                        }
                    }

                    if(foundIt != null)
                    {
                        break;
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
