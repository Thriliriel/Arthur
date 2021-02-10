using System;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using System.IO;
//using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;
//using System.Text;
//using System.Globalization;
using Prolog;

public class MainController : MonoBehaviour
{
    public GameObject faceName;
    //qnt of frames to consider when choosing the faceName (default: 3)
    public int framesToConsider;

    private GameObject[] eyes;
    //array with the last framesToConsider emotions found
    public List<string> foundEmotions;
    public string personName;
    public int personId;
    public string agentName;

    //input text to chat and the chat itself
    public GameObject inputText;
    public Text chatText;

    public string foundEmotion;

    //mario emtion
    public string marioEmotion;

    //agent memory
    //following George Miller definition, each person is able to keep 7 pieces of information in memory at each time, varying more or less 2
    public Dictionary<int,MemoryClass> agentShortTermMemory;
    private TimeSpan memorySpan;
    //long term memory, with the node information
    public Dictionary<int,MemoryClass> agentLongTermMemory;
    //general events
    public Dictionary<int,GeneralEvent> agentGeneralEvents;

    //is agent sleeping?
    public bool isSleeping;
    //zzz text
    public GameObject zzz;
    //sleep button
    public GameObject sleepButton;

    //can arthur speak?
    public bool canSpeak;

    //speaker to speak lol
    public GameObject sc;

    //dual mode chat: can talk with trained bot, or is getting information from the user?
    public bool isGettingInformation;
    public bool isKnowingNewPeople;
    //yes/no question
    public bool isYesNoQuestion;

    //answr for yes/no
    public string yesNoAnswer;

    //noun the user wants to give more information
    public string importantNoun;

    //save a new memory node?
    public bool saveNewMemoryNode;

    //weight threshold to remove an information from the memory
    public float weightThreshold;

    //using memory?
    public bool isUsingMemory;

    //how much time to take a picture?
    public int timeToPicture;
    private int timerPicture;
    public GameObject timerObject;
    public GameObject cam;
    public GameObject randomImage;
    public GameObject riTarget;

    //emotion
    //private string[] emotions;

    //who did the agent already greeted?
    private List<string> peopleGreeted;

    //ice breakers
    private IceBreakingTreeClass iceBreakers;

    //also, lets create the positive and negative answers
    private Dictionary<int, string> positiveAnswer;
    private Dictionary<int, string> negativeAnswer;

    //influence people
    private Dictionary<int, string> influencer;

    //is breaking ice?
    public bool isBreakingIce;

    //id of the icebreaker in use
    public int usingIceBreaker;

    //is influecing?
    public bool isInfluencing;

    //id of the actual root icebreaker, if it is going down the tree
    public int rootIceBreaker;

    //smallTalk stuff
    private List<Topic> topics;
    private List<Topic> topicsFinal;
    private Topic currentTopic;
    //list with all dialogs/topics in memory
    private List<string> dialogsInMemory;
    private List<string> dialogsAnswersInMemory;
    //to save in memory
    //private int qntTempDialogs = 0;
    //private Dictionary<int, string> tempDialogs;

    //next ID for memory
    private int nextEskId;
    private int nextEpisodeId;

    //mariano
    public GameObject mariano;

    //last sentence polarity found
    private float lastPolarity;

    //webservice path
    public string webServicePath;

    //timer for small talk
    public float idleTimer;
    //how much time should Arthur wait?
    public float waitForSeconds;

    public string lastInteraction;

    //is doing retrieval?
    private bool isRetrievingMemory;

    //temp memories to keep for general events later
    /*private int qntTempNodes = 0;
    private Dictionary<int,string> tempNodes;
    private string tempTypeEvent;
    private string tempRelationship;
    private int arthurIdDatabase = 0;*/
    //private bool arthurLearnsSomething = false;

    //can finish it all?
    //private bool canDestroy = false;

    //prolog var for beliefs
    PrologEngine prolog;

    private void Awake()
    {
        //if arthur cannot speak, deactivate the game Object
        if (!canSpeak)
        {
            GameObject.Find("Speaker").SetActive(false);
        }

        webServicePath = "http://localhost:8080/";

        lastPolarity = 0;

        positiveAnswer = new Dictionary<int, string>();
        negativeAnswer = new Dictionary<int, string>();
        influencer = new Dictionary<int, string>();
        //tempNodes = new Dictionary<int, string>();
        //tempDialogs = new Dictionary<int, string>();

        eyes = GameObject.FindGameObjectsWithTag("Eye");

        //set the ice breakers
        rootIceBreaker = usingIceBreaker = -1;
        //first element is just the pointer to the root questions
        iceBreakers = new IceBreakingTreeClass(0, "root", "", false);
        usingIceBreaker = 0;

        //load icebreakers and answers from the file
        LoadIceBreakersAndStuff();

        //set the small talks
        topics = new List<Topic>();
        topicsFinal = new List<Topic>();
        dialogsInMemory = new List<string>();
        dialogsAnswersInMemory = new List<string>();

        LoadSmallTalk();

        foreach(Topic tg in topics)
        {
            topicsFinal.Add(tg);
        }

        //load small talks from the memory
        LoadMemoryDialogs();

        PickTopic();

        //hide zzz
        zzz.SetActive(false);
        timerObject.SetActive(false);
        randomImage.SetActive(false);
        riTarget.SetActive(false);

        //foundNames = new List<string>();
        foundEmotions = new List<string>();
        peopleGreeted = new List<string>();
        agentShortTermMemory = new Dictionary<int, MemoryClass>();
        agentLongTermMemory = new Dictionary<int, MemoryClass>();
        agentGeneralEvents = new Dictionary<int, GeneralEvent>();
        memorySpan = new TimeSpan(0, 0, 15);

        //start the prolog
        prolog = new PrologEngine(persistentCommandHistory: false);

        //what we have on textLTM, load into auxiliary LTM
        LoadEpisodicMemory();

        //create the facts from the memory
        CreateFactsFromMemory();

        //load from the database
        //string match = "match(n) return n";
        //StartCoroutine(MatchWebService(match, true));

        //read the next ID from the file
        //first line: ESK Ids. Second line: Episode Ids
        StreamReader sr = new StreamReader("nextId.txt", System.Text.Encoding.Default);
        string textFile = sr.ReadLine();
        nextEskId = int.Parse(textFile.Trim());
        textFile = sr.ReadLine();
        nextEpisodeId = int.Parse(textFile.Trim());
        sr.Close();

        //load prolog beliefs
        LoadBeliefs();

        // Question
        /*PrologEngine.ISolution solution = prolog.GetFirstSolution(query: "sibling(jack,alice).");
        UnityEngine.Debug.Log("Solution sibling: " + solution.ToString().Trim());

        solution = prolog.GetFirstSolution(query: "love(knob,X).");
        UnityEngine.Debug.Log("Solution love: " + solution.ToString().Trim());*/
    }

    // Start is called before the first frame update
    void Start()
    {
        //reset the result file
        StreamWriter writingResult;
        writingResult = File.CreateText("result.txt");
        writingResult.Write("");
        writingResult.Close();

        //reset the result token files
        ResetTokenFiles();

        //short-term memory co-routine
        StartCoroutine(ControlSTM());

        //face recognition co-routine
        StartCoroutine(ChangeFaceName());

        //SetEmotion("disgust");
        SetEmotion("joy");

        //start the idle timer with the seconds now
        idleTimer = Time.time;
    }

    private void OnDestroy()
    {
        //save LTM as it is
        SaveEpisodic();

        //save used STs
        SaveUsedST();

        //save next ID
        StreamWriter textToToken = new StreamWriter("nextId.txt");
        textToToken.WriteLine(nextEskId+"\n"+nextEpisodeId);
        textToToken.Close();

        //consolidate memory
        MemoryREM();
    }

    // Update is called once per frame
    void Update()
    {
        //just update if it is awake
        if (!isSleeping)
        {
            isUsingMemory = true;

            //check keyboard. When user types "Enter", go to chat or submit it
            if (Input.GetButtonDown("Submit"))
            {
                //if the field is not focused, do it
                if (inputText.GetComponent<InputField>().text == "")
                {
                    inputText.GetComponent<InputField>().ActivateInputField();
                }//else, we submit the sentence
                else
                {
                    SendRequestChat();
                }
            }

            //check the predominant emotion
            if (foundEmotions.Count > 0)
            {
                Dictionary<string, int> emo = new Dictionary<string, int>();
                foreach (string em in foundEmotions)
                {
                    if (emo.ContainsKey(em))
                    {
                        emo[em]++;
                    }
                    else
                    {
                        emo.Add(em, 1);
                    }
                }

                string indx = "";
                int qntx = 0;
                foreach (KeyValuePair<string, int> hj in emo)
                {
                    if (hj.Value > qntx)
                    {
                        qntx = hj.Value;
                        indx = hj.Key;
                    }
                }

                foundEmotion = indx;

                //change emotion for empathy (deactivated for now)
                //SetEmotion(foundEmotion);
            }

            //ChangeFaceName();

            //check tokens
            CheckNewTokens();

            //if save new memory node, save it
            if (saveNewMemoryNode)
            {
                //dictionary with the word and its tag (noun, verb, etc...)
                Dictionary<string, string> tokens = GetTokensFile();

                //if it has tokens, we try to make a generative retrieval
                if (tokens != null)
                {
                    //change some tokens, if exists
                    if (tokens.ContainsKey("you") && !tokens.ContainsKey("Arthur"))
                    {
                        tokens.Remove("you");
                        tokens.Add("Arthur", "NNP");
                    }
                    if (tokens.ContainsKey("yourself") && !tokens.ContainsKey("Arthur"))
                    {
                        tokens.Remove("yourself");
                        tokens.Add("Arthur", "NNP");
                    }
                    if (tokens.ContainsKey("i"))
                    {
                        tokens.Remove("i");
                        tokens.Add(personName, "NNP");
                    }
                    if (tokens.ContainsKey("me"))
                    {
                        tokens.Remove("me");
                        tokens.Add(personName, "NNP");
                    }
                    if (tokens.ContainsKey("myself"))
                    {
                        tokens.Remove("myself");
                        tokens.Add(personName, "NNP");
                    }

                    if (!isGettingInformation && isKnowingNewPeople)
                    {
                        SaveNewPerson(tokens);
                        isUsingMemory = false;
                    }
                    //if it is a yes/no question, save the text, because the tokenizer excludes such words. Therefore, we do not need to tokenize
                    else if (isYesNoQuestion)
                    {
                        foreach (KeyValuePair<string, string> tt in tokens)
                        {
                            if(tt.Key.ToLower() == "yes" || tt.Key.ToLower() == "no")
                            {
                                yesNoAnswer = tt.Key.ToLower();
                                break;
                            }
                        }
                        
                        DealYesNo();

                        //do not save memory of this
                        saveNewMemoryNode = false;
                        isUsingMemory = false;
                    }
                    //if not using memory, just send it to the chatbot and whatever...
                    else if (!isUsingMemory)
                    {
                        //request for chat.
                        string txt = "";
                        foreach (KeyValuePair<string, string> tt in tokens)
                        {
                            txt += tt.Key + " ";
                        }
                        StartCoroutine(GetRequest("https://acobot-brainshop-ai-v1.p.rapidapi.com/get?bid=178&key=sX5A2PcYZbsN5EY6&uid=mashape&msg=" + txt));
                    }
                    else if (!isBreakingIce && !currentTopic.IsDialoging())
                    {
                        //if it is a question, we do not save it. Otherwise, yeap
                        saveNewMemoryNode = true;
                        foreach (KeyValuePair<string, string> tt in tokens)
                        {
                            if (tt.Key == "?")
                            {
                                saveNewMemoryNode = false;
                                break;
                            }
                        }
                        GenerativeRetrieval(tokens);

                        /*GeneralEvent foundIt = null;
                        bool useIt = false;
                        foreach(KeyValuePair<GeneralEvent,bool> ge in ahaaaa)
                        {
                            foundIt = ge.Key;
                            useIt = ge.Value;
                        }
                        if (foundIt != null && useIt)
                        {
                            DealWithIt(foundIt, tokens);
                        }
                        else
                        {
                            //else, see if we have some new term to learn
                            string unk = CheckNewTerm(foundIt, tokens);
                            if(unk != "" && unk != ". ")
                            {
                                SpeakYouFool(unk);
                            }//else, dunno
                            else
                            {
                                SpeakYouFool("Sorry, i do not know.");
                            }
                        }*/
                    }

                    //is using memory, go on
                    if (isUsingMemory)
                    {
                        string informationEvent = "";

                        //if it is breaking ice, add the topic of the conversation and the person
                        if (isBreakingIce)
                        {
                            tokens.Add(iceBreakers.FindIcebreaker(usingIceBreaker).GetType(), "NN");
                            tokens.Add(personName, "NNP");

                            informationEvent = iceBreakers.FindIcebreaker(usingIceBreaker).GetType() + " " + personName;
                        }

                        //save it
                        if (saveNewMemoryNode)
                            SaveMemoryNode(tokens, informationEvent);

                        //here, if it is breaking the ice, try to keep the conversation alive
                        if (isBreakingIce)
                        {
                            BreakIce();
                        }//else, if it is just small talking, get the answer
                        else if (currentTopic.IsDialoging())
                        {
                            SmallTalking();
                        }
                    }
                }

                //reset it
                saveNewMemoryNode = false;

                //reset files
                ResetTokenFiles();
            }

            //if it is not breaking ice or already small talking, check the idle timer for a small talk
            if(!isBreakingIce && !currentTopic.IsDialoging())
            if(Time.time - idleTimer > waitForSeconds)
            {
                SmallTalking();
            }

            //emotion based on last polarity answer
            /*if (lastPolarity < 0) SetEmotion("sadness");
            else if (lastPolarity > 0) SetEmotion("joy");
            else SetEmotion("neutral");*/
        }
    }

    //Load Episodic memory
    private void LoadEpisodicMemory()
    {
        StreamReader readingLTM = new StreamReader("AutobiographicalStorage/episodicMemory.txt", System.Text.Encoding.Default);
        //the file stores both info, divided by "%%%"
        bool readingESK = true;
        using (readingLTM)
        {
            string line;
            do
            {
                line = readingLTM.ReadLine();

                //when we read the dividing sequence "%%%", episodes start
                if(line == "%%%")
                {
                    readingESK = false;
                    continue;
                }

                if (line != "" && line != null)
                {
                    string[] info = line.Split(';');
                    int ide = System.Convert.ToInt32(info[0]);

                    //while it is reading ESK
                    if (readingESK)
                    {
                        //id;memory timestamp;information;5W1H class;Activation;Weight
                        MemoryClass newMem = new MemoryClass(System.DateTime.Parse(info[1]), info[3], info[2], ide, float.Parse(info[5]));
                        //newMem.activation = System.Convert.ToSingle(info[4]);

                        //LTM - everything
                        agentLongTermMemory.Add(ide, newMem);
                    }//else, it is episodes
                    else
                    {
                        //id;memory timestamp;information;nodes
                        GeneralEvent newGen = new GeneralEvent(System.DateTime.Parse(info[1]), info[2], ide);

                        //add the associated nodes of this episode
                        string[] memNodes = info[3].Split('_');
                        foreach(string nod in memNodes)
                        {
                            newGen.nodes.Add(agentLongTermMemory[int.Parse(nod)]);
                        }

                        //add
                        agentGeneralEvents.Add(ide, newGen);
                    }
                }
            } while (line != null);
        }
        readingLTM.Close();
    }

