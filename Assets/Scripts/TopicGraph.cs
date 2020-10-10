using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    author:  Victor Scherer Putrich
    propose: Manage dialogs about a commun topic
    info:    Keep DialogsGraph vector, runs Dialogs elements and manages when it has to change element
*/
public class TopicGraph
{

    protected string _identificator;
    protected Dictionary<string, DialogGraph> dialogNodes;
    protected List<DialogGraph> dialogs; //using to pick a random dialog ( remove it after graph implementation )
    protected bool _isDialoging;
    
    DialogGraph currentDialog;
    const int debugLevel = -1;
    
    //PRIMEIRA VERSÃO: gera Nodos e vai removendo de forma aleatório em um vetor
    public TopicGraph(string id /*closed dialogs*/)
    {
        if(debugLevel >= 0)
            Debug.Log("ok, im here");
        
        _identificator = id;
        dialogNodes = new Dictionary<string, DialogGraph>();
        dialogs = new List<DialogGraph>();
        _isDialoging = false;

    }

    //run get next dialog node
    //if its over, finish dialog to sort another one
    public string RunDialog(double p){

        currentDialog.NextContent(p);

        if (currentDialog.dialogIsOver()) { EndDialog(); return null; }

        return currentDialog.GetContent();            

    }

    //search for other dialog routine
    public void StartNewDialog(){
        currentDialog = GetDialog();
        ChangeState();
        currentDialog.StartDialog();
        
    }

    //internal function to finish a dialog
    private void EndDialog(){
        dialogs.Remove(currentDialog);
        currentDialog= null;
        ChangeState();
    }

    // choose new dialog
    // obs: next version it woundn´t be random dialog
    private DialogGraph GetDialog()
    {
        if(dialogs.Count == 0)
            return null;

        var rnd = new System.Random(DateTime.Now.Millisecond);
        int index = rnd.Next(0, dialogs.Count);
        
        DialogGraph d = dialogs[index];
        dialogs.Remove(d);
        return d;
    }

    //finish a dialog or start one
    private void ChangeState(){
        _isDialoging = !_isDialoging;
    }

    //for verification if a dialog is happening
    public bool IsDialoging(){ return _isDialoging; }
    public bool isDialogsAvailable(){ return dialogs.Count != 0; }

    // Add new dialog
    
    public void InsertDialog(string dialogKey, DialogGraph d){
        dialogNodes.Add(dialogKey, d); 
        dialogs.Add(d);
    }
    

    /* Functions to run on Start Function */

    // Move node dialogs to a list to be used
    // List receives only dialogs not used with the user
    // ** this function is used just in random version
    void MoveDialogsToPile(){
        foreach (KeyValuePair<string, DialogGraph> item in dialogNodes){
            /* Search on Arthur´s memory if the dialog is available -> if item e CLOSED: faz nada*/
            dialogs.Add(item.Value);
        }
    }

    
    public string GetId() { return _identificator; }
    public int GetLengthDialogs() { return dialogs.Count; }

    public DialogGraph GetCurrentDialog() { return currentDialog; }
    
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