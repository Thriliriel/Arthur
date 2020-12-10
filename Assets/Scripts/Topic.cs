using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    author:  Victor Scherer Putrich
    propose: Manage dialogs about a commun topic
    info:    Keep DialogsGraph vector, runs Dialogs elements and manages when it has to change element
*/
public class Topic
{

    protected string _identificator;
    public List<Dialog> dialogs; //using to pick a random dialog 
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
    public string RunDialog(double p, List<string> memoryDialogs) {

        if (p != 0) { 
            currentDialog.NextSentence(p);
        }

        //if it is root
        if (currentDialog.GetId() == 0) currentDialog.NextSentence(p);

        //check if already used
        while (memoryDialogs.Contains(currentDialog.GetDescription() + currentDialog.GetId().ToString()))
        {
            //get next
            currentDialog.NextSentence(p);

            //check leaf, if so break
            if (currentDialog.DialogIsOver()) break;
        }

        if (currentDialog.DialogIsOver()) { CloseDialog(); return null; }

        return currentDialog.GetSentence();            

    }

    //search for other dialog routine
    public void StartNewDialog(){
        currentDialog = GetDialog();
        if (currentDialog != null)
        {
            ChangeState();
            currentDialog.StartDialog();
        }
    }

    //internal function to finish a dialog
    private void CloseDialog(){
        dialogs.Remove(currentDialog);
        currentDialog= null;
        ChangeState();
    }

    // choose new dialog
    // obs: next version it woundn´t be random dialog
    private Dialog GetDialog()
    {
        if(dialogs.Count == 0)
            return null;

        var rnd = new System.Random(DateTime.Now.Millisecond);
        int index = rnd.Next(0, dialogs.Count);
        
        Dialog d = dialogs[index];
        dialogs.Remove(d);
        return d;
    }

    private void ChangeState(){
        busy = !busy;
    }

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

    //for verification if a dialog is happening
    public bool IsDialoging(){ return busy; }
    public bool IsDialogsAvailable(){
        if (dialogs.Count != 0) return true;
        else if (dialogs.Count == 0 && !currentDialog.currentNode.IsLeaf()) return true;
        else return false;
    }
    public string GetId() { return _identificator; }
    public int GetLengthDialogs() { return dialogs.Count; }
    public Dialog GetCurrentDialog() { return currentDialog; }
    
    // Add new dialog
    public void InsertDialog(string dialogKey, Dialog d){
        dialogs.Add(d);
    }
    

}

/*
    \\MODELO TXT MEMÓRIA
    $ topic_name                                 -> novo tópico
    [ dialog_name                                -> novo diálogo
    # id, conteúdo, polaridade, éfolha, nodo pai -> nodo
    ]                                            -> fim diálogo

    $ música
    [ gosto_musical
    # 0;  "Me diz uma coisa, você gosta de ouvir música ?"; 0; 0; -1;
    # 1; "Hum, talvez você tenha descoberto o seu estilo, posso te recomendar algumas músicas ?"; -1; 0; 0;
    # 2; "Legal! Costuma ouvir música com frequência ?"; 1; 0; 0;
    # 3; "Puxa, tudo bem. Não está gostando muito do assunto ?"; -1; 0; 1; 
    # 4; "Eu costumo ouvir música pop, talvez devesse experimentar Justin Bieber."; 0; 1; 1;
    # 5; "Vamos lá.. eu te recomendo ouvir Broken Crown do 'Mumford and Sons', 'Fuego' do Alok.."; 1; 1; 1;
    # 6; "Te recomendo ouvir, é muito bom!"; -1; 1; 2;
    # 7; "Eu também! Melhor momento do dia é ouvir música no café da manhã!"; 1; 1; 2;
    # 8; "Sem problema, vamos conversar outra coisa."; -1; 1; 3;
    # 7; "Eu não vivo sem ouvir minhas música, talvez devesse procurar por algumas."; 1; 1; 3;
    ]

*/