    //Agent says something
    private void SpeakYouFool(string weirdThingToTalk)
    {
        chatText.text += "<b>"+agentName+"</b>: "+ weirdThingToTalk+"\n";

        //just speak if canSpeak is true
        if (canSpeak)
        {
            //also, speak it
            sc.GetComponent<SpeakerController>().SpeakSomething(weirdThingToTalk);
        }
    }

    //deals with unknown information
    private string DealUnknown(string unknoun)
    {
        //asks the user if it wants to give more details about this subject
        string responseText = "I see. I do not know " + unknoun + ", would you like to show me a picture?";
        //SpeakYouFool(responseText);

        //yes/no question
        isYesNoQuestion = true;

        //keep it, so we know later what are we refering of
        importantNoun = unknoun;

        return responseText;
    }

    //deal with yes/no question
    private void DealYesNo()
    {
        //check if there are any yes, sure, or such on the tokens
        //if so, the user desires to provide more information
        if (yesNoAnswer.Contains("Yes") || yesNoAnswer.Contains("yes") ||
            yesNoAnswer.Contains("Sure") || yesNoAnswer.Contains("sure"))
        {
            //allow the user to present a picture
            DealYes();
        }//else, user is satisfied
        else
        {
            //just keep going
            DealNo();
        }
    }

    private void DealYes()
    {
        string responseText = "Great! Please, show me a image of a " + importantNoun + ". You have " + timeToPicture + " seconds!";
        SpeakYouFool(responseText);

        //dont need to draw the rectangle, we just take the picture from the webcam
        //DrawRectangle(new Vector3(0, 0, -1), new Vector3(1, 1, -1));

        //we start a timer for the person put the image in front of the webcam
        StartTimer();
    }

    private void DealNo()
    {
        string responseText = "Sure, no problem. Anything else you would like to talk about?";
        SpeakYouFool(responseText);

        //reset
        isYesNoQuestion = false;
        importantNoun = "";
        yesNoAnswer = "";
    }

    //deal with the retrieved memory
    private void DealWithIt(GeneralEvent retrieved, Dictionary<string, string> tokens)
    {
        string responseText = "";

        //by default, we get the episode itself
        responseText += retrieved.information;

        //some things we can try to infer, like Icebreakers
        //lets divide the nodes by the type
        List<MemoryClass> person = new List<MemoryClass>();
        List<MemoryClass> location = new List<MemoryClass>();
        List<MemoryClass> time = new List<MemoryClass>();
        List<MemoryClass> activity = new List<MemoryClass>();
        List<MemoryClass> emotion = new List<MemoryClass>();
        List<MemoryClass> imagery = new List<MemoryClass>();
        List<MemoryClass> objects = new List<MemoryClass>();
        foreach (MemoryClass mem in retrieved.nodes)
        {
            if (mem.informationType == "Person") person.Add(mem);
            if (mem.informationType == "Location") location.Add(mem);
            if (mem.informationType == "Time") time.Add(mem);
            if (mem.informationType == "Activity") activity.Add(mem);
            if (mem.informationType == "Emotion") emotion.Add(mem);
            if (mem.informationType == "Imagery") imagery.Add(mem);
            if (mem.informationType == "Object") objects.Add(mem);
        }
        //if we have activity
        if(activity.Count > 0)
        {
            foreach (MemoryClass mem in activity)
            {   
                //if it is "born", we get the age
                if(mem.information == "born" && time.Count > 0)
                {
                    responseText = (int.Parse(System.DateTime.Now.ToString("yyyy")) - int.Parse(time[0].information)) +" years old";
                    break;
                }//if it is meet, we also need to show the date, not the normal day (not "today", for example)
                else if (mem.information == "meet" && time.Count > 0)
                {
                    responseText = "Yeah, we met at " + time[0].information;
                    break;
                }
            }
        }

        //if has image, show it to the user
        if (imagery.Count > 0)
        {
            string imagePath = imagery[0].information;
            //say it also
            responseText += ". Here it is!";

            var bytes = System.IO.File.ReadAllBytes(imagePath);
            var tex = new Texture2D(1, 1);
            tex.LoadImage(bytes);
            randomImage.GetComponent<MeshRenderer>().material.mainTexture = tex;

            //show
            randomImage.SetActive(true);
            riTarget.SetActive(true);
        }

        //now, lets see if we have some new term
        //string unk = CheckNewTerm(retrieved, tokens);

        SpeakYouFool(responseText);// + unk
    }

    //UPDATE: deactivated for now, since it implies to have only one "term" (for example, can have only one cellphone)
    /*private string CheckNewTerm(GeneralEvent retrieved, Dictionary<string, string> tokens)
    {
        //now, lets see if we have some new term
        string unknoun = "";

        //check if the found event has the Noun or proper noun on it
        //if it does not, it means Arthur is not yet familiar with such term
        //so, we ask the user if he wants to give more details.
        List<string> nouns = new List<string>();
        foreach (KeyValuePair<string, string> tt in tokens)
        {
            if (tt.Value == "NN" || tt.Value == "NNP")
            {
                nouns.Add(tt.Key);
            }
        }

        //lets see if these nouns exist in the event
        if (retrieved != null)
        {
            foreach (MemoryClass nds in retrieved.nodes)
            {
                if (nds.informationType == "Person" || nds.informationType == "Object")
                {
                    if (nouns.Contains(nds.information))
                        nouns.Remove(nds.information);
                }
            }
        }

        //if nothing, need to learn
        if (nouns.Count > 0)
        {
            foreach(string nn in nouns)
            {
                if(nn != "Arthur" && nn != personName)
                {
                    unknoun = nn;
                    break;
                }
            }
        }

        //do not save this memory, since it already exists somehow
        saveNewMemoryNode = false;

        //if it has an "unknoun", deal with it
        string unk = ". ";
        if (unknoun != "")
        {
            unk += DealUnknown(unknoun);
        }

        return unk;
    }*/

    //deal version without event
    /*private void DealWithIt(Dictionary<string, string> retrieved, Dictionary<string, string> tokens)
    {
        string answer = "";

        //first, we check questions about icebreakers
        //about age
        if (tokens.ContainsKey("old") || tokens.ContainsKey("age"))
        {
            foreach (KeyValuePair<string, string> ret in retrieved)
            {
                //we assume there is only one
                string banana = ret.Value.Replace(",age:", "?");
                string[] info = banana.Split('?');
                //if does not have age, we can do nothing about it =)
                if (info.Length == 1)
                {
                    answer = "I do not know, sorry";
                }
                else
                {
                    info = info[1].Split(',');
                    answer = info[0] + " years old";
                }
                break;
            }
        } //about study
        else if (tokens.ContainsKey("study"))
        {
            string studyWhat = "";
            foreach (KeyValuePair<string, string> ret in retrieved)
            {
                string banana = ret.Value.Replace("study:", "?");
                string[] info = banana.Split('?');
                //yes or no
                if (info.Length == 2)
                {
                    info = info[1].Split(',');
                    answer = info[0];
                }//else, it is what he/she studies
                else
                {
                    studyWhat = ret.Key;
                }
            }

            if (answer == "")
            {
                answer = "I do not know, sorry";
            }
            else
            {
                answer += " " + studyWhat;
            }
        }//work
        else if (tokens.ContainsKey("work") || tokens.ContainsKey("job"))
        {
            string workWhat = "";
            foreach (KeyValuePair<string, string> ret in retrieved)
            {
                string banana = ret.Value.Replace("work:", "?");
                string[] info = banana.Split('?');
                //yes or no
                if (info.Length == 2)
                {
                    info = info[1].Split(',');
                    answer = info[0];
                }//else, it is what he/she studies
                else
                {
                    workWhat = ret.Key;
                }
            }

            if (answer == "")
            {
                answer = "I do not know, sorry";
            }
            else
            {
                answer += " " + workWhat;
            }
        }//children
        else if (tokens.ContainsKey("child") || tokens.ContainsKey("children"))
        {
            string kids = "";
            foreach (KeyValuePair<string, string> ret in retrieved)
            {
                string banana = ret.Value.Replace("children:", "?");
                string[] info = banana.Split('?');
                //yes or no
                if (info.Length == 2)
                {
                    info = info[1].Split(',');
                    answer = info[0];
                }//else, it is what he/she studies
                else
                {
                    kids += ret.Key + " ";
                }
            }

            if (answer == "")
            {
                answer = "I do not know, sorry";
            }
            else
            {
                answer += " " + kids;
            }
        }
        //depending on the verbs used, we give an answer
        //if tokens have "know", arthur is being asked if he knows something
        else if (tokens.ContainsKey("know"))
        {
            answer = "Yeah, i know a ";
            foreach (KeyValuePair<string, string> ret in retrieved)
            {
                if (ret.Key != "Arthur" && ret.Key != personName)
                {
                    answer += ret.Key + " ";
                }
            }

            //if found nothing, it may be a question about the person or arthur
            if (answer == "Yeah, i know a ")
            {
                if (retrieved.ContainsKey(personName))
                {
                    answer = "Yeah, i know " + personName;
                }else if (retrieved.ContainsKey("Arthur"))
                {
                    answer = "Of course i know myself! Duh!!";
                }
            }

            //if found nothing, he does not know it
            if (answer == "Yeah, i know a ")
            {
                answer = "No, i do not know it";
            }
            else
            {
                //if has image, show it to the user
                string imagePath = "";
                foreach (KeyValuePair<string, string> ret in retrieved)
                {
                    if (ret.Value.Contains("image"))
                    {
                        string temp = ret.Value.Replace("image:", "?");
                        string[] pato = temp.Split('?');
                        pato = pato[1].Split(',');
                        imagePath = pato[0];
                        break;
                    }
                }
                if (imagePath != "")
                {
                    //say it also
                    answer += ". Here it is!";

                    var bytes = System.IO.File.ReadAllBytes(imagePath);
                    var tex = new Texture2D(1, 1);
                    tex.LoadImage(bytes);
                    randomImage.GetComponent<MeshRenderer>().material.mainTexture = tex;

                    //show
                    randomImage.SetActive(true);
                    riTarget.SetActive(true);
                }
            }
        }//if tokens have "meet", arthur is being asked if he met someone (know as well, but already got it)
        //seems like "meet" is not a good verb for NLTK... will keep it here anyway...
        else if (tokens.ContainsKey("meet"))
        {
            if (retrieved.ContainsKey(personName))
            {
                answer = "Yeah, i already met " + personName;
            }
            else if (retrieved.ContainsKey("Arthur"))
            {
                answer = "Of course i met myself! Duh!!";
            }

            //if has image, show it to the user
            string imagePath = "";
            foreach (KeyValuePair<string, string> ret in retrieved)
            {
                if (ret.Value.Contains("image"))
                {
                    string temp = ret.Value.Replace("image:", "?");
                    string[] pato = temp.Split('?');
                    pato = pato[1].Split(',');
                    imagePath = pato[0];
                }
            }
            if (imagePath != "")
            {
                //say it also
                answer += ". Here it is!";

                var bytes = System.IO.File.ReadAllBytes(imagePath);
                var tex = new Texture2D(1, 1);
                tex.LoadImage(bytes);
                randomImage.GetComponent<MeshRenderer>().material.mainTexture = tex;

                //show
                randomImage.SetActive(true);
                riTarget.SetActive(true);
            }
        }//else, just show what he found
        else
        {
            foreach (KeyValuePair<string, string> ret in retrieved)
            {
                answer += ret.Key + " ";
            }
        }

        SpeakYouFool(answer);
    }

    //take an action depending the emotion
    private string EmotionMemoryPicker(string emo)
    {
        string returningText = "";

        switch (emo)
        {
            case "joy":
                returningText = "happy";
                break;
            case "sadness":
                returningText = "sad";
                break;
            case "fear":
                returningText = "afraid";
                break;
            case "disgust":
                returningText = "disgusted";
                break;
            case "anger":
                returningText = "angered";
                break;
            case "surprise":
                returningText = "surprised";
                break;
        }

        return returningText;
    }*/

    //reset token files
    private void ResetTokenFiles()
    {
        StreamWriter writingResult;

        //reset the result token file
        writingResult = File.CreateText("resultToken.txt");
        writingResult.Write("");
        writingResult.Close();

        //reset the textToToken
        writingResult = File.CreateText("textToToken.txt");
        writingResult.Write("");
        writingResult.Close();
    }

    //save ice breakers on memory
    private void SaveIceBreaker(Dictionary<string, string> tokens, string informationEvent)
    {
        float weight = 0.9f;
        //depending on the ice breaker, we just add info in the person
        if (tokens.ContainsKey("old"))
        {
            string birth = "";
            int thisID = -1;
            foreach (KeyValuePair<string, string> txt in tokens)
            {
                if(txt.Key != personName && txt.Key != "old")
                {
                    //StartCoroutine(UpdateMemoryNodeWebService(personName, "age", txt.Key));

                    //since we are saving the Time aspect, just do some math to create a proper date
                    int thisYear = int.Parse(System.DateTime.Now.ToString("yyyy"));
                    int result = thisYear - int.Parse(txt.Key);
                    birth = result.ToString();
                    thisID = AddToSTM("Time", birth, weight);
                }
            }

            //save the episode with person, "born" and year
            List<int> connectNodes = new List<int>();
            connectNodes.Add(personId);
            connectNodes.Add(5);
            connectNodes.Add(thisID);
            AddGeneralEvent(personName + " was born in " + birth + "-01-01", connectNodes);
        }else if (tokens.ContainsKey("study"))
        {
            /*foreach (KeyValuePair<string, string> txt in tokens)
            {
                if (txt.Key != personName && txt.Key != "study")
                {
                    StartCoroutine(UpdateMemoryNodeWebService(personName, "study", txt.Key));
                }
            }*/
            //here, we just need to get last polarity, because it is just study or not
            //if it is negative, we can already save it. Otherwise, we just save when details are provided
            if(lastPolarity < 0)
            {
                List<int> connectNodes = new List<int>();
                connectNodes.Add(personId);
                connectNodes.Add(7);
                AddGeneralEvent(personName + " is not studying", connectNodes);
            }
        }
        else if (tokens.ContainsKey("work"))
        {
            /*foreach (KeyValuePair<string, string> txt in tokens)
            {
                if (txt.Key != personName && txt.Key != "work")
                {
                    StartCoroutine(UpdateMemoryNodeWebService(personName, "work", txt.Key));
                }
            }*/
            //here, we just need to get last polarity, because it is just study or not
            //if it is negative, we can already save it. Otherwise, we just save when details are provided
            if (lastPolarity < 0)
            {
                List<int> connectNodes = new List<int>();
                connectNodes.Add(personId);
                connectNodes.Add(6);
                AddGeneralEvent(personName + " is not working", connectNodes);
            }
        }
        else if (tokens.ContainsKey("children"))
        {
            foreach (KeyValuePair<string, string> txt in tokens)
            {
                if (txt.Key != personName && txt.Key != "children")
                {
                    //if answer is no, so no!
                    if (lastPolarity < 0)
                    {
                        List<int> connectNodes = new List<int>();
                        connectNodes.Add(personId);
                        connectNodes.Add(8);
                        AddGeneralEvent(personName + " has no children", connectNodes);
                    }
                }
            }
        }
        else if (tokens.ContainsKey("study course"))
        {
            string course = "";
            foreach (KeyValuePair<string, string> txt in tokens)
            {
                if (txt.Key != personName && txt.Key != "study course")
                {
                    if(course == "")
                    {
                        course = txt.Key;
                    }
                    else
                    {
                        course += "_" + txt.Key;
                    }
                }
            }

            int thisID = AddToSTM("Activity", course, weight);

            List<int> connectNodes = new List<int>();
            connectNodes.Add(personId);
            connectNodes.Add(7);
            connectNodes.Add(thisID);
            AddGeneralEvent(personName + " is studying " + course, connectNodes);
        }
        else if (tokens.ContainsKey("work job"))
        {
            string job = "";
            foreach (KeyValuePair<string, string> txt in tokens)
            {
                if (txt.Key != personName && txt.Key != "work job")
                {
                    if (job == "")
                    {
                        job = txt.Key;
                    }
                    else
                    {
                        job += "_" + txt.Key;
                    }
                }
            }

            int thisID = AddToSTM("Activity", job, weight);

            List<int> connectNodes = new List<int>();
            connectNodes.Add(personId);
            connectNodes.Add(6);
            connectNodes.Add(thisID);
            AddGeneralEvent(personName + " is working as " + job, connectNodes);
        }
        /*else if (tokens.ContainsKey("children quantity"))
        {
            foreach (KeyValuePair<string, string> txt in tokens)
            {
                if (txt.Key != personName && txt.Key != "children quantity")
                {
                    StartCoroutine(UpdateMemoryNodeWebService(personName, "qntChildren", txt.Key));
                }
            }
        }*/
        else if (tokens.ContainsKey("children names"))
        {
            int qntChild = 0;
            List<int> connectNodes = new List<int>();
            connectNodes.Add(personId);
            connectNodes.Add(8);
            string who = "";
            foreach (KeyValuePair<string, string> txt in tokens)
            {
                if (txt.Key != personName && txt.Key != "children names" && txt.Key != "and")
                {
                    int thisID = AddToSTM("Person", txt.Key, weight);
                    connectNodes.Add(thisID);

                    if (who == "") who = txt.Key;
                    else who += " and " + txt.Key;

                    qntChild++;
                }
            }

            //if qntChild > 0, we save the children
            if (qntChild > 0)
            {
                AddGeneralEvent(personName + " has "+qntChild+" children: " + who, connectNodes);
            }
        }
    }

