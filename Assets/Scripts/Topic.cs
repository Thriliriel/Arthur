using System;
using System.Collections;
using System.Collections.Generic;

using DialogCS;
/*
    author:  Victor Scherer Putrich
    propose: Manage dialogs about a commun topic
    info:    Keep DialogsGraph vector, runs Dialogs elements and manages when it has to change element
*/

/*
     ### dialog ID format ###
    
    size must be 9:
         -3 letter from topic uppercase
         -2 from dialogue lowercase
         -2 from node number (for each dialogue tree)
         -2 from tree level (for each dialogue)
        
    example:
        topic: FOOD
        dialogue: FAVORITE_FOOD
        node counter: 03
        tree level: 02
        
        possible dialog_id = FODff0302   
     
     obs: IDs must be unique!
        
     ### dialogue tree format ###
    
     $ <topic_name>
     [ <dialog_name>                               --open dialogue
     # <dialog_id>, <utterance>, <dialog_id_father>
     ...
     ]                                             --close dialolgue
*/

namespace TopicCS
{
    public class Topic
    {

        protected string _identificator;
        protected List<Dialog> dialogs; //using to pick a random dialog 
        protected bool busy; //dialog is running

        Dialog currentDialog;

        public Topic(string id)
        {

            _identificator = id;
            dialogs = new List<Dialog>();
            busy = false;

        }

        //run get next dialog node
        //if its over, finish dialog to sort another one
        //public string RunDialog(double p, string sentence, List<string> memoryDialogs){
        public string RunDialog(double p, List<string> tokenizeSentence, List<string> memoryDialogs)
        {
            if (currentDialog.DialogIsOver())
            {
                CloseDialog();
                return null;
            }

            if (p != 0)
                currentDialog.NextSentence(tokenizeSentence);

            //check if already used
            //UnityEngine.Debug.Log(_identificator + "-" + currentDialog.GetDescription() + "-" + currentDialog.GetId().ToString());
            while (memoryDialogs.Contains(_identificator + "-" + currentDialog.GetDescription() + "-" + currentDialog.GetId().ToString()))
            {
                //get next
                //currentDialog.NextSentence(p, sentence);
                currentDialog.NextSentence(tokenizeSentence);

                //check leaf, if so break
                if (currentDialog.DialogIsOver()) break;
            }

            return currentDialog.GetSentence();
        }

        //search for other dialog routine
        public void StartNewDialog()
        {
            currentDialog = GetDialog();
            if (currentDialog != null)
            {
                ChangeState();
                currentDialog.StartDialog();
            }
        }

        //internal function to finish a dialog
        public void CloseDialog()
        {
            dialogs.Remove(currentDialog);
            //ChangeState();
            busy = false;
        }

        // choose new dialog
        // obs: next version it woundn´t be random dialog
        private Dialog GetDialog()
        {
            if (dialogs.Count == 0)
                return null;

            var rnd = new System.Random(DateTime.Now.Millisecond);
            int index = rnd.Next(0, dialogs.Count);

            Dialog d = dialogs[index];
            dialogs.Remove(d);
            return d;
        }

        private void ChangeState()
        {
            busy = !busy;
        }

        /*
        //Current dialog relevant information to save in Arthur´s memory
        public string[] GetCDInfo(){
            string[] info =  new string[4];
            Tuple<string, string> memData = currentDialog.GetMemoryData();
            info[0] = currentDialog.GetId().ToString(); //from node
            info[1] = memData.Item1;    //from node
            info[2] = memData.Item2;   //from node
            info[3] = currentDialog.GetDescription();   //from dialog - repeat for every node at the same dialog
            return info;
        }
        */

        //for verification if a dialog is happening
        public bool IsDialoging() { return busy; }
        public bool IsDialogsAvailable() { return dialogs.Count != 0; }
        public string GetId() { return _identificator; }
        public int GetLengthDialogs() { return dialogs.Count; }
        public Dialog GetCurrentDialog() { return currentDialog; }

        // Add new dialog
        public void InsertDialog(string dialogKey, Dialog d)
        {
            dialogs.Add(d);
        }
    }
}