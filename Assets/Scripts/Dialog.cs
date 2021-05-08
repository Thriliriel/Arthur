using System;
using System.Collections;
using System.Collections.Generic;

using System.Text.RegularExpressions;

using WeightWords = System.Collections.Generic.Dictionary<string, double>; //<keyword, weight>

namespace DialogCS
{
    public class Dialog
    {
        /*
        author:  Victor Scherer Putrich
        propose: Run dialog procedure, receive polarized information and return answer/question/information
        info:    Walks throught nodes and consume its information
        obs:     The way its implemented is temporary and maybe LL be transformed into a state machine
        */
        public struct Node
        {
            private string identification;
            private string sentence;
            private Dictionary<string, WeightWords> children; //< child_id, <..> >

            public Node(string id, string st)
            {
                identification = id;
                sentence = st;
                children = new Dictionary<string, WeightWords>();
            }

            public void AddChild(string childId)
            {
                if (!children.ContainsKey(childId))
                {
                    children[childId] = new WeightWords();
                }
                else
                {
                    throw new ArgumentException("Child ID already used at this node! (internal error)", nameof(childId));
                }
            }

            public void AddKeyword(string childId, string keyword, double weight)
            {

                WeightWords value;

                if (!children.TryGetValue(childId, out value)) { throw new ArgumentException("Child ID not found! (internal error)", nameof(childId)); }

                else if (value.ContainsKey(keyword) /*value[childId].ContainsKey(keyword)*/ ) { throw new ArgumentException("Keyword " + keyword + " already used in " + childId + ". Keyword must be unique for each ID!"); }

                children[childId][keyword] = weight;
            }

            public string GetId() { return identification; }
            public string GetSentence() { return sentence; }
            public Dictionary<string, WeightWords> GetChildren() { return children; }
            public bool IsLeaf() { return children.Count == 0;/*return isLeaf;*/ }
            public void ResetChildren() { children.Clear(); }

        }

        private string _description;
        private Node root;
        private Node currentNode;
        private Node exitNode = new Node("None", "Im sorry but I had dificulty to understand what you said, lets talk about another thing."); //when Arthur dont know what to say

        Dictionary<string, Node> nodes;


        public Dialog(string desc)
        {
            _description = desc;
            nodes = new Dictionary<string, Node>();
        }

        //Initiate dialog
        public void StartDialog()
        {
            currentNode = root;
        }


        // --> eu gostava de pizza
        // H: <id> eu gostava de pizza --> EU GOSTO
        //tokenlist: [eu, gosto, pizza]
        public void NextSentence(List<string> tokensList)
        {
            var childrenCount = new Dictionary<string, double>();


            foreach (string childId in currentNode.GetChildren().Keys)
            {
                if (childrenCount.ContainsKey(childId))
                {
                    throw new ArgumentException("Child ID already inserted!", nameof(childId));
                }
                childrenCount[childId] = 0;
            }

            foreach (KeyValuePair<string, WeightWords> childKwLst in currentNode.GetChildren())
            {
                foreach (string keyword in tokensList)
                {
                    double value;
                    bool keyExists = (childKwLst.Value).TryGetValue(keyword, out value);
                    if (keyExists)
                    {
                        childrenCount[childKwLst.Key] += value;
                    }
                }
            }

            double higher = 0;
            string higherId = "";

            //instantiate new keywords
            foreach (KeyValuePair<string, double> cc in childrenCount)
            {
                if (higher < cc.Value)
                {
                    higher = cc.Value;
                    higherId = cc.Key;
                }
            }

            bool foundKey = nodes.TryGetValue(higherId, out currentNode);
            
            if(higherId == "") currentNode = exitNode; // VICTOR COMMENT: here you can choose a rule to take everytime arthur not find a node with higher ponctuation (maybe polarity)
            if (!foundKey) throw new ArgumentException("Next utterance not found, supose to be: " + higherId, nameof(higherId));

            ReadCurrentNode();

        }

        /*
        private int SearchKeyword(string sentence){
            sentence = Regex.Replace(sentence.Trim().ToUpper(), @"[^0-9a-zA-Z ]+", string.Empty);
            string[] data = sentence.Split(' ');
            int[] matches = new int[currentNode.GetChildren().Count]; //list update


            //counting the index matching for each branch in current node
            var dic = currentNode.GetKeyWord();
            int nodeIndex = -1;
            for(int i = 0; i < data.Length; i++)
            {
                try
                { 
                    //nodeIndex = tmp[data[i]]; 
                    var indexes = dic[data[i]];
                    foreach(int ind in indexes){ 
                        Debug.Log("Match: " + data[i] + " | ID=" + ind);
                        matches[ind]++; 
                    } //list update

                }
                catch (KeyNotFoundException){ continue; }
            }


            //verify if there is a higher one
            int indexH = 0;
            int valueH = 0; 
            bool foundMatch = false;
            for(int i=0; i < matches.Length; i++)
            {
                if(matches[i] > valueH) 
                {
                    Debug.Log("Higher Match[" + i + "]=" + matches[i]);
                    foundMatch = true;
                    indexH = i;
                    valueH = matches[i];
                }
            }

            if(foundMatch)
                nodeIndex = indexH;

            return nodeIndex;
        }
        */

        public bool DialogIsOver() { return currentNode.IsLeaf(); }
        public string GetSentence() { return currentNode.GetSentence(); }
        public string GetId() { return currentNode.GetId(); }
        public string GetDescription() { return _description; }

        //used to build de dialog
        public void AddNode(string id, string content, string fatherId)
        {

            if (nodes.ContainsKey(id)) throw new ArgumentException("ID already inserted " + id + " must be unique!", nameof(id));

            Node newNode = new Node(id, content);

            if (fatherId != "-1")
            {
                Node fatherRef;
                if (nodes.TryGetValue(fatherId, out fatherRef))
                {
                    fatherRef.AddChild(id);
                    nodes[id] = newNode;
                }
                else { throw new ArgumentException("node ID " + fatherId + " dont exist or should be created before ID " + id, nameof(fatherId)); }

            }
            else //root
            {
                nodes[id] = newNode;
                nodes.TryGetValue(id, out root); //searching for root
            }


        }

        public void AddKeywords(string id, List<Tuple<string, double>> keywordsList, string fatherId)
        {
            if (fatherId != "-1")
            {

                if (!nodes.ContainsKey(fatherId))
                {
                    throw new ArgumentException("node ID not found: " + fatherId, nameof(fatherId));
                }
                if (!nodes.ContainsKey(id))
                {
                    throw new ArgumentException("node ID not found: " + id, nameof(id));
                }

                Node fatherRef = nodes[fatherId];

                foreach (Tuple<string, double> kw in keywordsList)
                {

                    fatherRef.AddKeyword(id, kw.Item1, kw.Item2);
                }
            }
        }

        /*
        //after build dialogs root, root gets dialog 0
        public void CloseInsertion()
        {

        }
        */


        //read content from node
        private void ReadCurrentNode()
        {
            //Debug.Log(currentNode.GetSentence());
        }

        public void Done()
        {
            currentNode.ResetChildren();
        }

    }
}