    private void SaveSmallTalk(Dictionary<string, string> tokens)
    {
        //just to be sure, clear it
        //tempDialogs.Clear();

        //first, we need to save this topic and dialog
        //for create relationship later
        //qntTempDialogs = 2; //topic and dialog to connect
        //type of the event, to save later

        //save topic dialog description
        /*string label = "name:'" + currentTopic.GetCurrentDialog().GetDescription() + "',activation:1,weight:0.8,nodeType:'text'";
        StartCoroutine(CreateMemoryNodeWebService(currentTopic.GetCurrentDialog().GetDescription(), "Topic", label, 0.8f));

        //save dialog
        label = "name:'Dialog" + currentTopic.GetCurrentDialog().GetId().ToString() + "',activation:1,weight:0.8,nodeType:'text',topic:'"+ currentTopic.GetCurrentDialog().GetDescription() + "'";//text:'"+ currentTopic.GetCurrentDialog() + "'
        StartCoroutine(CreateMemoryNodeWebService("Dialog"+currentTopic.GetCurrentDialog().GetId().ToString(), "Dialog", label, 0.8f));
        */
        //NEED TO SEE ABOVE

        //if we have 2 or more NNP in sequence, we understand it is a compound name (ex: Sonata Artica)
        Dictionary<string, string> newTokens = new Dictionary<string, string>();
        string merging = "";

        foreach(KeyValuePair<string,string> tt in tokens)
        {
            //if it is a NNP
            if (tt.Value == "NNP")
            {
                if(merging == "")
                {
                    merging += tt.Key;
                }
                else
                {
                    merging += "_" + tt.Key;
                }
            }//otherwise, we can check if we have something to add
            else
            {
                if(merging != "")
                {
                    newTokens.Add(merging, "NNP");
                    newTokens.Add(tt.Key, tt.Value);
                }
                else
                {
                    newTokens.Add(tt.Key, tt.Value);
                }

                merging = "";
            }
        }

        //if last word(s) are NNP, we still need to add it
        if(merging != "")
        {
            newTokens.Add(merging, "NNP");
            merging = "";
        }

        //now, save the answer
        //for create general event later
        //qntTempNodes = newTokens.Count;
        //type of the event, to save later
        //tempTypeEvent = "";
        //tempRelationship = "KNOWS";
        float weight = 0.8f;

        //"create" the person as well, just to get id back, if it is not already in the sentence
        /*if (!newTokens.ContainsKey(personName))
        {
            StartCoroutine(CreateMemoryNodeWebService(personName, "Person", "", 0.9f));
            qntTempNodes++;
        }*/

        List<int> connectNodes = new List<int>();
        string infor = personName;
        //if the smalltalk has the information, we use it
        Tuple<string, string> memData = currentTopic.GetCurrentDialog().GetMemoryData();
        if (memData.Item1 != "")
        {
            connectNodes.Add(personId);

            int thisID = AddToSTM("Activity", memData.Item1, weight);
            connectNodes.Add(thisID);
            infor += " " + memData.Item1;

            thisID = AddToSTM("Object", memData.Item2, weight);
            connectNodes.Add(thisID);
            infor += " " + memData.Item2;
        }
        else
        {
            //for each information, save it in memory
            foreach (KeyValuePair<string, string> txt in newTokens)
            {
                /*string nodeTag = "SmallTalk";

                //if is verb, relationship
                if (txt.Value == "VB" || txt.Value == "VBP")
                {
                    tempRelationship = txt.Key.ToUpper();
                    qntTempNodes--;
                    continue;
                }

                //if it is NNP, we save with Person tag
                if(txt.Value == "NNP")
                {
                    nodeTag = "Person";
                }

                //if it is Arthur or the person, just get it
                if (txt.Key == "Arthur" || txt.Key == personName)
                {
                    StartCoroutine(CreateMemoryNodeWebService(txt.Key, "Person", "", 0.9f));
                }
                else
                {
                    label = "name:'" + txt.Key + "',activation:1,weight:" + weight + ",nodeType:'text',lastEmotion:'" + foundEmotion + "'";
                    StartCoroutine(CreateMemoryNodeWebService(txt.Key, nodeTag, label, weight));
                }*/

                string fiveW = "";
                //NEED TO SEE HOW TO TAKE THE NAMED ENTITIES
                //if it is a proper noun, people
                if (txt.Value == "NNP")
                {
                    fiveW = "Person";
                }//else, if it is a noun, object
                else if (txt.Value == "NN")
                {
                    fiveW = "Object";
                }
                //else, if it is a verb, activity
                else if (txt.Value == "VB" || txt.Value == "VBP")
                {
                    fiveW = "Activity";
                }

                //if fiveW is empty, no need to store
                if (fiveW != "")
                {
                    //strip the "'"
                    int thisID = AddToSTM(fiveW, txt.Key, weight);
                    connectNodes.Add(thisID);

                    infor += " " + txt.Key;
                }
            }
        }

        //create a new general event
        if (connectNodes.Count > 0)
        {
            connectNodes.Add(personId);
            AddGeneralEvent(infor.Trim(), connectNodes);
        }

        connectNodes.Clear();
    }

    //save a new memory node and return the tokens
    private void SaveMemoryNode(Dictionary<string, string> tokens, string informationEvent)
    {
        //list to keep memory IDS inserted, so we can connect them later
        List<int> connectNodes = new List<int>();
        //string typeEvent = "interaction";
        float weight = 0.5f;

        if (isBreakingIce)
        {
            SaveIceBreaker(tokens, informationEvent);
        }else if (currentTopic.IsDialoging())
        {
            SaveSmallTalk(tokens);
        }
        else
        {
            //for each information, save it in memory
            string potato = "";
            foreach (KeyValuePair<string, string> txt in tokens)
            {
                string fiveW = "";
                //NEED TO SEE HOW TO DO IT YET. FOR NOW:
                //if it is a proper noun, people
                if (txt.Value == "NNP")
                {
                    fiveW = "Person";
                }//else, if it is a noun, object
                else if (txt.Value == "NN")
                {
                    fiveW = "Object";
                }//else, if it is a verb, activity
                else if (txt.Value == "VB" || txt.Value == "VBP")
                {
                    fiveW = "Activity";
                }

                //if fiveW is empty, no need to store
                if (fiveW != "")
                {
                    //strip the "'"
                    int thisID = AddToSTM(fiveW, txt.Key, weight);
                    connectNodes.Add(thisID);
                    if (potato == "") potato = txt.Key;
                    else potato += " " + txt.Key;
                }
                //save on Neo4j
                //on temp, we have to find 2 information later

                //if is verb, relationship
                /*if(txt.Value == "VB" || txt.Value == "VBP")
                {
                    tempRelationship = txt.Key.ToUpper();
                    qntTempNodes--;
                    continue;
                }
                
                string label = "name:'" + txt.Key + "',activation:1,weight:"+weight+ ",nodeType:'text',lastEmotion:'" + foundEmotion + "'";
                StartCoroutine(CreateMemoryNodeWebService(txt.Key, "Interaction", label, weight));*/
            }

            //create a new general event
            if (connectNodes.Count > 0)
            {
                if(informationEvent == "")
                {
                    informationEvent = potato;
                }

                AddGeneralEvent(informationEvent.Trim(), connectNodes);
            }

            connectNodes.Clear();
        }
    }

    //get the tokens from the file
    private Dictionary<string, string> GetTokensFile()
    {
        //list to keep memory IDS inserted, so we can connect them later
        Dictionary<string, string> returnValues = new Dictionary<string, string>();

        //open the file
        //open file with result
        StreamReader sr = new StreamReader("resultToken.txt", System.Text.Encoding.Default);

        using (sr)
        {
            string line;
            do
            {
                line = sr.ReadLine();

                if (line != "" && line != null && line != "[]" && !line.Contains("false"))
                {
                    //skip comments
                    if (line.Contains("#")) continue;

                    //memory time;person;emotion
                    string[] info = line.Split(';');

                    //if just have 1 index, it is the polarity. BUT: just change it if it is not influencing
                    if (info.Length == 1)
                    {
                        //just change it if it is not influencing
                        if (!isInfluencing)
                            lastPolarity = float.Parse(info[0]);
                    }
                    else
                    {
                        string token = info[0];
                        string tknType = info[1];

                        if (!returnValues.ContainsKey(token))
                        {
                            returnValues.Add(token, tknType);
                        }
                    }
                }
            } while (line != null);
        }

        sr.Close();

        //reset it
        ResetTokenFiles();

        if (returnValues.Count > 0)
        {
            return returnValues;
        }
        else
        {
            return null;
        }
    }

    private int GenerateEskID()
    {
        return nextEskId++;
    }
    private int GenerateEpisodeID()
    {
        return nextEpisodeId++;
    }

    /*EMOTION STUFF*/
    public string[] ReadEmotion(string emotion)
    {
        List<string> lst;
        string[] aux, lines;
        int i;

        lst = new List<string>();

        foreach (string file in System.IO.Directory.EnumerateFiles("Assets/Data/" + emotion, "*.txt"))
        {
            lines = System.IO.File.ReadAllLines(file);

            // Lado esquerdo
            lst.Add(lines[17]); lst.Add(lines[18]); lst.Add(lines[19]); lst.Add(lines[20]);
            lst.Add(lines[21]); lst.Add(lines[36]); lst.Add(lines[37]); lst.Add(lines[38]);
            lst.Add(lines[39]); lst.Add(lines[40]); lst.Add(lines[41]); lst.Add(lines[68]);

            // Lado direito
            lst.Add(lines[22]); lst.Add(lines[23]); lst.Add(lines[24]); lst.Add(lines[25]);
            lst.Add(lines[26]); lst.Add(lines[42]); lst.Add(lines[43]); lst.Add(lines[44]);
            lst.Add(lines[45]); lst.Add(lines[46]); lst.Add(lines[47]); lst.Add(lines[69]);
        }

        aux = lst.ToArray();
        lst.Clear();
        for (i = 0; i < aux.Length; i++) aux[i] = aux[i].Split(' ')[1] + "_" + aux[i].Split(' ')[2];
        return aux;
    }

    public string[] PrepareEmotion(string[] emotionFrames)
    {
        string[] aux = new string[emotionFrames.Length / 24];
        int i;

        for (i = 0; i < emotionFrames.Length; i += 24)
        {
            aux[i / 24] = emotionFrames[0 + i] + " " + emotionFrames[1 + i] + " " + emotionFrames[2 + i] + " " + emotionFrames[3 + i] + " " +
                          emotionFrames[4 + i] + ":" + emotionFrames[5 + i] + " " + emotionFrames[6 + i] + " " + emotionFrames[7 + i] + " " +
                          emotionFrames[8 + i] + " " + emotionFrames[9 + i] + " " + emotionFrames[10 + i] + " " + emotionFrames[11 + i] +
                          ";" +
                          emotionFrames[12 + i] + " " + emotionFrames[13 + i] + " " + emotionFrames[14 + i] + " " + emotionFrames[15 + i] + " " +
                          emotionFrames[16 + i] + ":" + emotionFrames[17 + i] + " " + emotionFrames[18 + i] + " " + emotionFrames[19 + i] + " " +
                          emotionFrames[20 + i] + " " + emotionFrames[21 + i] + " " + emotionFrames[22 + i] + " " + emotionFrames[23 + i];
        }

        return aux;
    }

    public void SetEmotion(string emotion)
    {
        //List<string[]> aux = EmotionScript(emotion); //bad slow guy!!
        //string[] actionScript = aux.ToArray()[0];
        //string[] emotions = aux.ToArray()[1];

        //if it is setting emotion, it means it found a face. So, let us find out whom face it is
        if (personName == "" && marioEmotion == "")
        {
            StartCoroutine(RecognitionWebService());
        }

        marioEmotion = emotion;

        if (marioEmotion != "")
        {
            if (marioEmotion == "joy")
                marioEmotion = emotion = "happiness";

            string emoAnim = marioEmotion.Substring(0, 1).ToUpper() + marioEmotion.Substring(1) + "_A";
            //UnityEngine.Debug.Log(emoAnim);

            mariano.GetComponent<CharacterCTRL>().PlayAnimation(emoAnim);
        }

        //StartCoroutine(EyesMovement(actionScript));
        //StartCoroutine(EyeBrowsMovement());
        //StartCoroutine(SaccadeMovement(false));
        //StartCoroutine(PerlinMovement());

        //AudioSource audio = GetComponent<AudioSource>();
        //audio.Play();

        //if (emotion != null) UnityEngine.Debug.Log("The emotion " + emotion + " was set!");
    }
    /*END EMOTION STUFF*/

    //new version of SendRequestChat
    public void SendRequestChat()
    {
        //get the text and reset the input
        string textSend = inputText.GetComponent<InputField>().text;
        inputText.GetComponent<InputField>().text = "";

        //put it in the general chat as well
        chatText.text += "<b>" + personName + "</b>: " + textSend + "\n";

        //reset the idle timer
        idleTimer = Time.time;

        lastInteraction = textSend;

        //if the len is different, the user is answering a small talk. Save
        if (dialogsAnswersInMemory.Count != dialogsInMemory.Count)
            dialogsAnswersInMemory.Add(lastInteraction);

        //UPDATE: we always tokenize now, and treat things in the update
        //UPDATE: now we send a request to our webservice, through a json
        StartCoroutine(TokenizationWebService(textSend));

        //replace occurences of "you" for "Arthur"
        //UPDATE: IT IS BETTER TO REPLACE IT LATER, SO THE VERBS ARE CLASSIFIED IN A BETTER WAY
        /*textSend = textSend.Replace(" you ", " Arthur ");
        textSend = textSend.Replace(" you?", " Arthur ");
        textSend = textSend.Replace(" yourself?", " Arthur ");

        //replace occurences of "me" for personName
        textSend = textSend.Replace(" me ", " "+ personName +" ");
        textSend = textSend.Replace(" me?", " " + personName + " ");
        textSend = textSend.Replace(" i ", " " + personName + " ");
        textSend = textSend.Replace(" i?", " " + personName + " ");*/

        /*if (!isGettingInformation && isKnowingNewPeople)
        {
            SaveNewPerson(textSend);
        }
        //if it is a yes/no question, save the text, because the tokenizer excludes such words. Therefore, we do not need to tokenize
        else if (isYesNoQuestion)
        {
            yesNoAnswer = textSend;
            DealYesNo();

            //do not save memory of this
            saveNewMemoryNode = false;
        }
        //else, tokenize
        else
        {
            //tokenize the text, removing the stop words
            //to do so, we save the text in the textToToken file. The tokenization routine will deal with the rest
            /*StreamWriter textToToken = new StreamWriter("textToToken.txt");
            textToToken.WriteLine(textSend);
            textToToken.Close();*

            //UPDATE: now we send a request to our webservice, through a json
            StartCoroutine(TokenizationWebService(textSend));
        }*/
    }

