using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogGraph
{
    /*
    author:  Victor Scherer Putrich
    propose: Run dialog procedure, receive polarized information and return answer/question/information
    info:    Walks throught nodes and consume its information
    obs:     The way its implemented is temporary and maybe LL be transformed into a state machine
    */

    /*
        List<>topicos;

        currentTopic = topics[0]
        func defineOQFazer()
        {
            if( isTalking){ ativouSM() }
        }

        ativouSM(polaridade){
            if currentTopic.isDialogging()
                currentTopic.nextContent()
            else
                currentTopic.StartNew..()
        }
    */

    public struct Node{
        private int _identification;
        private string _content;
        private List<Node> sons;
        private List<double> polarization;
        private bool _isLeaf;
        
        public Node(int id, string ct, bool lf){
            _identification = id;
            _content = ct;
            _isLeaf = lf;
            
            sons = new List<Node>();
            polarization = new List<double>(); 
        }

        public void AddSon(Node ch, double pol)
        {
            sons.Add(ch);
            polarization.Add(pol);
        }

        public int GetId(){ return _identification; }
        public string GetContent(){ return _content; }
        public List<Node> GetSons(){ return sons; }
        public List<double> GetPolarization(){ return polarization; }
        public bool isLeaf() { return _isLeaf; }
        
        /*
        public void SetId(int id){ _identification; }
        public int GetId(){ return _identification; }
        public void SetLeaf(bool il) { _isLeaf = il; }
        public void AddSon(Node ch, double pol)
        {
            _sons.Add(ch);
            _polarization.Add(pol);
        }
        public void AddSons(List<Node> sons){ _sons = sons; }
        public void AddPolarity(List<double> polarity){ _polarization = polarity; }
        public string GetContent(){ return _content; }
        public void SetContent(string ct){ _content = ct; }
        public List<Node> GetSons(){ return _sons; }
        public List<double> GetPolarization() { return _polarization;}
        public bool isLeaf(){ return _isLeaf; }
        */
    }

    private string _description;
    private Node root; //always empty and points to the first sentence
    private Node currentNode;

    Dictionary<int, Node> aux;


    public DialogGraph(string desc)
    {
        _description = desc;
        aux = new Dictionary<int, Node>();
    }

    //Initiate dialog
    public void StartDialog()
    {
        currentNode = root;
        //ReadCurrentNode();
    }

    //Update current node
    public void NextContent(double response){
        Debug.Log(response);
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

        currentNode = currentNode.GetSons()[nodeIndex];
        ReadCurrentNode();
            
        

        
    }

    public bool dialogIsOver()
    {
        return currentNode.isLeaf();
    }

    //read content from node
    private void ReadCurrentNode(){
        Debug.Log(currentNode.GetContent());
    }

    public string GetContent(){
        return currentNode.GetContent();
    }

    public int GetId()
    {
        return currentNode.GetId();
    }

    public string GetDescription() { return _description; }

    public void AddNode(int id, string content, double polarity, bool isLeaf, int fatherId)
    {
        Node newNode = new Node(id, content, isLeaf);

        if(id != 0)
        {
            Node fatherRef;
            aux.TryGetValue( fatherId, out fatherRef );
            fatherRef.AddSon( newNode, polarity );
        }
        aux.Add(newNode.GetId(), newNode);

    }

    public void CloseInsertion()
    {

        aux.TryGetValue(0, out root); //searching for root
        aux.Clear();
    }

}
