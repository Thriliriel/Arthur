using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialog
{
    /*
    author:  Victor Scherer Putrich
    propose: Run dialog procedure, receive polarized information and return answer/question/information
    info:    Walks throught nodes and consume its information
    obs:     The way its implemented is temporary and maybe LL be transformed into a state machine
    */
     public struct Node{
        private int identification;
        private string sentence;
        private string memoryEdge;
        private string memoryValue;
        private List<Node> children;
        private List<double> polarization;
        private bool isLeaf;
        
        public Node(int id, string st, bool lf, string edge, string value){
            identification = id;
            sentence = st;
            isLeaf = lf;
            memoryEdge = edge;
            memoryValue = value;
            children = new List<Node>();
            polarization = new List<double>(); 
        }

        public void AddChild(Node ch, double pol)
        {
            children.Add(ch);
            polarization.Add(pol);
        }

        public int GetId(){ return identification; }
        public string GetSentence(){ return sentence; }
        public string GetMemoryEdge(){ return memoryEdge; }
        public string GetMemoryValue() { return memoryValue; }
        public List<Node> GetChildren(){ return children; }
        public List<double> GetPolarization(){ return polarization; }
        public bool IsLeaf() { return isLeaf; }
        
    }

    private string _description;
    private Node root;
    public Node currentNode;

    Dictionary<int, Node> aux;


    public Dialog(string desc)
    {
        _description = desc;
        aux = new Dictionary<int, Node>();
    }

    //Initiate dialog
    public void StartDialog()
    {
        currentNode = root;
    }

    //Update current node
    public void NextSentence(double response){
        /* response value between -1 and 1 */
        List<double> tmp = currentNode.GetPolarization();
        int nodeIndex = 0;
        double closestDist = tmp[0];
            
        //get the closest distance between response and polarization´s vector
        for(int i = 0; i < tmp.Count; i++){
            double dist = Math.Abs(tmp[i] - response);
            if(dist < closestDist || (dist == closestDist && tmp[i] > tmp[nodeIndex]))
            {
                closestDist = dist;
                nodeIndex = i;
            }
                
        }

        currentNode = currentNode.GetChildren()[nodeIndex];
        ReadCurrentNode();        
    }

    public bool DialogIsOver(){ return currentNode.IsLeaf();}
    public string GetSentence(){ return currentNode.GetSentence(); }
    public int GetId(){ return currentNode.GetId();}
    public Tuple<string, string> GetMemoryData(){ return new Tuple<string, string>(currentNode.GetMemoryEdge(), currentNode.GetMemoryValue()); }
    public string GetDescription() { return _description; }

    //used to build de dialog
    public void AddNode(int id, string content, double polarity, bool isLeaf, int fatherId, string memEdge, string memValue)
    {
        Node newNode = new Node(id, content, isLeaf, memEdge, memValue);

        if(id != 0)
        {
            Node fatherRef;
            aux.TryGetValue( fatherId, out fatherRef );
            fatherRef.AddChild( newNode, polarity );
        }
        aux.Add(newNode.GetId(), newNode);

    }

    //after build dialogs root, root gets dialog 0
    public void CloseInsertion()
    {

        aux.TryGetValue(0, out root); //searching for root
        aux.Clear();
    }

    //read content from node
    private void ReadCurrentNode(){
        Debug.Log(currentNode.GetSentence());
    }
}