    IEnumerator GetRequest(string uri)
    {
        //just update if it is awake
        if (!isSleeping)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                webRequest.SetRequestHeader("X-RapidAPI-Key", "0aefee70bdmshb32146707c42e78p196f20jsn250666397e7f");
                webRequest.SetRequestHeader("X-RapidAPI-Host", "acobot-brainshop-ai-v1.p.rapidapi.com");

                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                if (webRequest.isNetworkError)
                {
                    UnityEngine.Debug.Log(pages[page] + ": Error: " + webRequest.error);
                }
                else
                {
                    string response = webRequest.downloadHandler.text;
                    //UnityEngine.Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                    response = response.Replace("\"", "");
                    response = response.Replace("}", "");
                    string[] resp = response.Split(':');
                    SpeakYouFool(resp[1]);
                }
            }
        }
        else
        {
            yield return 0;
        }
    }

    //Change the face name found
    protected IEnumerator ChangeFaceName()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            //open file with result
            StreamReader sr = new StreamReader("result.txt", System.Text.Encoding.Default);

            string textFile = sr.ReadToEnd();

            sr.Close();

            if (textFile != "" && !textFile.Contains("false"))
            {
                /*string[] info = textFile.Split('[');
                info = info[1].Split(']');
                info = info[0].Split(',');
                info[0] = info[0].Replace("'", "");
                info = info[0].Split(':');*/

                string info = textFile;

                faceName.GetComponent<Text>().text = info;
                personName = info = info.Trim();

                bool talaaaaa = false;

                //if it already exists in memory, update it, because it is being rehearsed (just work for the name...)
                foreach (KeyValuePair<int, MemoryClass> mc in agentShortTermMemory)
                {
                    if (mc.Value.information.Contains(personName))
                    {
                        talaaaaa = true;

                        mc.Value.weight = mc.Value.activation = 1;
                        mc.Value.memoryTime = System.DateTime.Now;
                        personId = mc.Key;
                        break;
                    }
                }

                //if not found, trying to get ID from LTM
                foreach (KeyValuePair<int, MemoryClass> mc in agentLongTermMemory)
                {
                    if (mc.Value.information.Contains(personName)) {
                        personId = mc.Key;
                        break;
                    }
                }

                //if the person already exists at LTM (and not at the STM), bring it to STM (just work for the name...)
                if (!talaaaaa)
                {
                    //search the general event where the agent met this person
                    /*foreach (GeneralEvent ges in agentGeneralEvents)
                    {
                        if (ges.eventType == "meet new person" && ges.information.Contains(personName))
                        {
                            foreach (MemoryClass mc in ges.nodes)
                            {
                                if (mc.information.Contains(personName))
                                {
                                    mc.weight = mc.activation = 1;
                                    mc.memoryTime = System.DateTime.Now;

                                    AddToSTM(mc.informationType, mc.information, mc.weight);
                                }
                            }

                            break;
                        }
                    }*/
                }

                //if the agent still did not greeted this motherfucker, howdy mate!
                if (!peopleGreeted.Contains(info))
                {
                    isGettingInformation = true;
                    peopleGreeted.Add(info);
                    GreetingTraveler(info);
                }
            }//else, if it is empty, did not find anyone. So, the agent can meet someone new!! How amazing!!!
            //UPDATE: just meet someone new IF did not see anyone yet (to avoid changing between person/not knowing)
            else if (textFile.Contains("false") && !isKnowingNewPeople && faceName.GetComponent<Text>().text == "")
            {
                isGettingInformation = true;

                faceName.GetComponent<Text>().text = "";

                MeetNewPeople();
            }
        }
    }

    //save the new person known
    private void SaveNewPerson(Dictionary<string,string> tokens)
    {
        //reset the result file
        StreamWriter writingResult;
        writingResult = File.CreateText("result.txt");
        writingResult.Write("");
        writingResult.Close();

        //people can answer with more than just the name (My name is Knob). So, lets treat this
        string namePerson = "";

        foreach(KeyValuePair<string, string> tt in tokens)
        {
            //find the NNP
            if(tt.Value == "NNP")
            {
                if (namePerson == "")
                {
                    namePerson += tt.Key;
                }
                else
                {
                    namePerson += "_" + tt.Key;
                }
            }
        }

        if (namePerson != "")
        {
            personName = namePerson.Trim();
            //already know it, do not need to greet
            peopleGreeted.Add(personName);

            //copy the camFile to the Data directory, saving with person name
            //it is going to serve both for face recognition and autobiographical storage for images
            //File.Copy("camImage.png", "Python/face_recognition-master/Data/"+personName+".png");
            if (File.Exists("AutobiographicalStorage/Images/" + namePerson + ".png"))
                File.Delete("AutobiographicalStorage/Images/" + namePerson + ".png");

            File.Copy("camImage.png", "AutobiographicalStorage/Images/" + namePerson + ".png");
            StartCoroutine(SavePersonWebService());

            //save on Neo4j
            //on temp, we have to find 2 information later
            /*qntTempNodes = 2;
            string label = "name:'" + namePerson + "',activation:1,weight:0.9,nodeType:'text',lastEmotion:'" + foundEmotion + "',image:'AutobiographicalStorage/Images/" + namePerson + ".png'";
            StartCoroutine(CreateMemoryNodeWebService(namePerson, "Person", label, 0.9f));

            label = "name:'myself',image:'AutobiographicalStorage/Images/" + namePerson + ".png',activation:1,weight:0.9,nodeType:'image',lastEmotion:'" + foundEmotion + "'";
            StartCoroutine(CreateMemoryNodeWebService("myself", "Image", label, 0.9f));

            //type of the event, to save later
            tempTypeEvent = "meet new person";
            tempRelationship = "HAS_PHOTO";*/

            int thisID = AddToSTM("Person", namePerson, 0.9f);
            List<int> connectNodes = new List<int>();
            connectNodes.Add(1);
            connectNodes.Add(thisID);
            thisID = AddToSTM("Imagery", "AutobiographicalStorage/Images/" + namePerson + ".png", 0.9f);
            connectNodes.Add(thisID);
            connectNodes.Add(11);

            //add this date as well
            string thisYear = System.DateTime.Now.ToString("yyyy-MM-dd");
            thisID = AddToSTM("Time", thisYear, 0.9f);
            connectNodes.Add(thisID);
            AddGeneralEvent("I met " + namePerson + " today", connectNodes);

            isKnowingNewPeople = false;

            //do not need to greet it right now
            peopleGreeted.Add(personName);

            saveNewMemoryNode = false;

            //now that they know each other, lets start to break the ice!
            isBreakingIce = true;
            BreakIce();
        }
    }

    //meet someone new
    private void MeetNewPeople()
    {
        string greetingText = "Hello stranger! May i know your name?";
        SpeakYouFool(greetingText);

        //need to wait for the answer
        isGettingInformation = false;
        isKnowingNewPeople = true;
    }

    //at the first time the agent founds a face, checks if it already knows it. If so, greet it
    private void GreetingTraveler(string mate)
    {
        string greetingText = "Greetings " + mate + "!";

        //create a new autobiographical storage for the person name, into the STM
        /*int idMemoryName = AddToSTM(0, mate, 1);

        //create a new autobiographical storage for the person image, into the STM
        int idImagePerson = AddToSTM(1, "Python/face_recognition-master/Data/" + mate + ".png", 1);

        List<int> connectNodes = new List<int>();
        connectNodes.Add(idMemoryName);
        connectNodes.Add(idImagePerson);

        //connect them both
        ConnectMemoryNodes(connectNodes);

        //create a new general event
        AddGeneralEvent("recognizes person", "i recognize " + mate, connectNodes);*/

        //back to chatbot
        isGettingInformation = false;
        isKnowingNewPeople = false;

        //since they know each other, lets start to break the ice!
        isBreakingIce = true;
        BreakIce(greetingText);
    }

    

    /*private float CalculateSaccade()
    {
        return (-6.9f * Mathf.Log(UnityEngine.Random.Range(1, 15) / 15.7f));
    }*/

    //add to stm and return the memory ID
    private int AddToSTM(string informationType, string information, float weight = 0.1f, int nodeId = -1)
    {
        //first, checks if the memory already exists
        int ind = 0;
        bool backToSTM = false;

        foreach (KeyValuePair<int, MemoryClass> st in agentShortTermMemory)
        {
            if (st.Value.information == information)
            {
                ind = st.Value.informationID;

                //since it already exists, the virtual agent is remembering it. Change the activation and weight
                st.Value.activation = st.Value.weight = 1;

                break;
            }
        }

        //if did not find it in STM, it may be in LTM. So, lets check
        if (ind == 0)
        {
            foreach (KeyValuePair<int,MemoryClass> st in agentLongTermMemory)
            {
                if (st.Value.information == information)
                {
                    ind = st.Value.informationID;

                    //since it already exists, the virtual agent is remembering it. Change the activation and weight
                    st.Value.activation = st.Value.weight = 1;

                    //also, since it is remembering, it should be back to STM
                    backToSTM = true;

                    break;
                }
            }
        }

        //if ind is zero, we did not find the memory, so it is new. Add it
        //otherwise, if ind is not zero, but backToSTM is true, it means the memory was found in the LTM. Do not create new, but add to STM also
        if (ind == 0 || (ind > 0 && backToSTM))
        {
            //if memory is full (7 itens), forget the oldest information and store at the LTM
            if (agentShortTermMemory.Count == 7)
            {
                //transfer to the LTM
                //agentLongTermMemory.Insert(0, agentShortTermMemory[6]);

                //delete
                //agentShortTermMemory.RemoveAt(6);

                //we delete the less important memory (weight)
                int less = -1;
                float minWeight = 1;
                foreach (KeyValuePair<int,MemoryClass> mc in agentShortTermMemory)
                {
                    if (mc.Value.weight < minWeight)
                    {
                        minWeight = mc.Value.weight;
                        less = mc.Value.informationID;
                    }
                }

                if (less != -1)
                {
                    //transfer to the LTM
                    agentLongTermMemory.Add(agentShortTermMemory[less].informationID, agentShortTermMemory[less]);

                    //delete
                    agentShortTermMemory.Remove(less);
                }
            }

            //add the new memory at the beggining of the memory
            //just generate new if ind == 0 
            MemoryClass newMemory = null;
            if (ind == 0)
            {
                if (nodeId > -1)
                {
                    ind = nodeId;
                }
                else
                {
                    ind = GenerateEskID();
                }
                newMemory = new MemoryClass(System.DateTime.Now, informationType, information, ind, weight);
                agentShortTermMemory.Add(ind, newMemory);
            }//else, it already exists in the LTM or in the STM.
            else
            {
                //if backToSTM is false, it is in the STM. So, does nothing
                //otherwise, it is in the LTM. Bring it to the STM
                if (backToSTM)
                {
                    foreach (KeyValuePair<int, MemoryClass> ltm in agentLongTermMemory)
                    {
                        if (ltm.Value.informationID == ind)
                        {
                            newMemory = ltm.Value;
                            newMemory.memoryTime = System.DateTime.Now;
                            break;
                        }
                    }

                    agentShortTermMemory.Add(ind, newMemory);
                }
            }
        }

        return ind;
    }

    //add a new general event and return its id
    private int AddGeneralEvent(string informationEvent, List<int> connectNodes)
    {
        //if the memory already contains this general event, or something similar, do not add
        int ind = 0;
        //int qntNodes = 0;
        //int totalNodes = informationEvent.Split(' ').Length;
        foreach (KeyValuePair<int,GeneralEvent> ges in agentGeneralEvents)
        {
            /*qntNodes = 0;
            foreach(MemoryClass mg in agentGeneralEvents[i].nodes)
            {
                if (informationEvent.Contains(mg.information))
                {
                    qntNodes++;
                }
            }*/

            if (informationEvent == ges.Value.information)
            {
                ind = ges.Key;
                break;
            }
        }

        if (ind > 0)
        {
            //although we do not add a new general event, we can update the information
            agentGeneralEvents[ind].nodes.Clear();
            //agentGeneralEvents[ind].eventType = typeEvent;
            agentGeneralEvents[ind].information = informationEvent;
            //agentGeneralEvents[ind].polarity = lastPolarity;
            //add the updated memory nodes on this event
            foreach (KeyValuePair<int, MemoryClass> mc in agentShortTermMemory)
            {
                if (connectNodes.Contains(mc.Value.informationID) && !agentGeneralEvents[ind].nodes.Contains(mc.Value))
                {
                    agentGeneralEvents[ind].nodes.Add(mc.Value);
                }
            }

            return 0;
        }

        //create a new general event
        int geId = GenerateEpisodeID();
        GeneralEvent ge = new GeneralEvent(System.DateTime.Now, informationEvent, geId);

        //set the polarity
        //ge.polarity = lastPolarity;

        //add the memory nodes on this event
        foreach (KeyValuePair<int,MemoryClass> mc in agentShortTermMemory)
        {
            if (connectNodes.Contains(mc.Value.informationID) && !ge.nodes.Contains(mc.Value))
            {
                ge.nodes.Add(mc.Value);
            }
        }

        //add the memory nodes on this event
        foreach (KeyValuePair<int, MemoryClass> mc in agentLongTermMemory)
        {
            if (connectNodes.Contains(mc.Value.informationID) && !ge.nodes.Contains(mc.Value))
            {
                ge.nodes.Add(mc.Value);
            }
        }

        //add to list
        agentGeneralEvents.Add(geId, ge);

        return geId;
    }

    //every second, we update the short term memory of the agent
    private IEnumerator ControlSTM()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            //just update if it is awake
            if (!isSleeping)
            {
                //first, delete all old memories (15 seconds)
                List<int> idsToKill = new List<int>();
                foreach (KeyValuePair<int, MemoryClass> stm in agentShortTermMemory)
                {
                    //UnityEngine.Debug.Log(System.DateTime.Now - agentShortTermMemory[k].memoryTime);
                    if (System.DateTime.Now - stm.Value.memoryTime >= memorySpan)
                    {
                        //check if this memory does not already exists in long term
                        bool exists = false;
                        foreach (KeyValuePair<int, MemoryClass> lt in agentLongTermMemory)
                        {
                            if (lt.Value.information == stm.Value.information)
                            {
                                exists = true;
                                break;
                            }
                        }

                        if (!exists)
                        {
                            //before delete, transfer to LTM
                            agentLongTermMemory.Add(stm.Value.informationID, stm.Value);
                        }

                        //byyyeeee
                        //agentShortTermMemory.Remove(stm.Key);
                        idsToKill.Add(stm.Key);
                    }
                }
                foreach(int kill in idsToKill) agentShortTermMemory.Remove(kill);

                //memory decay
                foreach (KeyValuePair<int, MemoryClass> mem in agentShortTermMemory)
                {
                    System.TimeSpan memTime = System.DateTime.Now - mem.Value.memoryTime;
                    //UnityEngine.Debug.Log(memTime.Seconds);

                    if (memTime.Seconds > 1)
                    {
                        //exponential function for our interval, using log
                        mem.Value.activation = Mathf.Log(mem.Value.activation + 1);
                        //UnityEngine.Debug.Log(mem.activation);

                        //if activation drops below 0.2, loses a bit weight also
                        //update: if has max weight, memory node is permanent
                        if (mem.Value.activation < 0.2f && mem.Value.weight < 0.9)
                        {
                            mem.Value.weight = Mathf.Log(mem.Value.weight + 1);
                        }
                    }
                }
            }
        }
    }

    //save episodic memory file
    private void SaveEpisodic()
    {
        //save LTM as it is
        StreamWriter writingLTM;
        writingLTM = File.CreateText("AutobiographicalStorage/episodicMemory.txt");

        //first, save the ESK
        foreach (KeyValuePair<int, MemoryClass> mem in agentLongTermMemory)
        {
            //ID;Timestamp;Information;Type;Activation;Weight
            writingLTM.WriteLine(mem.Key.ToString() + ";" + mem.Value.memoryTime + ";" + mem.Value.information.Trim() + ";"
                + mem.Value.informationType.ToString() + ";" + mem.Value.activation.ToString() + ";" + mem.Value.weight.ToString());
        }

        //second, we save the episodes
        writingLTM.WriteLine("%%%");
        foreach (KeyValuePair<int, GeneralEvent> mem in agentGeneralEvents)
        {
            //get the nodes first
            string allNodes = "";
            foreach(MemoryClass mc in mem.Value.nodes)
            {
                if (allNodes == "") allNodes = mc.informationID.ToString();
                else allNodes += "_"+mc.informationID.ToString();
            }

            //ID;Timestamp;Information;Nodes
            writingLTM.WriteLine(mem.Key.ToString() + ";" + mem.Value.eventTime + ";" + mem.Value.information.Trim() + ";" + allNodes);
        }

        writingLTM.Close();
    }

    public void SleepAgent()
    {
        //put agent to sleep
        isSleeping = true;
        CloseEyesAndDream();

        //change button
        sleepButton.GetComponentInChildren<Text>().text = "Wake up";
        sleepButton.GetComponentInParent<Button>().onClick.RemoveAllListeners();
        sleepButton.GetComponentInParent<Button>().onClick.AddListener(WakeAgent);

        //show zzz
        zzz.SetActive(true);

        //consolidate LTM
        MemoryREM();
    }

    public void WakeAgent()
    {
        //come ooooonnn!!!
        isSleeping = false;
        OpenEyes();

        //change button
        sleepButton.GetComponentInChildren<Text>().text = "Sleep";
        sleepButton.GetComponentInParent<Button>().onClick.RemoveAllListeners();
        sleepButton.GetComponentInParent<Button>().onClick.AddListener(SleepAgent);

        //hide zzz
        zzz.SetActive(false);
    }

    private void CloseEyesAndDream()
    {
        //call the character animation to sleepy leepy
        mariano.GetComponent<CharacterCTRL>().PlayAnimation("sleep");
    }

    private void OpenEyes()
    {
        //call the character animation to good morning sunshine
        mariano.GetComponent<CharacterCTRL>().PlayAnimation("wakywaky");
    }

    //save the used small talks
    private void SaveUsedST()
    {
        if (dialogsInMemory.Count > 0)
        {
            //save LTM as it is
            StreamWriter writingLTM = File.CreateText("AutobiographicalStorage/smallTalksUsed.txt");

            //first, save the ESK
            foreach (string dmem in dialogsInMemory)
            {
                //str with the used ones
                writingLTM.WriteLine(dmem);
            }
            writingLTM.Close();

            //also, save the file for Scherer
            writingLTM = File.CreateText("AutobiographicalStorage/schererFile.txt");

            for (int i = 0; i < dialogsInMemory.Count; i++)
            {
                //str with the used ones
                string[] info = dialogsInMemory[i].Split('-');
                writingLTM.WriteLine(info[2] + "+" + info[3] + "+" + dialogsAnswersInMemory[i]);
            }
            writingLTM.Close();
        }
    }

    //consolidate memory on REM sleep
    private void MemoryREM()
    {
        //copy from STM to LTM
        foreach (KeyValuePair<int,MemoryClass> stm in agentShortTermMemory)
        {
            //check if this memory does not already exists in long term
            bool exists = false;
            foreach (KeyValuePair<int,MemoryClass> lt in agentLongTermMemory)
            {
                if (lt.Value.information == stm.Value.information)
                {
                    exists = true;
                    break;
                }
            }

            //if does not exist, copy
            if (!exists)
            {
                agentLongTermMemory.Add(stm.Value.informationID, stm.Value);
            }
        }

        //clean STM
        agentShortTermMemory.Clear();

        //first: all memory nodes with low activation have their respective weights lowered
        //update: memory nodes with weight 1 are considered permanent
        foreach (KeyValuePair<int,MemoryClass> memC in agentLongTermMemory)
        {
            if (memC.Value.activation < 0.2f && memC.Value.weight < 0.9)
            {
                memC.Value.weight = Mathf.Log(memC.Value.weight + 1);
            }
        }

        //all memory nodes with low weight are removed
        foreach (KeyValuePair<int, MemoryClass> memC in agentLongTermMemory)
        {
            if (memC.Value.weight <= weightThreshold)
            {
                //get its ID, so we can remove from general events also
                int memId = memC.Value.informationID;

                //check general events with this ID
                foreach (KeyValuePair<int, GeneralEvent> ge in agentGeneralEvents)
                {
                    if (ge.Value.nodes.Contains(memC.Value))
                    {
                        int i = 0;
                        foreach (MemoryClass nodis in ge.Value.nodes)
                        {
                            if(nodis.informationID == memId)
                            {
                                ge.Value.nodes.RemoveAt(i);
                                break;
                            }
                            i++;
                        }   
                    }
                }

                //remove the memory itself
                agentLongTermMemory.Remove(memId);
            }
        }

        //now, lets check if the nodes in the General Events still exist. Otherwise, kill them!
        /*foreach (GeneralEvent ge in agentGeneralEvents)
        {
            int nodesCount = ge.nodes.Count;
            for (int y = 0; y < nodesCount; y++)
            {
                bool found = false;
                foreach (MemoryClass mc in agentLongTermMemory)
                {
                    if (mc.informationID == ge.nodes[y].informationID)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    ge.nodes.RemoveAt(y);
                    nodesCount--;
                }
            }
        }*/

        //after removing memories, check the general events which have no more nodes
        List<int> idesKill = new List<int>();
        foreach (KeyValuePair<int, GeneralEvent> ge in agentGeneralEvents)
        {
            //if after we remove, there are no more nodes, the event itself is not important
            if (ge.Value.nodes.Count == 0)
            {
                idesKill.Add(ge.Key);
            }
        }
        foreach(int kill in idesKill) {
            agentGeneralEvents.Remove(kill);
        }

        //delete information from LTM.
        //Basically, we save the new LTM file with just complete information.
        SaveEpisodic();

        //clean LTM
        agentLongTermMemory.Clear();
    }

    //consolidate memory on REM sleep (Graph mode)
    /*private void MemoryREM()
    {
        StartCoroutine(ConsolidationWebService());
    }*/

    /*IEnumerator ConsolidationWebService()
    {
        UnityWebRequest www = new UnityWebRequest(webServicePath + "neo4jTransaction", "POST");
        string jason = "{\"typeTransaction\" : [\"matchNode\"], \"match\" : [\"match (n) return n.name,n.weight,n.activation\"]}";
        //UnityEngine.Debug.Log(jason);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jason);
        //byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes("\"typeTransaction\" : [\"createNode\"]");
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        www.SetRequestHeader("Content-Type", "application/json");

        using (www)
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                UnityEngine.Debug.Log(www.error);
                canDestroy = true;
            }
            else
            {
                //UnityEngine.Debug.Log("Match: " + www.downloadHandler.text);

                string temp = www.downloadHandler.text.Replace("\"", "");
                temp = temp.Replace("\\n.name\\:", "?");
                temp = temp.Replace("}", "");
                //UnityEngine.Debug.Log("oi: " + temp);
                string[] temp2 = temp.Split('?');
                foreach(string tmp in temp2)
                {
                    if (!tmp.Contains(",")) continue;

                    string node;
                    float nodeAcv, nodeWeight;
                    //UnityEngine.Debug.Log("-: " + tmp);
                    string[] sprt = tmp.Split(',');

                    node = sprt[0].Replace("\\", "");

                    string[] inf = sprt[1].Split(':');
                    nodeWeight = float.Parse(inf[1]);

                    inf = sprt[2].Split(':');
                    nodeAcv = float.Parse(inf[1]);

                    //UnityEngine.Debug.Log("ID: " + node + " - Weight: " + nodeWeight + " - Activation: " + nodeAcv);

                    if (nodeAcv < 0.2f && nodeWeight < 0.9)
                    {
                        nodeWeight = Mathf.Log(nodeWeight + 1);

                        //if the weight is too low, remove
                        if(nodeWeight < weightThreshold)
                        {
                            StartCoroutine(DeleteMemoryNodeWebService(node));
                        }//else, just update info
                        else
                        {
                            StartCoroutine(UpdateMemoryNodeWebService(node, "weight", nodeWeight.ToString()));
                        }
                    }
                }

                canDestroy = true;
            }
        }
    }*/

    //retrieve a memory based on cues
    //deactivated for now
    private void GenerativeRetrieval(Dictionary<string, string> cues)
    {
        GeneralEvent eventFound = new GeneralEvent();
        Dictionary<string, string> auxCues = new Dictionary<string, string>();
        foreach(KeyValuePair<string,string> cue in cues)
        {
            if (cue.Key == "old" || cue.Key == "age") auxCues.Add("born", cue.Value);
            else if (cue.Key == "working") auxCues.Add("work", cue.Value);
            else if (cue.Key == "studying") auxCues.Add("study", cue.Value);
            else if (cue.Key == "children" || cue.Key == "kids") auxCues.Add("has children", cue.Value);
            else auxCues.Add(cue.Key, cue.Value);

            //we can also try to infer some things. For example, if asks if has brother/sister, we can search for has children
            if (cue.Key == "brother" || cue.Key == "sister" || cue.Key == "sibling" || cue.Key == "father" ||
                cue.Key == "mother" || cue.Key == "parent") auxCues.Add("has children", cue.Value);
        }
        cues = auxCues;
        //auxCues.Clear();

        //look for similar words
        string textParam = "";
        foreach(KeyValuePair<string,string> cue in cues)
        {
            if (textParam == "") textParam = cue.Key;
            else textParam += "-" + cue.Key;
        }

        //retrieving memory
        isRetrievingMemory = true;

        StartCoroutine(WordVecWebService(textParam, cues));
    }

    //new generative retrieval, based on the graph memory
    /*void GenerativeRetrieval(Dictionary<string, string> cues)
    {
        string match = "";
        string whoIsIt = "";
        
        List<string> verbs = new List<string>();
        List<string> nouns = new List<string>();
        foreach (KeyValuePair<string,string> cu in cues)
        {
            //first of all: since NLTK understands some verbs as nouns, lets make a work around for the icebreakers ones
            if(cu.Key == "work" || cu.Key == "works" || cu.Key == "working" || cu.Key == "study" || cu.Key == "studies" || cu.Key == "studying")
            {
                verbs.Add(cu.Key);
            }//if verb is "have" and children is involved, add
            else if(cu.Key == "have" && (cues.ContainsKey("children") || cues.ContainsKey("kids") || cues.ContainsKey("child")))
            {
                verbs.Add("HAS_CHILD");
            }
            else if(cu.Value == "VB" || cu.Value == "VBP")
            {
                if(cu.Key.ToLower() != "do" && cu.Key.ToLower() != "does" && cu.Key.ToLower() != "are" && cu.Key.ToLower() != "were" && cu.Key.ToLower() != "was" && cu.Key.ToLower() != "am" && cu.Key.ToLower() != "be")
                verbs.Add(cu.Key);
            }else if (cu.Value == "NN" || cu.Value == "NNP")
            {
                nouns.Add(cu.Key);

                //if (cu.Key == personName) whoIsIt = cu.Key;
                /*else* if (cu.Key == "Arthur") whoIsIt = cu.Key;
            }
        }

        if (whoIsIt == "" && cues.ContainsKey(personName)) whoIsIt = personName;

        //if we have verbs on the sentence, we try to use them for the relationships
        if (verbs.Count > 0)
        {
            match = "MATCH ";
            char letter = 'a';

            foreach(string vb in verbs)
            {
                string useVerb = vb.ToUpper();
                if (useVerb == "KNOW") useVerb = "KNOWS";
                if (useVerb == "STUDY") useVerb = "IS_STUDYING";
                if (useVerb == "WORK") useVerb = "IS_WORKING";

                //if using verb KNOWS, maybe it is more like MET
                if(nouns.Contains("Arthur") && nouns.Contains(personName))
                {
                    useVerb = "MET";
                }

                int added = 0;
                
                if (letter == 'a')
                {
                    foreach(string nn in nouns)
                    {
                        if(nn != "Arthur" && nn != personName)
                        {
                            match += "({name:'"+whoIsIt+"'})-[:" + useVerb + "]->(" + letter + " {name:'"+nn+"'})";
                            added++;
                        }
                    }
                }
                else
                {
                    foreach (string nn in nouns)
                    {
                        if (nn != "Arthur" && nn != personName)
                        {
                            match += ", ({name:'" + whoIsIt + "'})-[:" + useVerb + "]->(" + letter + " {name:'" + nn + "'})";
                            added++;
                        }
                    }
                }

                //if did not added anything, but has nouns, it means just have Arthur or the person
                if (added == 0 && nouns.Count > 0)
                {
                    //if we have only Arthur as noun, it is a question about him
                    if (nouns.Contains("Arthur") && nouns.Count == 1)
                    {
                        match += "(" + letter + " {name:'" + whoIsIt + "'})";
                    }
                    else
                    {
                        match += "(" + letter + " {name:'" + whoIsIt + "'})-[:" + useVerb + "]->(" + ++letter + ")";
                    }
                }

                letter++;
            }

            match += " return ";
            do
            {
                letter--;
                if(letter != 'a')
                {
                    match += letter + ",";
                }
                else
                {
                    match += letter;
                }
            } while (letter != 'a');
        }//else, just find it all
        else
        {
            match = "MATCH (n) WHERE ";

            //for each cue
            foreach (string cue in nouns)
            {
                //if it is the first, ok.
                if (match == "MATCH (n) WHERE ")
                {
                    match += "n.name='" + cue + "'";
                }//otherwise, need the comma to separate
                else
                {
                    match += " OR n.name='" + cue + "'";
                }
            }

            //done, add the return on the match
            match += " return n";
        }

        UnityEngine.Debug.LogWarning(match);
        StartCoroutine(MatchWebService(match, false, cues));
    }*/

    /*public void FollowFace(Vector3 point)
    {
        foreach (GameObject eye in eyes)
        {
            eye.GetComponent<EyeController>().FollowFace(point);
        }
    }*/

    //timer to take a picture
    private void StartTimer()
    {
        timerPicture = timeToPicture;

        //set the timer object
        timerObject.GetComponent<Text>().text = timerPicture.ToString();
        timerObject.SetActive(true);

        StartCoroutine("LoseTime");
    }

    private IEnumerator LoseTime()
    {
        while (timerPicture > 0)
        {
            yield return new WaitForSeconds(1);
            timerPicture--;
            timerObject.GetComponent<Text>().text = timerPicture.ToString();
        }

        //save image
        cam.GetComponent<ViewCam>().StartSaveImageCoRo("AutobiographicalStorage/Images/" + importantNoun + ".png");

        //create the memory
        int newId = AddToSTM("Object", importantNoun, 1);

        //create a new autobiographical storage for the thing image
        int newIdImage = AddToSTM("Imagery", "AutobiographicalStorage/Images/" + importantNoun + ".png", 1);

        //save on Neo4j
        //on temp, we have to find 2 information later
        /*qntTempNodes = 2;
        string label = "name:'" + importantNoun + "',activation:1,weight:0.9,nodeType:'text',lastEmotion:'" + foundEmotion + "',image:'AutobiographicalStorage/Images/" + importantNoun + ".png'";
        StartCoroutine(CreateMemoryNodeWebService(importantNoun, "Thing", label, 0.9f));

        label = "name:'thing',image:'AutobiographicalStorage/Images/" + importantNoun + ".png',activation:1,weight:0.9,nodeType:'image',lastEmotion:'" + foundEmotion + "'";
        StartCoroutine(CreateMemoryNodeWebService("thing", "Image", label, 0.9f));

        //type of the event, to save later
        tempTypeEvent = "learn thing";
        tempRelationship = "HAS_PHOTO";
        arthurLearnsSomething = true;*/

        List<int> connectNodes = new List<int>();
        connectNodes.Add(newId);
        connectNodes.Add(newIdImage);
        connectNodes.Add(1);
        connectNodes.Add(12);

        //create a new general event
        AddGeneralEvent("I learned what a " + importantNoun + " is", connectNodes);

        //reset
        isYesNoQuestion = false;
        importantNoun = "";
        yesNoAnswer = "";
        timerObject.SetActive(false);

        //ok, learned...
        string responseText = "Nice, thanks!";
        SpeakYouFool(responseText);
    }

    //check memory for an icebreaker
    /*private bool FindIceBreakerInMemory(int whichIceBreaker)
    {
        IceBreakingTreeClass thisIceBreaker = iceBreakers.FindIcebreaker(whichIceBreaker);

        //check general events
        foreach (GeneralEvent geez in agentGeneralEvents)
        {
            //for each node
            foreach (MemoryClass node in geez.nodes)
            {
                //if it has both the name of the person and the type of the icebreaker, already exists
                if (node.information.Contains(thisIceBreaker.GetType()) && node.information.Contains(personName))
                {
                    return true;
                }
            }
        }

        return false;
    }*/

    //breaking the ice!
    private void BreakIce(string beforeText = "")
    {
        saveNewMemoryNode = false;

        //reset the idle timer
        idleTimer = Time.time;

        IceBreakingTreeClass actualIceBreaker = iceBreakers.FindIcebreaker(usingIceBreaker);

        //first, lets check if the actual icebreaker has an influencer, IF it is not yet influencing
        if (!isInfluencing)
        {
            int actualId = actualIceBreaker.GetId();
            if (influencer.ContainsKey(actualId))
            {
                //just try to influence if the answer has a contrary polarity
                if ((actualIceBreaker.GetPolarity() == true && lastPolarity < 0) ||
                    (actualIceBreaker.GetPolarity() == false && lastPolarity > 0))
                {
                    isInfluencing = !isInfluencing;

                    SpeakYouFool(beforeText + influencer[actualId]);
                    return;
                }
            }
        }
        else
        {
            //if was influencing, toggle it back
            if (isInfluencing) isInfluencing = !isInfluencing;
        }

        
        //just follow the tree
        //if still not using any, get the first
        if (usingIceBreaker == 0)
        {
            usingIceBreaker = iceBreakers.GetChild(0).GetId();
            //rootIceBreaker = iceBreakers.GetChild(0).GetId();
            rootIceBreaker = iceBreakers.GetId();

            //check the memory for this little motherfucker
        }
        else
        {
            //othewise, we check if this icebreaker has children. 
            //If it has, it means it has an alternative route depending on the answer of the person
            if (actualIceBreaker.QntChildren() > 0)
            {
                //now we check which question is it
                //if it is one of the first levels, we check the polarity of the answer: if it is opposite of what was expected, we take the route
                if (actualIceBreaker.GetParent().GetId() == 0)
                {
                    if ((actualIceBreaker.GetPolarity() == true && lastPolarity < 0) ||
                        (actualIceBreaker.GetPolarity() == false && lastPolarity > 0))
                    {
                        //if needed further ahead, here we would keep the root
                        //rootIceBreaker = actualIceBreaker.GetId();

                        //down the hill
                        usingIceBreaker = actualIceBreaker.GetChild(0).GetId();
                    }
                    else
                    {
                        //otherwise, we just get next
                        int thisChild = actualIceBreaker.CheckWhichChild();

                        //next one
                        thisChild++;

                        //see if the parent has more children
                        if (iceBreakers.FindIcebreaker(rootIceBreaker).QntChildren() > thisChild)
                        {
                            usingIceBreaker = iceBreakers.FindIcebreaker(rootIceBreaker).GetChild(thisChild).GetId();
                        }//otherwise, we are done
                        else
                        {
                            usingIceBreaker = rootIceBreaker = -1;
                        }
                    }
                }//otherwise, just keep going
                else
                {
                    usingIceBreaker = actualIceBreaker.GetChild(0).GetId();
                }
            }
            //Otherwise, we can just go on to the next
            else
            {
                int thisChild = actualIceBreaker.CheckWhichChild();

                //next one
                thisChild++;

                //see if the parent has more children
                if (iceBreakers.FindIcebreaker(rootIceBreaker).QntChildren() > thisChild)
                {
                    usingIceBreaker = iceBreakers.FindIcebreaker(rootIceBreaker).GetChild(thisChild).GetId();
                }//otherwise, we are done
                else
                {
                    usingIceBreaker = rootIceBreaker = -1;
                }
            }
        }
        
        //if found some icebreaker to still use, icebreak should not be empty
        if (usingIceBreaker > 0)
        {
            //before we speak, we should check the memory to see if this questions was already answered before.
            IceBreakingTreeClass target = iceBreakers.FindIcebreaker(usingIceBreaker);

            string targetType = target.GetType();
            if (targetType == "old") targetType = "born";
            if (targetType == "working") targetType = "job";
            if (targetType == "study") targetType = "studying";

            //so, lets try to find some general event
            GeneralEvent fuck = null;

            foreach (KeyValuePair<int,GeneralEvent> geez in agentGeneralEvents)
            {
                //if it exists, ding!
                
                if (geez.Value.information.Contains(personName) && geez.Value.information.Contains(targetType))
                {
                    fuck = geez.Value;
                    break;
                }
            }

            //if found it, we change the question to reflect the previous knowledge
            //update: here, we do not make questions again. We just dont call icebreakers
            if (fuck != null)
            {
                //we check the polarity of the answer.
                /*if (fuck.polarity > 0)
                {
                    SpeakYouFool(beforeText + positiveAnswer[target.GetId()]);
                }
                else if (fuck.polarity < 0)
                {
                    SpeakYouFool(beforeText + negativeAnswer[target.GetId()]);
                }
                else
                {
                    SpeakYouFool(beforeText + target.GetQuestion());
                }*/
                BreakIce(beforeText);
                return;
            }//otherwise, just make the question
            else
            {
                SpeakYouFool(beforeText + target.GetQuestion());
            }
        }//else, there is no more to talk about. Stop it
        else
        {
            if (beforeText != "")
            {
                SpeakYouFool(beforeText);
            }
            else
            {
                SpeakYouFool("Thanks! Anything else you would like to talk about?");
            }

            isBreakingIce = false;
        }
    }

    //new small talking
    private void PickTopic()
    {
        if (topics.Count == 0) return;
        var rnd = new System.Random(DateTime.Now.Millisecond);
        int index = rnd.Next(0, topics.Count);
        currentTopic = topics[index];
        topics.Remove(currentTopic);
    }

    private void SmallTalking(string beforeText = "")
    {
        //if topics is empty, we are done
        if (topics.Count == 0 && !currentTopic.IsDialogsAvailable()) return;

        string ct;
        idleTimer = Time.time;
        saveNewMemoryNode = false;
        bool first = false;

        if (!currentTopic.IsDialoging()) //sort new dialog
        {
            if (!currentTopic.IsDialogsAvailable()) PickTopic(); //there isnt available dialogs in current topic

            currentTopic.StartNewDialog();
            first = true;
        }

        if (first)
        {
            ct = currentTopic.RunDialog(0, dialogsInMemory);
        }
        else
        {
            ct = currentTopic.RunDialog(lastPolarity, dialogsInMemory);
        }

        if (ct != null)
        {
            string digmem = currentTopic.GetId() + "-" + currentTopic.GetCurrentDialog().GetDescription() + "-" + currentTopic.GetCurrentDialog().GetId().ToString() + "-" + currentTopic.GetCurrentDialog().GetTreeLevel().ToString();
            if (!dialogsInMemory.Contains(digmem))
            {
                dialogsInMemory.Add(digmem);
                SpeakYouFool(ct);
            }
        }

        UnityEngine.Debug.Log(ct);
    }

    //hide the random image
    public void HideRandomImage()
    {
        randomImage.SetActive(false);
        riTarget.SetActive(false);
    }

    //checks if there are new tokens in the file
    private void CheckNewTokens()
    {
        if (new FileInfo("resultToken.txt").Length > 0)
        {
            saveNewMemoryNode = true;
        }
    }

    //load the icebreakers and respective answers
    private void LoadIceBreakersAndStuff()
    {
        string iceBreakerFile = "iceBreakers.txt";
        string positiveFile = "positiveAnswers.txt";
        string negativeFile = "negativeAnswers.txt";
        string influencerFile = "influencer.txt";

        StreamReader readingLTM = new StreamReader(iceBreakerFile, System.Text.Encoding.Default);
        using (readingLTM)
        {
            string line;
            do
            {
                line = readingLTM.ReadLine();

                if (line != "" && line != null)
                {
                    //skip comments
                    if (line.Contains("#")) continue;

                    //memory time;person;emotion
                    string[] info = line.Split(';');
                    int ibId = int.Parse(info[0]);
                    string ibType = info[1];
                    string ibQuestion = info[2];
                    bool ibPolarity = bool.Parse(info[3]);
                    int ibParent = int.Parse(info[4]);

                    //if parent is 0, is one of the primary ones
                    if (ibParent == 0)
                    {
                        iceBreakers.AddChild(new IceBreakingTreeClass(ibId, ibType, ibQuestion, ibPolarity));
                    }//otherwise, it is one of the secondary ones. Need to first find the parent and, then, add
                    else
                    {
                        iceBreakers.FindIcebreaker(ibParent).AddChild(new IceBreakingTreeClass(ibId, ibType, ibQuestion, ibPolarity));
                    }
                }
            } while (line != null);
        }
        readingLTM.Close();

        readingLTM = new StreamReader(positiveFile, System.Text.Encoding.Default);
        using (readingLTM)
        {
            string line;
            do
            {
                line = readingLTM.ReadLine();

                if (line != "" && line != null)
                {
                    //skip comments
                    if (line.Contains("#")) continue;

                    //memory time;person;emotion
                    string[] info = line.Split(';');
                    int ibId = int.Parse(info[0]);
                    string ibQuestion = info[1];

                    positiveAnswer.Add(ibId, ibQuestion);
                }
            } while (line != null);
        }
        readingLTM.Close();

        readingLTM = new StreamReader(negativeFile, System.Text.Encoding.Default);
        using (readingLTM)
        {
            string line;
            do
            {
                line = readingLTM.ReadLine();

                if (line != "" && line != null)
                {
                    //skip comments
                    if (line.Contains("#")) continue;

                    //memory time;person;emotion
                    string[] info = line.Split(';');
                    int ibId = int.Parse(info[0]);
                    string ibQuestion = info[1];

                    negativeAnswer.Add(ibId, ibQuestion);
                }
            } while (line != null);
        }
        readingLTM.Close();

        readingLTM = new StreamReader(influencerFile, System.Text.Encoding.Default);
        using (readingLTM)
        {
            string line;
            do
            {
                line = readingLTM.ReadLine();

                if (line != "" && line != null)
                {
                    //skip comments
                    if (line.Contains("#")) continue;

                    //memory time;person;emotion
                    string[] info = line.Split(';');
                    int ibId = int.Parse(info[0]);
                    string ibQuestion = info[1];

                    influencer.Add(ibId, ibQuestion);
                }
            } while (line != null);
        }
        readingLTM.Close();
    }

    public void LoadSmallTalk()
    {
        string smallTalkFile = "smallTalk.txt";

        StreamReader readingLTM = new StreamReader(smallTalkFile, System.Text.Encoding.Default);
        using (readingLTM)
        {
            string line;
            //Dictionary<int, Node> aux = new Dictionary<int, Node>();
            topics = new List<Topic>();
            Topic currentTopic = null;
            Dialog currentDialog = null;
            int lastParent = -1;
            int treeLevel = 0;

            do
            {

                line = readingLTM.ReadLine();
                if (line == "" || line == null) continue;
                line = line.Trim();
                char command = line[0];
                line = (line.Substring(1, line.Length - 1)).Trim();

                //new topic
                if (command.Equals('$'))
                {
                    currentTopic = new Topic(line);
                    topics.Add(currentTopic);
                }
                //new dialog
                else if (command.Equals('['))
                {
                    currentDialog = new Dialog(line);
                }
                //dialog
                else if (command.Equals('#'))
                {

                    //id, sentence, polarity, isLeaf, father id, memory edge, memory node value, tree level
                    string[] data = line.Split(';');

                    int parentID = int.Parse(data[4]);

                    //when the parent changes, the tree level changes as well
                    //here is the tricky part: even if parents are in the same level, the treeLevel updates, which is bad =/
                    if(parentID != lastParent)
                    {
                        lastParent = parentID;
                        treeLevel++;
                    }

                    currentDialog.AddNode(int.Parse(data[0]), data[1].Trim(), double.Parse(data[2]), bool.Parse(data[3].Trim()), parentID, data[5].Trim(), data[6].Trim(), treeLevel);
                }
                //close dialog (insert on topic)
                else if (command.Equals(']'))
                {
                    currentDialog.CloseInsertion();
                    currentTopic.InsertDialog(currentDialog.GetDescription(), currentDialog);
                }

            } while (line != null);
        }

        if (topics.Count >= 1)
        {
            UnityEngine.Debug.Log("leu os small talks");
        }

        readingLTM.Close();
    }

    //load small talks saved in memory
    private void LoadMemoryDialogs()
    {
        //StartCoroutine(MatchTopicsDialogs());

        StreamReader readingLTM = new StreamReader("AutobiographicalStorage/smallTalksUsed.txt", System.Text.Encoding.Default);

        using (readingLTM)
        {
            string line;
            do
            {
                line = readingLTM.ReadLine();

                if (line != "" && line != null)
                {
                    dialogsInMemory.Add(line);
                }
            } while (line != null);
        }
        readingLTM.Close();
    }

    //load beliefs
    private void LoadBeliefs()
    {
        StreamReader readingLTM = new StreamReader("beliefs.txt", System.Text.Encoding.Default);

        using (readingLTM)
        {
            string line;
            do
            {
                line = readingLTM.ReadLine();

                if (line != "" && line != null)
                {
                    prolog.ConsultFromString(line + ".");
                }
            } while (line != null);
        }
        readingLTM.Close();
    }

    /*private IEnumerator MatchTopicsDialogs()
    {
        string match = "match (a:Topic)-[]->(b:Dialog) return a,b";
        UnityWebRequest www = new UnityWebRequest(webServicePath + "neo4jTransaction", "POST");
        string jason = "{\"typeTransaction\" : [\"matchNode\"], \"match\" : [\"" + match + "\"]}";
        //UnityEngine.Debug.Log(jason);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jason);
        //byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes("\"typeTransaction\" : [\"createNode\"]");
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        www.SetRequestHeader("Content-Type", "application/json");

        using (www)
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                UnityEngine.Debug.Log(www.error);
            }
            else
            {
                UnityEngine.Debug.Log("Match: " + www.downloadHandler.text);
                string batata = www.downloadHandler.text.Replace("\\", "");
                batata = batata.Replace("\"", "");
                batata = batata.Replace("}}}", "?");
                batata = batata.Replace("}", "");
                batata = batata.Replace("{", "");
                string[] info = batata.Split('?');

                foreach(string inf in info)
                {
                    if (!inf.Contains("a:")) continue;
                    //UnityEngine.Debug.Log("Match: " + inf);

                    string nameToAdd = "";

                    string treat = inf.Replace("a:", "?");
                    string[] top = treat.Split('?');
                    treat = top[1].Replace("b:", "?");
                    //dia has both topic dialog description [0] and dialog [1]
                    string[] dia = treat.Split('?');
                    //UnityEngine.Debug.Log("Match: " + dia[0] + " - " + dia[1]);

                    string[] final = dia[0].Split(',');
                    foreach(string fin in final)
                    {
                        if (fin.Contains("name:"))
                        {
                            string[] jumanji = fin.Split(':');
                            nameToAdd += jumanji[1];
                            break;
                        }
                    }

                    final = dia[1].Split(',');
                    foreach (string fin in final)
                    {
                        if (fin.Contains("name:"))
                        {
                            string[] jumanji = fin.Split(':');
                            jumanji[1] = jumanji[1].Replace("Dialog", "");
                            nameToAdd += jumanji[1];
                            break;
                        }
                    }

                    dialogsInMemory.Add(nameToAdd);
                }
            }
        }
    }*/

    //create prolog facts from memory
    private void CreateFactsFromMemory()
    {
        //for each general event, we create one or more prolog facts
        //we do not add directly because ConsultFromStrong does not allow to define same facts separately. So, we need to add it in the same command
        Dictionary<string, string> facts = new Dictionary<string, string>();
        foreach(KeyValuePair<int, GeneralEvent> ge in agentGeneralEvents)
        {
            //get all nodes separated by type
            List<MemoryClass> person = new List<MemoryClass>();
            List<MemoryClass> location = new List<MemoryClass>();
            List<MemoryClass> time = new List<MemoryClass>();
            List<MemoryClass> activity = new List<MemoryClass>();
            List<MemoryClass> emotion = new List<MemoryClass>();
            List<MemoryClass> imagery = new List<MemoryClass>();
            List<MemoryClass> objects = new List<MemoryClass>();
            foreach (MemoryClass mem in ge.Value.nodes)
            {
                if (mem.informationType == "Person") person.Add(mem);
                if (mem.informationType == "Location") location.Add(mem);
                if (mem.informationType == "Time") time.Add(mem);
                if (mem.informationType == "Activity") activity.Add(mem);
                if (mem.informationType == "Emotion") emotion.Add(mem);
                if (mem.informationType == "Imagery") imagery.Add(mem);
                if (mem.informationType == "Object") objects.Add(mem);
            }

            //we create the facts based on the activity (verb)
            if(activity.Count > 0)
            {
                //START ICEBREAKERS AND MEETING
                //if it is "born", we have a Time and a possible location
                if(activity[0].information == "born")
                {
                    string born = "";
                    if (time.Count > 0)
                    {
                        born += "born(" + person[0].information.ToLower() + ", " + time[0].information.ToLower() + "). ";
                    }
                    if (location.Count > 0)
                    {
                        born += "born(" + person[0].information.ToLower() + ", " + location[0].information.ToLower() + "). ";
                    }

                    //if it is empty, add. Otherwise, update
                    if (facts.ContainsKey("born"))
                    {
                        facts["born"] += born;
                    }
                    else
                    {
                        facts.Add("born", born);
                    }
                }//else, if it is "meet", we have Arthur meeting someone with a date of the meeting (which is not needed, i believe)
                else if (activity[0].information == "meet")
                {
                    string meet = "";
                    
                    if (person.Count > 1)
                    {
                        meet += "meet(" + person[0].information.ToLower() + ", " + person[1].information.ToLower() + "). ";
                    }

                    //if it is empty, add. Otherwise, update
                    if (facts.ContainsKey("meet"))
                    {
                        facts["meet"] += meet;
                    }
                    else
                    {
                        facts.Add("meet", meet);
                    }
                }//else, if it is "study", someone is studying or not. If we have 2 activities, one of them is the study course.
                else if (activity[0].information == "study")
                {
                    string study = "";

                    if(activity.Count == 1)
                    {
                        study += "study(" + person[0].information.ToLower() + ", false). ";
                    }else if (activity.Count == 2)
                    {
                        study += "study(" + person[0].information.ToLower() + ", " + activity[1].information.ToLower() + "). ";
                    }

                    //if it is empty, add. Otherwise, update
                    if (facts.ContainsKey("study"))
                    {
                        facts["study"] += study;
                    }
                    else
                    {
                        facts.Add("study", study);
                    }
                }//else, if it is "work", someone is working or not. If we have 2 activities, one of them is the job.
                else if (activity[0].information == "work")
                {
                    string work = "";

                    if (activity.Count == 1)
                    {
                        work += "work(" + person[0].information.ToLower() + ", false). ";
                    }
                    else if (activity.Count == 2)
                    {
                        work += "work(" + person[0].information.ToLower() + ", " + activity[1].information.ToLower() + "). ";
                    }

                    //if it is empty, add. Otherwise, update
                    if (facts.ContainsKey("work"))
                    {
                        facts["work"] += work;
                    }
                    else
                    {
                        facts.Add("work", work);
                    }
                }//else, if it is "has children", someone has children or not. If we have 2 or more person, it has children.
                else if (activity[0].information == "has children")
                {
                    string children = "";

                    if (person.Count == 1)
                    {
                        children += "parent(" + person[0].information.ToLower() + ", false). ";
                    }
                    else if (person.Count > 1)
                    {
                        for(int i = 1; i < person.Count; i++)
                        {
                            children += "parent(" + person[0].information.ToLower() + ", " + person[i].information.ToLower() + "). ";
                        }
                    }

                    //if it is empty, add. Otherwise, update
                    if (facts.ContainsKey("parent"))
                    {
                        facts["parent"] += children;
                    }
                    else
                    {
                        facts.Add("parent", children);
                    }
                }
                //END ICEBREAKERS AND MEETING
                //if not icebreaker, we use activities as rule and Person/Object as parameters
                else
                {
                    List<string> paramer = new List<string>();
                    foreach(MemoryClass prn in person)
                    {
                        paramer.Add(prn.information.ToLower());
                    }
                    foreach (MemoryClass obj in objects)
                    {
                        paramer.Add(obj.information.ToLower());
                    }

                    foreach (MemoryClass acs in activity)
                    {
                        string rule = "";

                        if (paramer.Count == 2)
                        {
                            rule += acs.information + "(" + paramer[0] + ", " + paramer[1] + "). ";
                        }else if(paramer.Count > 2)
                        {
                            for (int i = 1; i < paramer.Count; i++)
                            {
                                rule += acs.information + "(" + paramer[0] + ", " + paramer[i] + "). ";
                            }
                        }

                        //if it is empty, add. Otherwise, update
                        if (facts.ContainsKey(acs.information))
                        {
                            facts[acs.information] += rule;
                        }
                        else
                        {
                            facts.Add(acs.information, rule);
                        }
                    }
                }
            }
        }

        //now, for each fact, we add to Prolog
        foreach(KeyValuePair<string,string> fct in facts)
        {
            prolog.ConsultFromString(fct.Value);
        }
    }

    //Web Service for Tokenization
    private IEnumerator TokenizationWebService(string sentence)
    {
        UnityWebRequest www = new UnityWebRequest(webServicePath + "tokenize", "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes("{\"text\" : [\"" + sentence + "\"]}");
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        www.SetRequestHeader("Content-Type", "application/json");

        using (www)
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                UnityEngine.Debug.Log(www.error);
            }
            else
            {
                //UnityEngine.Debug.Log("Received: " + www.downloadHandler.data);
                WriteTokens(www.downloadHandler.text);
            }
        }
    }

    //Web Service for Word2Vec
    private IEnumerator WordVecWebService(string sentence, Dictionary<string, string> cues)
    {
        UnityWebRequest www = new UnityWebRequest(webServicePath + "similarWords", "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes("{\"text\" : [\"" + sentence + "\"]}");
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        www.SetRequestHeader("Content-Type", "application/json");

        //before sending, lets see the nouns to guess the topic
        List<string> topicSent = new List<string>();
        foreach(KeyValuePair<string, string> cu in cues)
        {
            if(cu.Value == "NN")
            {
                topicSent.Add(cu.Key);
            }else if(cu.Value == "NNP" && cu.Key != personName && cu.Key != agentName)
            {
                topicSent.Add(cu.Key);
            }
        }

        using (www)
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                UnityEngine.Debug.Log(www.error);
            }
            else
            {
                UnityEngine.Debug.Log("Received: " + www.downloadHandler.data);
                //WriteWordVec(www.downloadHandler.text);

                //need to format it properly now
                string info = www.downloadHandler.text.Replace("\"", "");
                info = info.Replace(@"\", "");
                info = info.Replace("},", "@");
                string[] infoSplit = info.Split('@');

                infoSplit[0] = infoSplit[0].Replace("{0:{", "");
                infoSplit[1] = infoSplit[1].Replace("1:{", "");
                infoSplit[1] = infoSplit[1].Replace("}}", "");

                string[] tokens = infoSplit[0].Split(',');
                string[] tknType = infoSplit[1].Split(',');

                for(int h = 0; h < tokens.Length; h++)
                {
                    string[] gt = tokens[h].Split(':');
                    tokens[h] = gt[1];
                }
                for (int h = 0; h < tknType.Length; h++)
                {
                    string[] gt = tknType[h].Split(':');
                    tknType[h] = gt[1];
                }

                //organize cues with similars
                Dictionary<string, string> newCues = new Dictionary<string, string>();

                //each cue has 5 results. So, we take for each of them
                int qntCue = 0;
                foreach(KeyValuePair<string,string> cu in cues)
                {
                    if(!newCues.ContainsKey(cu.Key))
                        newCues.Add(cu.Key, cu.Value);

                    //we just actually use for NN
                    if(cu.Value == "NN")
                    {
                        //5 of this cue
                        for(int i = 0; i < 5; i++)
                        {
                            //ALSO, we just add if the similarity is over 60%
                            if (float.Parse(tknType[5*qntCue+i]) > 0.6f)
                            {
                                if (!newCues.ContainsKey(tokens[5 * qntCue + i]))
                                    newCues.Add(tokens[5 * qntCue + i], cu.Value);
                            }
                        }
                    }
                    qntCue++;
                }

                //we find the general event which has the most cues compounding its memory nodes
                //BUT... select the general event is a bit trickier, since it can exist many events with the same memory information.
                //so, we select the event which has the most cues
                int maxCues = 0;
                //making a test: instead to find just one event, bring all events which have the same amount of cues, and we decide later which one to pick
                //GeneralEvent eventFound = null;
                List<GeneralEvent> eventFound = new List<GeneralEvent>();
                foreach (KeyValuePair<int, GeneralEvent> geez in agentGeneralEvents)
                {
                    //for each general event, we count the cues found
                    int eventCues = 0;
                    bool aboutAgent = false;
                    //for each memory node which compounds this general event
                    foreach (MemoryClass node in geez.Value.nodes)
                    {
                        //if it exists, ++
                        if (newCues.ContainsKey(node.information))
                        {
                            eventCues++;
                        }

                        //we try to avoid finding info about the agent itself
                        //if (node.information == agentName) aboutAgent = true;
                    }

                    //if it is higher than the max cues, select this general event
                    /*if (eventCues > maxCues || (eventCues == maxCues && !aboutAgent))
                    {
                        maxCues = eventCues;
                        eventFound = geez.Value;
                    }*/
                    //if it is higher than the max cues, add this general event
                    if(eventCues > maxCues)
                    {
                        //reset it
                        eventFound.Clear();

                        maxCues = eventCues;
                        eventFound.Add(geez.Value);
                    }//if has the same amount, add
                    else if(eventCues == maxCues)
                    {
                        eventFound.Add(geez.Value);
                    }
                }

                //if maxCues changed, we found an event
                //MAYBE INSTEAD OF GETTING THE MAX CUES, WE TRY TO GET EXACT CUES, SO WE DO NOT GET A RANDOM EVENT EVERYTIME, EVEN WHEN IT IS SOMETHING NOT KNOWN
                //IDEA: instead of just checking if it is above 0, it has to have, at least, 50% of the cues found
                //if (maxCues >= (cues.Count/2))
                if (maxCues > 0)
                {
                    GeneralEvent theChosenOne = eventFound[0];
                    //from the events found, we try to choose the one more aligned with the topic
                    if(topicSent.Count > 0)
                    {
                        foreach(GeneralEvent cow in eventFound)
                        {
                            foreach (MemoryClass mem in cow.nodes)
                            {
                                if (topicSent.Contains(mem.information))
                                {
                                    theChosenOne = cow;
                                    break;
                                }
                            }
                        }
                    }

                    //add the nodes back to the STM
                    foreach (MemoryClass mem in theChosenOne.nodes)
                    {
                        AddToSTM(mem.informationType, mem.information, mem.weight);
                    }

                    DealWithIt(theChosenOne, newCues);
                }//else, nothing was found
                else
                {
                    //else, see if we have some new term to learn
                    /*string unk = CheckNewTerm(eventFound, newCues);
                    if (unk != "" && unk != ". ")
                    {
                        SpeakYouFool(unk);
                    }//else, dunno
                    else
                    {*/
                        SpeakYouFool("Sorry, i do not know.");
                    //}
                }

                isRetrievingMemory = false;
            }
        }
    }

    //Web Service for Face Recognition
    private IEnumerator RecognitionWebService()
    {
        if (!File.Exists("camImage.png"))
        {
            //save image
            Texture txtr = cam.GetComponent<ViewCam>().GetComponent<MeshRenderer>().materials[0].mainTexture;
            Texture2D image = new Texture2D(txtr.width, txtr.height, TextureFormat.RGB24, false);

            RenderTexture rt = new RenderTexture(txtr.width, txtr.height, 0);
            RenderTexture.active = rt;
            // Copy your texture ref to the render texture
            Graphics.Blit(txtr, rt);

            Destroy(rt);

            image.ReadPixels(new Rect(0, 0, txtr.width, txtr.height), 0, 0);
            image.Apply();

            byte[] _bytes = image.EncodeToPNG();
            //Debug.Log(_bytes);
            FileStream newImage = File.Create("camImage.png");
            newImage.Close();
            File.WriteAllBytes("camImage.png", _bytes);

            Destroy(image);
        }

        UnityWebRequest www = new UnityWebRequest(webServicePath + "recognize", "POST");

        //convert image to string
        byte[] imageData = File.ReadAllBytes("camImage.png");
        string b64 = System.Convert.ToBase64String(imageData);

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes("{\"image\" : [\"" + b64 + "\"], \"direc\" : [\"Data\"], \"th\" : [0.5], \"mode\" : [\"n\"]}");
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        www.SetRequestHeader("Content-Type", "application/json");

        using (www)
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                //UnityEngine.Debug.Log(www.error);
                //if error, try again
                StartCoroutine(RecognitionWebService());
            }
            else
            {
                //UnityEngine.Debug.Log("Received: " + www.downloadHandler.text);
                WriteFaceResult(www.downloadHandler.text);
            }
        }
    }

    //Web Service for save a new person
    private IEnumerator SavePersonWebService()
    {
        UnityWebRequest www = new UnityWebRequest(webServicePath + "savePerson", "POST");

        //convert image to string
        byte[] imageData = File.ReadAllBytes("camImage.png");
        string b64 = System.Convert.ToBase64String(imageData);

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes("{\"image\" : [\"" + b64 + "\"], \"direc\" : [\"Data\"], \"name\" : [\"" + personName + "\"]}");
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        www.SetRequestHeader("Content-Type", "application/json");

        using (www)
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                UnityEngine.Debug.Log(www.error);
            }
            else
            {
                UnityEngine.Debug.Log("Received: " + www.downloadHandler.text);
            }
        }

        //try to find again
        StartCoroutine(RecognitionWebService());
    }

    //Web Service for create node in memory
    /*private IEnumerator CreateMemoryNodeWebService(string node, string typeNode = "", string label = "", float weight = 0.1f)
    {
        string jason = "";
        UnityWebRequest www = new UnityWebRequest(webServicePath + "neo4jTransaction", "POST");
        
        jason = "{\"typeTransaction\" : [\"createNode\"], \"node\" : [\"" + node + "\"], \"typeNode\" : [\"" + typeNode + "\"], \"label\" : [\"" + label + "\"]}";
        
        //UnityEngine.Debug.Log(jason);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jason);
        //byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes("\"typeTransaction\" : [\"createNode\"]");
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        www.SetRequestHeader("Content-Type", "application/json");

        using (www)
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                UnityEngine.Debug.Log(www.error);
            }
            else
            {
                //UnityEngine.Debug.Log("Received 0: " + www.downloadHandler.text);
                string[] aux = www.downloadHandler.text.Split(':');
                //UnityEngine.Debug.Log("Received 0.1: " + aux[2]);
                aux[0] = aux[2].Replace("}}", "");
                aux[0] = aux[0].Replace("\"", "");
                //UnityEngine.Debug.Log("Received 1: " + aux[0]);
                int idReturned = Int32.Parse(aux[0]);
                UnityEngine.Debug.Log("Received 2: " + idReturned);

                //add in STM
                string infoType = "";

                AddToSTM(infoType, node, weight, idReturned);

                //add this on temp and dialogs stuff
                if(typeNode == "Topic" || typeNode == "Dialog")
                {
                    tempDialogs.Add(idReturned, node);
                }
                else
                {
                    tempNodes.Add(idReturned, node);
                }
            }
        }
    }*/

    //Web Service for update node in memory
    /*private IEnumerator UpdateMemoryNodeWebService(string node, string nodeKey = "", string nodeValue = "")
    {
        UnityWebRequest www = new UnityWebRequest(webServicePath + "neo4jTransaction", "POST");
        string jason = "{\"typeTransaction\" : [\"updateNode\"], \"node\" : [\"" + node + "\"], \"nodeKey\" : [\"" + nodeKey + "\"], \"nodeValue\" : [\"" + nodeValue + "\"]}";
        //UnityEngine.Debug.Log(jason);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jason);
        //byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes("\"typeTransaction\" : [\"createNode\"]");
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        www.SetRequestHeader("Content-Type", "application/json");

        using (www)
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                UnityEngine.Debug.Log(www.error);
            }
            else
            {
                UnityEngine.Debug.Log("Received: " + www.downloadHandler.text);
            }
        }
    }*/

    //Web Service for delete node in memory
    /*private IEnumerator DeleteMemoryNodeWebService(string node)
    {
        UnityWebRequest www = new UnityWebRequest(webServicePath + "neo4jTransaction", "POST");
        string jason = "{\"typeTransaction\" : [\"deleteNode\"], \"node\" : [\"" + node + "\"]}";
        //UnityEngine.Debug.Log(jason);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jason);
        //byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes("\"typeTransaction\" : [\"createNode\"]");
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        www.SetRequestHeader("Content-Type", "application/json");

        using (www)
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                UnityEngine.Debug.Log(www.error);
            }
            else
            {
                UnityEngine.Debug.Log("Received: " + www.downloadHandler.text);
            }
        }
    }*/

    /*private IEnumerator CreateRelatioshipNodesWebService(int node, int node2, string relationship = "")
    {
        UnityWebRequest www = new UnityWebRequest(webServicePath + "neo4jTransaction", "POST");
        string jason = "{\"typeTransaction\" : [\"addRelationship\"], \"node\" : [" + node + "], \"node2\" : [" + node2 + "], \"relationship\" : [\"" + relationship + "\"]}";
        //UnityEngine.Debug.Log(jason);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jason);
        //byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes("\"typeTransaction\" : [\"createNode\"]");
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        www.SetRequestHeader("Content-Type", "application/json");

        using (www)
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                UnityEngine.Debug.Log(www.error);
            }
            else
            {
                UnityEngine.Debug.Log("Relationship: " + www.downloadHandler.text);
            }
        }
    }*/

    //Web Service for match (select)
    /*private IEnumerator MatchWebService(string match, bool addOnLTM = false, Dictionary<string,string> tokens = null)
    {
        UnityWebRequest www = new UnityWebRequest(webServicePath + "neo4jTransaction", "POST");
        string jason = "{\"typeTransaction\" : [\"matchNode\"], \"match\" : [\"" + match + "\"]}";
        //UnityEngine.Debug.Log(jason);
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jason);
        //byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes("\"typeTransaction\" : [\"createNode\"]");
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        www.SetRequestHeader("Content-Type", "application/json");

        using (www)
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                UnityEngine.Debug.Log(www.error);
            }
            else
            {
                //UnityEngine.Debug.Log("Match: " + www.downloadHandler.text);

                //loading all from database
                if (addOnLTM)
                {
                    //MatchesToLTM(www.downloadHandler.text);
                }//else, just normal match
                else
                {
                    MatchesFromRetrieval(www.downloadHandler.text, tokens);
                }
            }
        }
    }*/

    //get all matches and place in the LTM
    /*private void MatchesToLTM(string results)
    {
        string temp;
        temp = results.Replace("}}}", "?");
        string[] temp2 = temp.Split('?');

        foreach (string part in temp2)
        {
            //if has no comma, doesnt matter
            if (!part.Contains(","))
            {
                continue;
            }

            string part2 = part.Replace("\"", "");
            part2 = part2.Replace("\\n\\:", "?");
            string[] ohFuck = part2.Split('?');
            string[] ohFuck2 = ohFuck[1].Split(',');

            //create memory node
            MemoryClass newMem = new MemoryClass();

            foreach (string really in ohFuck2)
            {
                string aff = really.Replace("{", "");
                aff = aff.Replace("}", "");
                string[] itens = aff.Split(':');
                //UnityEngine.Debug.Log("Match 1: " + itens[0]);
                //UnityEngine.Debug.Log("Match 2: " + itens[1]);
                itens[0] = itens[0].Substring(1, itens[0].Length - 2);
                //itens[1] = itens[1].Substring(1, itens[1].Length - 2);
                //UnityEngine.Debug.Log("Match 2: " + itens[0]);

                switch (itens[0])
                {
                    case "name":
                        newMem.information = itens[1].Substring(1, itens[1].Length - 2);

                        break;
                    case "activation":
                        newMem.activation = float.Parse(itens[1]);
                        break;
                    case "weight":
                        newMem.weight = float.Parse(itens[1]);
                        break;
                    case "type":
                        itens[1] = itens[1].Substring(1, itens[1].Length - 2);
                        if (itens[1] == "text")
                            newMem.informationType = 0;
                        else if (itens[1] == "image")
                            newMem.informationType = 1;
                        break;
                    default:
                        if (itens[1][0] == '\\')
                        {
                            itens[1] = itens[1].Substring(1, itens[1].Length - 2);
                        }
                        newMem.properties.Add(itens[0], itens[1]);
                        break;
                }
            }
            
            agentLongTermMemory.Add(newMem);
        }
    }*/

    //matches from generative retrieval
    /*private void MatchesFromRetrieval(string results, Dictionary<string, string> tokens)
    {
        string temp;
        //temp = results.Replace("}}}", "?");
        temp = results.Replace("\"", "");
        temp = temp.Replace("\\a\\:", "?");
        temp = temp.Replace("\\b\\:", "?");
        temp = temp.Replace("\\c\\:", "?");
        temp = temp.Replace("\\d\\:", "?");
        temp = temp.Replace("\\n\\:", "?");
        string[] temp2 = temp.Split('?');
        //UnityEngine.Debug.LogWarning(temp2);

        Dictionary<string,string> found = new Dictionary<string, string>();
        foreach (string part in temp2)
        {
            //if has no comma, doesnt matter
            if (!part.Contains(","))
            {
                continue;
            }

            /*string part2 = part.Replace("\"", "");
            part2 = part2.Replace("\\n\\:", "?");
            part2 = part2.Replace("\\a\\:", "?");
            part2 = part2.Replace("\\b\\:", "?");
            part2 = part2.Replace("\\c\\:", "?");
            part2 = part2.Replace("\\d\\:", "?");
            part2 = part2.Replace("\\e\\:", "?");
            string[] ohFuck = part2.Split('?');*
            string[] ohFuck2 = part.Split(',');

            string node = "";
            string info = "";

            foreach (string really in ohFuck2)
            {
                string aff = really.Replace("{", "");
                aff = aff.Replace("}", "");
                string[] itens = aff.Split(':');
                //UnityEngine.Debug.Log("Match 1: " + itens[0]);
                //UnityEngine.Debug.Log("Match 2: " + itens[1]);
                if (itens[0] == "") continue;
                itens[0] = itens[0].Substring(1, itens[0].Length - 2);
                //itens[1] = itens[1].Substring(1, itens[1].Length - 2);
                //UnityEngine.Debug.Log("Match 2: " + itens[0]);

                switch (itens[0])
                {
                    case "name":
                        node = itens[1].Substring(1, itens[1].Length - 2);
                        break;
                    case "activation":
                        if(info == "")
                        {
                            info = "activation:"+ float.Parse(itens[1]);
                        }
                        else
                        {
                            info += ",activation:" + float.Parse(itens[1]);
                        }
                        break;
                    case "weight":
                        if (info == "")
                        {
                            info = "weight:" + float.Parse(itens[1]);
                        }
                        else
                        {
                            info += ",weight:" + float.Parse(itens[1]);
                        }
                        break;
                    case "type":
                        if (info == "")
                        {
                            info = "type:" + itens[1].Substring(1, itens[1].Length - 2);
                        }
                        else
                        {
                            info += ",type:" + itens[1].Substring(1, itens[1].Length - 2);
                        }
                        break;
                    default:
                        if (itens[1][0] == '\\')
                        {
                            itens[1] = itens[1].Substring(1, itens[1].Length - 2);
                        }

                        if (info == "")
                        {
                            info = itens[0] + ":" + itens[1];
                        }
                        else
                        {
                            info += "," + itens[0] + ":" + itens[1];
                        }
                        break;
                }
            }

            //create a list with the stuff
            if (!found.ContainsKey(node))
            {
                found.Add(node, info);
            }
        }

        //now we deal with what was remembered
        string unknoun = "";

        //check if the found event has the Noun or proper noun on it
        //if it does not, it means Arthur is not yet familiar with such term
        //so, we ask the user if he wants to give more details.
        List<string> nouns = new List<string>();
        foreach (KeyValuePair<string, string> tt in tokens)
        {
            if (tt.Value == "NN" || tt.Value == "NNP")
            {
                nouns.Add(tt.Key);
            }
        }

        //if nothing, need to learn
        if (found.Count == 0)
        {
            unknoun = nouns[nouns.Count - 1];
        }

        /*if (nouns.Count > 0)
        {
            foreach (string nn in nouns)
            {
                int recor = 0;

                if (found.Count > 0)
                {
                    if (found.ContainsKey(nn))
                    {
                        recor++;
                    }
                }

                //if recor is still 0, it means we found no occurrences of this noun in the event.
                //DEACTIVATE FOR NOW, MAYBE THIS IS NOT THE BEST PLACE WITHOUT GENERAL EVENTS
                if (recor == 0)
                {
                    unknoun = nn;
                    break;
                }
            }
        }*

        if (found.Count > 0)
        {
            //do not save this memory, since it already exists somehow
            saveNewMemoryNode = false;

            //if it has an "unknoun", deal with it
            if (unknoun != "")
            {
                DealUnknown(unknoun);
            }
            else
            {
                //UnityEngine.Debug.Log(foundIt.information);
                //do something with the retrieved memory
                DealWithIt(found, tokens);

                //change the face of Arthur according emotion of the event
                //NEED TO SEE IT, NOT TREATING GENERAL EVENTS YET
                //SetEmotion(foundIt.emotion);
            }
        }
        else
        {
            if (unknoun != "")
            {
                DealUnknown(unknoun);

                //do not save this memory
                saveNewMemoryNode = false;
            }
            else
            {

                //request for chat.
                string txt = "";
                foreach (KeyValuePair<string, string> tt in tokens)
                {
                    txt += tt.Key + " ";
                }
                StartCoroutine(GetRequest("https://acobot-brainshop-ai-v1.p.rapidapi.com/get?bid=178&key=sX5A2PcYZbsN5EY6&uid=mashape&msg=" + txt));
            }
        }
    }*/

    private void WriteTokens(string webServiceResponse)
    {
        //need to format it properly now
        string info = webServiceResponse.Replace("\"", "");
        info = info.Replace(@"\", "");
        info = info.Replace("},", "@");
        string[] infoSplit = info.Split('@');

        infoSplit[0] = infoSplit[0].Replace("{0:{", "");
        infoSplit[1] = infoSplit[1].Replace("1:{", "");
        infoSplit[1] = infoSplit[1].Replace("}}", "");

        string[] tokens = infoSplit[0].Split(',');
        string[] tknType = infoSplit[1].Split(',');

        for (int i = 0; i < tokens.Length; i++)
        {
            tokens[i] = tokens[i].Split(':')[1];
        }
        for (int i = 0; i < tknType.Length; i++)
        {
            tknType[i] = tknType[i].Split(':')[1];
        }
        //end formatting

        //UnityEngine.Debug.Log(tokens[0]);
        //UnityEngine.Debug.Log(tknType[0]);

        //write the file
        StreamWriter sr = File.CreateText("resultToken.txt");

        for (int i = 0; i < tokens.Length; i++)
        {
            //if it is the last, it is the polarity
            if (i == tokens.Length - 1)
            {
                sr.WriteLine(tokens[i]);
                //UnityEngine.Debug.Log(tokens[i]);
            }
            else
            {
                sr.WriteLine(tokens[i] + ";" + tknType[i]);
                //UnityEngine.Debug.Log(tknType[i]);
            }
        }

        sr.Close();
    }

    private void WriteFaceResult(string webServiceResponse)
    {
        //need to format it properly now
        string info = webServiceResponse.Replace("\"", "");
        info = info.Replace(@"\", "");
        info = info.Replace("}}", "");
        info = info.Replace("{0:{0:", "");

        string[] infoSplit = info.Split(',');
        //end formatting

        //UnityEngine.Debug.Log(tokens[0]);
        //UnityEngine.Debug.Log(tknType[0]);

        //write the file
        StreamWriter sr = File.CreateText("result.txt");
        sr.WriteLine(infoSplit[0].Split(':')[0]);
        sr.Close();
    }
}