using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UI;


public class MainController : MonoBehaviour
{
    public GameObject faceName;
    //qnt of frames to consider when choosing the faceName (default: 3)
    public int framesToConsider;

    private GameObject[] eyes;
    //array with the last framesToConsider frames read
    //public List<string> foundNames;
    //array with the last framesToConsider emotions found
    public List<string> foundEmotions;
    public string personName;

    //input text to chat and the chat itself
    public GameObject inputText;
    public Text chatText;

    public string foundEmotion;

    //mario emtion
    public string marioEmotion;

    //agent memory
    //following George Miller definition, each person is able to keep 7 pieces of information in memory at each time, varying more or less 2
    public List<MemoryClass> agentShortTermMemory;
    private TimeSpan memorySpan;
    //private TimeSpan rehearseMemorySpan;
    //long term memory, with the node information
    public List<MemoryClass> agentLongTermMemory;
    //general events
    //public List<GeneralEvent> agentGeneralEvents;

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

    //vars to communicate with python
    /*private ProcessStartInfo psi;
    private string script;
    private List<string> args;
    private string errors;
    private string results;
    private Process process;*/

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
    //private SmallTalkClass smallTalk;
    //is small talking?
    /*public bool isSmallTalking;
    //id of the actual small talk
    public int usingSmallTalk;
    //id of the actual root smallTalk, if it is going down the tree
    public int rootSmallTalk;*/
    private List<TopicGraph> topics;
    private TopicGraph currentTopic;
    //list with all dialogs/topics in memory
    private List<string> dialogsInMemory;

    //has the face recognition already returned?
    //private bool isFaceReco;

    //next ID for memory
    private int nextId;

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

    //temp memories to keep for general events later
    private int qntTempNodes = 0;
    private Dictionary<int,string> tempNodes;
    private string tempTypeEvent;
    private string tempRelationship;
    private int arthurIdDatabase = 0;
    private bool arthurLearnsSomething = false;

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
        tempNodes = new Dictionary<int, string>();

        eyes = GameObject.FindGameObjectsWithTag("Eye");

        //set the ice breakers
        rootIceBreaker = usingIceBreaker = -1;
        //first element is just the pointer to the root questions
        iceBreakers = new IceBreakingTreeClass(0, "root", "", false);
        usingIceBreaker = 0;

        //load icebreakers and answers from the file
        LoadIceBreakersAndStuff();

        //set the small talks
        /*usingSmallTalk = rootSmallTalk = -1;
        //first element is just the pointer to the root questions
        //smallTalk = new SmallTalkClass(0, "", false);
        usingSmallTalk = 0;*/
        topics = new List<TopicGraph>();
        dialogsInMemory = new List<string>();

        LoadSmallTalk();
        PickTopic();

        //load small talks from the memory
        LoadMemoryDialogs();

        //load the small talks from the file
        //LoadSmallTalk();

        //hide zzz
        zzz.SetActive(false);
        timerObject.SetActive(false);
        randomImage.SetActive(false);
        riTarget.SetActive(false);

        //foundNames = new List<string>();
        foundEmotions = new List<string>();
        peopleGreeted = new List<string>();
        agentShortTermMemory = new List<MemoryClass>();
        agentLongTermMemory = new List<MemoryClass>();
        //agentGeneralEvents = new List<GeneralEvent>();
        memorySpan = new TimeSpan(0, 0, 15);
        //rehearseMemorySpan = new TimeSpan(0, 0, 2);

        //emotion stuff from will
        /*List<string[]> aux = EmotionScript(); //bad slow guy!!
        //string[] actionScript = aux.ToArray()[0];
        emotions = aux.ToArray()[1];*/

        //what we have on textLTM, load into auxiliary LTM
        /*StreamReader readingLTM = new StreamReader("AutobiographicalStorage/textLTM.txt", System.Text.Encoding.Default);
        using (readingLTM)
        {
            string line;
            do
            {
                line = readingLTM.ReadLine();

                if (line != "" && line != null)
                {
                    //memory time;person;emotion
                    string[] info = line.Split(';');

                    MemoryClass newMem = new MemoryClass(System.DateTime.Now, System.Convert.ToInt32(info[3]), info[2],
                        System.Convert.ToInt32(info[1]), System.Convert.ToSingle(info[5]));
                    newMem.activation = System.Convert.ToSingle(info[4]);

                    //LTM - everything
                    agentLongTermMemory.Add(newMem);
                }
            } while (line != null);
        }
        readingLTM.Close();*/
        //load from the database
        string match = "match(n) return n";
        StartCoroutine(MatchWebService(match, true));

        //we also load the general events
        /*StreamReader readingLTM = new StreamReader("AutobiographicalStorage/generalEvents.txt", System.Text.Encoding.Default);
        using (readingLTM)
        {
            string line;
            do
            {
                line = readingLTM.ReadLine();

                if (line != "" && line != null)
                {
                    //memory time;person;emotion
                    string[] info = line.Split(';');

                    GeneralEvent newGe = new GeneralEvent(System.DateTime.Parse(info[0]), info[3], info[2],
                        System.Convert.ToInt32(info[1]), info[4]);

                    //polarity
                    newGe.polarity = float.Parse(info[5]);

                    //if has nodes...
                    if (info.Length > 6)
                    {
                        for (int i = 6; i < info.Length; i++)
                        {
                            int newId = System.Convert.ToInt32(info[i]);
                            foreach (MemoryClass mc in agentLongTermMemory)
                            {
                                if (mc.informationID == newId)
                                {
                                    newGe.nodes.Add(mc);
                                    break;
                                }
                            }
                        }
                    }

                    agentGeneralEvents.Add(newGe);
                }
            } while (line != null);
        }
        readingLTM.Close();*/

        //read the next ID from the file
        StreamReader sr = new StreamReader("nextId.txt", System.Text.Encoding.Default);
        string textFile = sr.ReadToEnd();
        sr.Close();
        nextId = System.Convert.ToInt32(textFile.Trim());
    }

    //method to execute the python batch
    /*static void ExecuteCommand(string command, string args)
    {
        var processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
        processInfo.CreateNoWindow = true;
        processInfo.UseShellExecute = false;
        processInfo.RedirectStandardError = true;
        processInfo.RedirectStandardOutput = true;

        var process = Process.Start(processInfo);

        /*process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            UnityEngine.Debug.LogError("D>" + e.Data);
        process.BeginOutputReadLine();

        process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            UnityEngine.Debug.LogError("E>" + e.Data);
        process.BeginErrorReadLine();

        process.WaitForExit();

        //Console.WriteLine("ExitCode: {0}", process.ExitCode);
        process.Close();*
    }*/

    // Start is called before the first frame update
    void Start()
    {
        /*SmallTalkClass teste = smallTalk.FindSmallTalk(6);
        try
        {
            UnityEngine.Debug.Log(teste.Getsentence());
        }
        catch
        {
            UnityEngine.Debug.Log("Erroouuuu");
        }
        UnityEngine.Debug.Break();*/

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

        //start the idle timer with the seconds now
        idleTimer = Time.time;

        //just testing...
        //StartCoroutine(CreateMemoryNodeWebService("Demonio", "Demon", "age:31,ocupation:'teacher'"));
        /*Dictionary<string,string> test = new Dictionary<string, string>();
        test.Add("Knob", "NNP");
        test.Add("cellphone", "NN");
        test.Add("potato", "NN");
        test.Add("knows", "VB");
        GenerativeRetrieval(test);*/
    }

    private void OnDestroy()
    {
        //save LTM as it is
        //SaveLTM();

        //save general events
        //SaveGeneralEvents();

        //save next ID
        StreamWriter textToToken = new StreamWriter("nextId.txt");
        textToToken.WriteLine(nextId);
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
            //for each eye, generate a saccade behavior
            /*float saccadeMag = CalculateSaccade();
            float direction = UnityEngine.Random.Range(0f, 100f);

            foreach (GameObject eye in eyes)
            {
                eye.GetComponent<EyeController>().SaccadeBehavior(saccadeMag, direction, true);
            }*/

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

                //change emotion for empathy
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
                    //if not using memory, just send it to the chatbot and whatever...
                    if (!isUsingMemory)
                    {
                        //request for chat.
                        string txt = "";
                        foreach (KeyValuePair<string, string> tt in tokens)
                        {
                            txt += tt.Key + " ";
                        }
                        StartCoroutine(GetRequest("https://acobot-brainshop-ai-v1.p.rapidapi.com/get?bid=178&key=sX5A2PcYZbsN5EY6&uid=mashape&msg=" + txt));
                    }
                    else
                    {

                        //if the user is answering a yes/no question, we dont need to save or retrieve memory
                        //IT SHOULD NEVER ENTER HERE, BECAUSE WE TREAT IT IN THE SENDREQUEST. BUT, JUST TO BE SURE...
                        if (isYesNoQuestion)
                        {
                            DealYesNo();
                        }//else, if it is breaking the ice or small talking, dont need to try to recover memory neither. Just save the information later and try to keep going                 
                        else if (!isBreakingIce && !currentTopic.IsDialoging())
                        {
                            //this is tricky :D
                            saveNewMemoryNode = false;
                            GenerativeRetrieval(tokens);
                        }
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
                        //HERE!!! MAYBE THIS SAVENEWMEMORYNODE IS NOT UPDATED YET, BECAUSE OF THE MATCH
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

            //if we have temp nodes, need to create general event for it
            if(tempNodes.Count > 0 && tempNodes.Count == qntTempNodes)
            {
                //create a new general event
                string infoEvent = "interaction";
                if(tempTypeEvent == "meet new person")
                {
                    infoEvent = "i met " + personName;
                }else if (tempTypeEvent == "learn thing")
                {
                    infoEvent = "i learned something";
                }

                //if we have Arthur or personName, we make it be the first node
                if (tempNodes.ContainsValue("Arthur") || tempNodes.ContainsValue(personName))
                {
                    Dictionary<int, string> temp = new Dictionary<int, string>();

                    foreach (KeyValuePair<int, string> cn in tempNodes)
                    {
                        if(cn.Value == "Arthur" || cn.Value == personName)
                        {
                            temp.Add(cn.Key, cn.Value);
                            break;
                        }
                    }
                    foreach (KeyValuePair<int, string> cn in tempNodes)
                    {
                        if (cn.Value != "Arthur" && cn.Value != personName)
                        {
                            temp.Add(cn.Key, cn.Value);
                            break;
                        }
                    }

                    tempNodes = temp;
                }

                //connect nodes for event and create relationship on the database
                List<int> connectNodes = new List<int>();
                //just the text ids
                List<int> textInfo = new List<int>();
                List<int> twoByTwo = new List<int>();

                foreach (KeyValuePair<int, string> cn in tempNodes)
                {
                    connectNodes.Add(cn.Key);
                    twoByTwo.Add(cn.Key);

                    if (!cn.Value.Contains("myself") && !cn.Value.Contains("thing"))
                    {
                        textInfo.Add(cn.Key);
                    }

                    if (twoByTwo.Count == 2)
                    {
                        StartCoroutine(CreateRelatioshipNodesWebService(twoByTwo[0], twoByTwo[1], tempRelationship));

                        //if it is children, the pairing is a bit different, since they all connect with the person
                        if (tempRelationship == "HAS_CHILD")
                        {
                            twoByTwo.RemoveAt(1);
                        }
                        else
                        {
                            twoByTwo.RemoveAt(0);
                        }
                    }
                }

                //if he is learning something, we create relationship between the text and Arthur
                if (arthurLearnsSomething)
                {
                    foreach(int ite in textInfo)
                    {
                        StartCoroutine(CreateRelatioshipNodesWebService(arthurIdDatabase, ite, "KNOWS"));
                    }
                }//else, if Arthur is meeting someone new, needs to create this relationship
                else if(tempTypeEvent == "meet new person")
                {
                    foreach (int ite in textInfo)
                    {
                        StartCoroutine(CreateRelatioshipNodesWebService(arthurIdDatabase, ite, "MET"));
                    }
                }

                //AddGeneralEvent(tempTypeEvent, infoEvent, connectNodes);

                connectNodes.Clear();

                //reset it
                tempTypeEvent = tempRelationship = "";
                qntTempNodes = -1;
                tempNodes.Clear();
                arthurLearnsSomething = false;
            }

            //emotion based on last polarity answer
            if (lastPolarity < 0) SetEmotion("sadness");
            else if (lastPolarity > 0) SetEmotion("joy");
            else SetEmotion("neutral");
        }
    }

    //Agent says something
    private void SpeakYouFool(string weirdThingToTalk)
    {
        chatText.text = weirdThingToTalk;

        //just speak if canSpeak is true
        if (canSpeak)
        {
            //also, speak it
            sc.GetComponent<SpeakerController>().SpeakSomething(weirdThingToTalk);
        }
    }

    //deals with unknown information
    private void DealUnknown(string unknoun)
    {
        //asks the user if it wants to give more details about this subject
        string responseText = "I see. I do not know " + unknoun + ", would you like to show me a picture?";
        SpeakYouFool(responseText);

        //yes/no question
        isYesNoQuestion = true;

        //keep it, so we know later what are we refering of
        importantNoun = unknoun;
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
    /*private void DealWithIt(GeneralEvent retrieved, Dictionary<string, string> tokens)
    {
        string responseText = "";

        //depending the type of the event, do something different
        switch (retrieved.eventType)
        {
            case "recognizes person":
            case "meet new person":
                //get the person name using the memory nodes
                string personName = "";
                foreach (MemoryClass nd in retrieved.nodes)
                {
                    //if it is text
                    if (nd.informationType == 0)
                    {
                        personName = nd.information;
                        break;
                    }
                }

                //if the answer is about Arthur himself, lets make it more personal =)
                if (personName == "Arthur")
                {
                    responseText = "Of course i know myself! Duh!!";
                }
                else
                {
                    responseText = "Yes, i already know " + personName;

                    //depending the emotion found for the event
                    responseText += ". It seemed " + EmotionMemoryPicker(retrieved.emotion) + " when we first met!";
                }

                //talk motherfucker!
                SpeakYouFool(responseText);

                break;
            case "learn thing":
                //get the noun
                string nummy = "";
                foreach (KeyValuePair<string, string> tks in tokens)
                {
                    if (tks.Value == "NN" || tks.Value == "NNP")
                    {
                        nummy = tks.Key;
                        break;
                    }
                }

                //if the agent is recovering a learning memory, it can be of something random (like an object) or something about someone.
                //if it is about someone, answer the question
                //otherwise, tell and show
                //how do we know? if nummy has a MET general event, it is a person
                bool met = false;
                foreach (GeneralEvent ge in agentGeneralEvents)
                {
                    if (ge.eventType == "meet new person")
                    {
                        foreach (MemoryClass mk in ge.nodes)
                        {
                            if (mk.information == nummy)
                            {
                                met = true;
                                break;
                            }
                        }
                    }

                    if (met) break;
                }

                if (met)
                {
                    //if it is something about Arthur, lets make it more personal
                    if (tokens.ContainsKey("Arthur"))
                    {
                        if (tokens.ContainsKey("old"))
                        {
                            responseText = "I am ";

                            foreach (MemoryClass mk in retrieved.nodes)
                            {
                                if (!tokens.ContainsKey(mk.information))
                                {
                                    responseText += mk.information + " ";
                                }
                            }

                            responseText += "year old!";
                        }else if (tokens.ContainsKey("study"))
                        {
                            responseText = "No, i do not study";
                        }
                        else if (tokens.ContainsKey("work"))
                        {
                            responseText = "No, i do not work";
                        }
                        else if (tokens.ContainsKey("children"))
                        {
                            responseText = "I have no children";
                        }
                    }
                    else
                    {
                        //get all information about this person
                        foreach (MemoryClass mk in retrieved.nodes)
                        {
                            if (!tokens.ContainsKey(mk.information))
                            {
                                responseText += mk.information + " ";
                            }
                        }
                    }
                    
                    responseText = responseText.Trim();
                }
                else
                {
                    responseText = "Yeah, i know a " + nummy + "!";

                    //get the image
                    string imagePath = "";
                    foreach (MemoryClass mc in retrieved.nodes)
                    {
                        if (mc.informationType == 1)
                        {
                            imagePath = mc.information;
                            break;
                        }
                    }

                    //if has image, show it to the user
                    if (imagePath != "")
                    {
                        //say it also
                        responseText += " Here is one!";

                        var bytes = System.IO.File.ReadAllBytes(imagePath);
                        var tex = new Texture2D(1, 1);
                        tex.LoadImage(bytes);
                        randomImage.GetComponent<MeshRenderer>().material.mainTexture = tex;

                        //show
                        randomImage.SetActive(true);
                        riTarget.SetActive(true);
                    }
                }

                SpeakYouFool(responseText);

                break;
            case "interaction":
                responseText = "Men working, come back later...";

                SpeakYouFool(responseText);

                break;
            default:
                UnityEngine.Debug.LogWarning("General Type not found!");
                break;
        }
    }*/

    //deal version without event
    private void DealWithIt(Dictionary<string, string> retrieved, Dictionary<string, string> tokens)
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
    }

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
        //depending on the ice breaker, we just add info in the person
        if (tokens.ContainsKey("old"))
        {
            foreach (KeyValuePair<string, string> txt in tokens)
            {
                if(txt.Key != personName && txt.Key != "old")
                {
                    StartCoroutine(UpdateMemoryNodeWebService(personName, "age", txt.Key));
                }
            }   
        }else if (tokens.ContainsKey("study"))
        {
            foreach (KeyValuePair<string, string> txt in tokens)
            {
                if (txt.Key != personName && txt.Key != "study")
                {
                    StartCoroutine(UpdateMemoryNodeWebService(personName, "study", txt.Key));
                }
            }
        }
        else if (tokens.ContainsKey("work"))
        {
            foreach (KeyValuePair<string, string> txt in tokens)
            {
                if (txt.Key != personName && txt.Key != "work")
                {
                    StartCoroutine(UpdateMemoryNodeWebService(personName, "work", txt.Key));
                }
            }
        }
        else if (tokens.ContainsKey("children"))
        {
            foreach (KeyValuePair<string, string> txt in tokens)
            {
                if (txt.Key != personName && txt.Key != "children")
                {
                    StartCoroutine(UpdateMemoryNodeWebService(personName, "children", txt.Key));

                    //if answer is no, add the NoChildren node
                    if (lastPolarity < 0)
                    {
                        //"create" the person as well, just to get id back
                        StartCoroutine(CreateMemoryNodeWebService(personName, "Person", "", 0.9f));

                        string label = "name:'NoChildren',activation:1,weight:0.9,nodeType:'text',lastEmotion:'" + foundEmotion + "'";
                        StartCoroutine(CreateMemoryNodeWebService("NoChildren", "Person", label, 0.9f));

                        //type of the event, to save later
                        tempTypeEvent = "learn thing";
                        tempRelationship = "HAS_CHILD";
                        qntTempNodes = 2;
                    }
                }
            }
        }
        else if (tokens.ContainsKey("study course"))
        {
            //"create" the person as well, just to get id back
            StartCoroutine(CreateMemoryNodeWebService(personName, "Person", "", 0.9f));

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

            //type of the event, to save later
            tempTypeEvent = "learn thing";
            tempRelationship = "IS_STUDYING";
            qntTempNodes = 2;

            //create this node
            string label = "name:'" + course + "',activation:1,weight:0.9,nodeType:'text',lastEmotion:'" + foundEmotion + "'";
            StartCoroutine(CreateMemoryNodeWebService(course, "Course", label, 0.9f));

            //create the relationship between the person and the course
            //StartCoroutine(CreateRelatioshipNodesWebService(personName, course, "IS_STUDYING"));
        }
        else if (tokens.ContainsKey("work job"))
        {
            //"create" the person as well, just to get id back
            StartCoroutine(CreateMemoryNodeWebService(personName, "Person", "", 0.9f));

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

            //type of the event, to save later
            tempTypeEvent = "learn thing";
            tempRelationship = "IS_WORKING";
            qntTempNodes = 2;

            //create this node
            string label = "name:'" + job + "',activation:1,weight:0.9,nodeType:'text',lastEmotion:'" + foundEmotion + "'";
            StartCoroutine(CreateMemoryNodeWebService(job, "Job", label, 0.9f));

            //create the relationship between the person and the course
            //StartCoroutine(CreateRelatioshipNodesWebService(personName, job, "IS_WORKING"));
        }
        else if (tokens.ContainsKey("children quantity"))
        {
            foreach (KeyValuePair<string, string> txt in tokens)
            {
                if (txt.Key != personName && txt.Key != "children quantity")
                {
                    StartCoroutine(UpdateMemoryNodeWebService(personName, "qntChildren", txt.Key));
                }
            }
        }
        else if (tokens.ContainsKey("children names"))
        {
            //"create" the person as well, just to get id back
            StartCoroutine(CreateMemoryNodeWebService(personName, "Person", "", 0.9f));

            int qntChild = 0;
            foreach (KeyValuePair<string, string> txt in tokens)
            {
                if (txt.Key != personName && txt.Key != "children names")
                {
                    string label = "name:'" + txt.Key + "',activation:1,weight:0.9,nodeType:'text',lastEmotion:'" + foundEmotion + "'";
                    StartCoroutine(CreateMemoryNodeWebService(txt.Key, "Person", label, 0.9f));
                    qntChild++;

                    //create the relationship between the person and the child
                    //StartCoroutine(CreateRelatioshipNodesWebService(personName, txt.Key, "HAS_CHILD"));
                }
            }

            //type of the event, to save later
            tempTypeEvent = "learn thing";
            tempRelationship = "HAS_CHILD";

            //if qntChild is 0, we create a node with "NoChildren"
            if (qntChild == 0)
            {
                string label = "name:'NoChildren',activation:1,weight:0.9,nodeType:'text',lastEmotion:'" + foundEmotion + "'";
                StartCoroutine(CreateMemoryNodeWebService("NoChildren", "Person", label, 0.9f));
                qntTempNodes = 2;
            }
            else
            {
                qntTempNodes = qntChild + 1;
            }
        }

        //iceBreakers.FindIcebreaker(usingIceBreaker)
        //string label = "name:'" + namePerson + "',activation:1,weight:0.9,nodeType:'text'";
        //StartCoroutine(CreateMemoryNodeWebService(namePerson, "Person", label, 0.9f));
    }

    private void SaveSmallTalk(Dictionary<string, string> tokens)
    {
        //first, we need to save this topic and dialog
        //for create relationship later
        qntTempNodes = 2; //topic and dialog to connect
        //type of the event, to save later
        tempTypeEvent = "";
        tempRelationship = "HAS_DIALOG";

        //save topic
        string label = "name:'" + currentTopic.GetId() + "',activation:1,weight:0.8,nodeType:'text'";
        StartCoroutine(CreateMemoryNodeWebService(currentTopic.GetId(), "Topic", label, 0.8f));

        //save dialog
        label = "name:'Dialog" + currentTopic.GetCurrentDialog().GetId().ToString() + "',activation:1,weight:0.8,nodeType:'text'";//text:'"+ currentTopic.GetCurrentDialog() + "'
        StartCoroutine(CreateMemoryNodeWebService("Dialog"+currentTopic.GetCurrentDialog().GetId().ToString(), "Dialog", label, 0.8f));
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
            //for create general event later
            qntTempNodes = tokens.Count;
            //type of the event, to save later
            tempTypeEvent = "";
            tempRelationship = "HAS_PHOTO";

            //for each information, save it in memory
            foreach (KeyValuePair<string, string> txt in tokens)
            {
                //strip the "'"
                //int thisID = AddToSTM(0, txt.Key, weight);
                //connectNodes.Add(thisID);
                //save on Neo4j
                //on temp, we have to find 2 information later

                //if is verb, relationship
                if(txt.Value == "VB")
                {
                    tempRelationship = txt.Key.ToUpper();
                    continue;
                }
                
                string label = "name:'" + txt.Key + "',activation:1,weight:"+weight+ ",nodeType:'text',lastEmotion:'" + foundEmotion + "'";
                StartCoroutine(CreateMemoryNodeWebService(txt.Key, "Interaction", label, weight));
            }

            //if we have a second level icebreaker, we need to find the general event to add info into it
            //UPDATE: deactivate it, because the answers of the questions get mixed
            /*GeneralEvent fuck = null;
            if (rootIceBreaker != -1)
            {
                foreach (GeneralEvent geez in agentGeneralEvents)
                {
                    //for each memory node which compounds this general event
                    foreach (MemoryClass node in geez.nodes)
                    {
                        //if it exists, ++
                        if (node.information.Contains(iceBreakers.FindIcebreaker(usingIceBreaker).GetType()))
                        {
                            //found it!
                            fuck = geez;
                            break;
                        }
                    }
                }
            }*/

            /*GeneralEvent fuck = null;
            if (fuck == null)
            {
                //create a new general event
                AddGeneralEvent(typeEvent, informationEvent.Trim(), connectNodes);

                //now, we connect the memories
                ConnectMemoryNodes(connectNodes);
            }*/
            //deactivated for now
            /*else
            {
                foreach(MemoryClass nd in fuck.nodes)
                {
                    //see if it already exists
                    if (connectNodes.Contains(nd.informationID))
                    {
                        //take this out of connectnodes, since it already exists
                        connectNodes.Remove(nd.informationID);
                        //break;
                    }
                }

                //now, we add the remaining connectnodes
                if (connectNodes.Count > 0)
                {
                    foreach (MemoryClass mc in agentShortTermMemory)
                    {
                        if (connectNodes.Contains(mc.informationID))
                        {
                            fuck.nodes.Add(mc);
                        }
                    }
                }

                //and if it should be final, add it to know later
                if (iceBreakers.FindIcebreaker(rootIceBreaker).GetType().Contains("final"))
                {
                    fuck.information += " final";
                }
            }*/

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

    //generate an unique ID
    //TODO: THE TIME IS GETTING SAME SECONDS/MILISECONDS... =(
    private int GenerateID()
    {
        //DateTime epochStart = new System.DateTime(2020, 12, 16, 8, 0, 0, System.DateTimeKind.Utc);
        //int timestamp = System.Convert.ToInt32((System.DateTime.UtcNow - epochStart).TotalMilliseconds);
        //UnityEngine.Debug.Log(timestamp);
        //return timestamp;
        //return UnityEngine.Random.Range(0, 100000);
        return nextId++;
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

        //reset the idle timer
        idleTimer = Time.time;

        //replace occurences of "you" for "Arthur"
        textSend = textSend.Replace(" you ", " Arthur ");
        textSend = textSend.Replace(" you?", " Arthur ");
        textSend = textSend.Replace(" yourself?", " Arthur ");

        //replace occurences of "me" for personName
        textSend = textSend.Replace(" me ", " "+ personName +" ");
        textSend = textSend.Replace(" me?", " " + personName + " ");
        textSend = textSend.Replace(" i ", " " + personName + " ");
        textSend = textSend.Replace(" i?", " " + personName + " ");

        if (!isGettingInformation && isKnowingNewPeople)
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
            textToToken.Close();*/

            //UPDATE: now we send a request to our webservice, through a json
            StartCoroutine(TokenizationWebService(textSend));
        }
        //WE CAN HAVE SOME PROBLEM HERE, WHEN SAVING NEW FELLA, DOES NOT NEED TO TOKENIZE IT
        //if it is meeting a new person, we can save it here already


        //we do nothing else here, since the tokenization is going to happen on the requester, and we need the response of it to keep going
    }

    /*public void SendRequestChat()
    {
        string textSend = inputText.GetComponent<InputField>().text;
        inputText.GetComponent<InputField>().text = "";

        //just try to get information if agent is not getting/delivering specific information
        if (!isGettingInformation && !isKnowingNewPeople)
        {
            //save in memory
            //first, tokenizate it and remove stop words
            //change information on file
            StreamWriter textToToken = new StreamWriter("textToToken.txt");
            textToToken.WriteLine(textSend);
            textToToken.Close();
            //UnityEngine.Debug.Break();

            // A correct website page.
            StartCoroutine(GetRequest("https://acobot-brainshop-ai-v1.p.rapidapi.com/get?bid=178&key=sX5A2PcYZbsN5EY6&uid=mashape&msg=" + textSend));
        }else if (!isGettingInformation && isKnowingNewPeople)
        {
            SaveNewPerson(textSend);
        }
    }*/

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
                foreach (MemoryClass mc in agentShortTermMemory)
                {
                    if (mc.information.Contains(personName))
                    {
                        talaaaaa = true;

                        mc.weight = mc.activation = 1;
                        mc.memoryTime = System.DateTime.Now;
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
    private void SaveNewPerson(string namePerson)
    {
        //reset the result file
        StreamWriter writingResult;
        writingResult = File.CreateText("result.txt");
        writingResult.Write("");
        writingResult.Close();

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
        qntTempNodes = 2;
        string label = "name:'"+namePerson+ "',activation:1,weight:0.9,nodeType:'text',lastEmotion:'"+foundEmotion+ "',image:'AutobiographicalStorage/Images/" + namePerson + ".png'";
        StartCoroutine(CreateMemoryNodeWebService(namePerson, "Person", label, 0.9f));

        label = "name:'myself',image:'AutobiographicalStorage/Images/" + namePerson + ".png',activation:1,weight:0.9,nodeType:'image',lastEmotion:'" + foundEmotion + "'";
        StartCoroutine(CreateMemoryNodeWebService("myself", "Image", label, 0.9f));

        //type of the event, to save later
        tempTypeEvent = "meet new person";
        tempRelationship = "HAS_PHOTO";

        isKnowingNewPeople = false;

        //do not need to greet it right now
        peopleGreeted.Add(personName);

        saveNewMemoryNode = false;

        //now that they know each other, lets start to break the ice!
        isBreakingIce = true;
        BreakIce();
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

    //connect memory nodes
    private void ConnectMemoryNodes(List<int> memoryIDs)
    {
        for (int i = 0; i < agentShortTermMemory.Count; i++)
        {
            if (memoryIDs.Contains(agentShortTermMemory[i].informationID))
            {
                for (int j = 0; j < agentShortTermMemory.Count; j++)
                {
                    if (memoryIDs.Contains(agentShortTermMemory[j].informationID) &&
                        agentShortTermMemory[j].informationID != agentShortTermMemory[i].informationID)
                    {
                        agentShortTermMemory[i].nodes.Add(agentShortTermMemory[j]);
                        continue;
                    }
                }
            }
        }
    }

    private float CalculateSaccade()
    {
        return (-6.9f * Mathf.Log(UnityEngine.Random.Range(1, 15) / 15.7f));
    }

    //add to stm and return the memory ID
    private int AddToSTM(int informationType, string information, float weight = 0.1f, int nodeId = -1)
    {
        //first, checks if the memory already exists
        int ind = 0;
        bool backToSTM = false;

        foreach (MemoryClass st in agentShortTermMemory)
        {
            if (st.information == information)
            {
                ind = st.informationID;

                //since it already exists, the virtual agent is remembering it. Change the activation and weight
                st.activation = st.weight = 1;

                break;
            }
        }

        //if did not find it in STM, it may be in LTM. So, lets check
        if (ind == 0)
        {
            foreach (MemoryClass st in agentLongTermMemory)
            {
                if (st.information == information)
                {
                    ind = st.informationID;

                    //since it already exists, the virtual agent is remembering it. Change the activation and weight
                    st.activation = st.weight = 1;

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
                for (int i = 0; i < agentShortTermMemory.Count; i++)
                {
                    if (agentShortTermMemory[i].weight < minWeight)
                    {
                        minWeight = agentShortTermMemory[i].weight;
                        less = i;
                    }
                }

                if (less != -1)
                {
                    //transfer to the LTM
                    agentLongTermMemory.Insert(0, agentShortTermMemory[less]);

                    //delete
                    agentShortTermMemory.RemoveAt(less);
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
                    ind = GenerateID();
                }
                newMemory = new MemoryClass(System.DateTime.Now, informationType, information, ind, weight);
                agentShortTermMemory.Insert(0, newMemory);
            }//else, it already exists in the LTM or in the STM.
            else
            {
                //if backToSTM is false, it is in the STM. So, does nothing
                //otherwise, it is in the LTM. Bring it to the STM
                if (backToSTM)
                {
                    foreach (MemoryClass ltm in agentLongTermMemory)
                    {
                        if (ltm.informationID == ind)
                        {
                            newMemory = ltm;
                            newMemory.memoryTime = System.DateTime.Now;
                            break;
                        }
                    }

                    agentShortTermMemory.Insert(0, newMemory);
                }
            }
        }

        return ind;
    }

    //add a new general event and return its id
    /*private int AddGeneralEvent(string typeEvent, string informationEvent, List<int> connectNodes)
    {
        //if the memory already contains this general event, or something similar, do not add
        int ind = -1;
        //int qntNodes = 0;
        //int totalNodes = informationEvent.Split(' ').Length;
        for (int i = 0; i < agentGeneralEvents.Count; i++)
        {
            /*qntNodes = 0;
            foreach(MemoryClass mg in agentGeneralEvents[i].nodes)
            {
                if (informationEvent.Contains(mg.information))
                {
                    qntNodes++;
                }
            }*

            if (informationEvent == agentGeneralEvents[i].information)
            {
                ind = i;
                break;
            }
        }

        if (ind >= 0)
        {
            //although we do not add a new general event, we can update the information
            agentGeneralEvents[ind].nodes.Clear();
            agentGeneralEvents[ind].eventType = typeEvent;
            agentGeneralEvents[ind].information = informationEvent;
            agentGeneralEvents[ind].polarity = lastPolarity;
            //add the updated memory nodes on this event
            foreach (MemoryClass mc in agentShortTermMemory)
            {
                if (connectNodes.Contains(mc.informationID) && !agentGeneralEvents[ind].nodes.Contains(mc))
                {
                    agentGeneralEvents[ind].nodes.Add(mc);
                }
            }

            return 0;
        }

        //create a new general event
        int geId = GenerateID();
        GeneralEvent ge = new GeneralEvent(System.DateTime.Now, typeEvent, informationEvent, geId, foundEmotion);

        //set the polarity
        ge.polarity = lastPolarity;

        //add the memory nodes on this event
        foreach (MemoryClass mc in agentShortTermMemory)
        {
            if (connectNodes.Contains(mc.informationID) && !ge.nodes.Contains(mc))
            {
                ge.nodes.Add(mc);
            }
        }

        //add the memory nodes on this event
        foreach (MemoryClass mc in agentLongTermMemory)
        {
            if (connectNodes.Contains(mc.informationID) && !ge.nodes.Contains(mc))
            {
                ge.nodes.Add(mc);
            }
        }

        //add to list
        agentGeneralEvents.Add(ge);

        return geId;
    }*/

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
                int maxMemories = agentShortTermMemory.Count;
                for (int k = 0; k < maxMemories; k++)
                {
                    //UnityEngine.Debug.Log(System.DateTime.Now - agentShortTermMemory[k].memoryTime);
                    if (System.DateTime.Now - agentShortTermMemory[k].memoryTime >= memorySpan)
                    {
                        //check if this memory does not already exists in long term
                        bool exists = false;
                        foreach (MemoryClass lt in agentLongTermMemory)
                        {
                            if (lt.information == agentShortTermMemory[k].information)
                            {
                                exists = true;
                                break;
                            }
                        }

                        if (!exists)
                        {
                            //before delete, transfer to LTM
                            agentLongTermMemory.Insert(0, agentShortTermMemory[k]);
                        }

                        //byyyeeee
                        agentShortTermMemory.RemoveAt(k);
                        maxMemories--;
                        k--;
                    }
                }

                //check if the list already contains the person
                /*int ind = -1;
                int i = 0;
                foreach (MemoryClass mem in agentShortTermMemory)
                {
                    //UnityEngine.Debug.Log(agentShortTermMemory[i].person + " - " + faceName.GetComponent<Text>().text);
                    if (agentShortTermMemory[i].information == faceName.GetComponent<Text>().text)
                    {
                        ind = i;
                        break;
                    }

                    i++;
                }*/

                //memory decay
                foreach (MemoryClass mem in agentShortTermMemory)
                {
                    System.TimeSpan memTime = System.DateTime.Now - mem.memoryTime;
                    //UnityEngine.Debug.Log(memTime.Seconds);

                    if (memTime.Seconds > 1)
                    {
                        //exponential function for our interval, using log
                        mem.activation = Mathf.Log(mem.activation + 1);
                        //UnityEngine.Debug.Log(mem.activation);

                        //if activation drops below 0.2, loses a bit weight also
                        //update: if has max weight, memory node is permanent
                        if (mem.activation < 0.2f && mem.weight < 0.9)
                        {
                            mem.weight = Mathf.Log(mem.weight + 1);
                        }
                    }
                }
            }
        }
    }

    //save text LTM file
    //rem = true -> save just complete information
    /*private void SaveLTM(bool rem = false)
    {
        //save LTM as it is
        StreamWriter writingLTM;
        writingLTM = File.CreateText("AutobiographicalStorage/textLTM.txt");
        //writingLTM = File.AppendText(Application.dataPath + "/AutobiographicalStorage/textLTM.txt");

        //we define a memory is being rehearsed if it was updated max 2 seconds ago.
        foreach (MemoryClass mem in agentLongTermMemory)
        {
            if (rem)
            {
                //just save if information is complete
                if (!mem.information.Contains("_"))
                {
                    //Timestamp;ID;Information;Type;Activation;Weight;Node1;Node2;...
                    writingLTM.Write(mem.memoryTime + ";" + mem.informationID.ToString() + ";" + mem.information.Trim() + ";"
                        + mem.informationType.ToString() + ";" + mem.activation.ToString() + ";" + mem.weight.ToString());

                    //if has nodes connected, save it also
                    if (mem.nodes.Count > 0)
                    {
                        foreach (MemoryClass mc in mem.nodes)
                        {
                            writingLTM.Write(";" + mc.informationID);
                        }
                    }

                    writingLTM.Write("\n");
                }
            }
            else
            {
                //Timestamp;ID;Information;Type;Activation;Weight;Node1;Node2;...
                writingLTM.Write(mem.memoryTime + ";" + mem.informationID.ToString() + ";" + mem.information.Trim() + ";"
                    + mem.informationType.ToString() + ";" + mem.activation + ";" + mem.weight);

                //if has nodes connected, save it also
                if (mem.nodes.Count > 0)
                {
                    foreach (MemoryClass mc in mem.nodes)
                    {
                        writingLTM.Write(";" + mc.informationID);
                    }
                }

                writingLTM.Write("\n");
            }
        }
        writingLTM.Close();
    }*/

    //save general events file
    /*private void SaveGeneralEvents()
    {
        //save general events as it is
        StreamWriter writingGE;
        writingGE = File.CreateText("AutobiographicalStorage/generalEvents.txt");

        //we define a memory is being rehearsed if it was updated max 2 seconds ago.
        foreach (GeneralEvent ge in agentGeneralEvents)
        {
            //Timestamp;ID;Information;Type;Emotion;Polarity;Node1;Node2;...
            writingGE.Write(ge.eventTime + ";" + ge.informationID.ToString() + ";" + ge.information.Trim() + ";"
                + ge.eventType + ";" + ge.emotion + ";" + ge.polarity);

            //if has nodes connected, save it also
            if (ge.nodes.Count > 0)
            {
                foreach (MemoryClass mc in ge.nodes)
                {
                    writingGE.Write(";" + mc.informationID);
                }
            }

            writingGE.Write("\n");
        }
        writingGE.Close();
    }*/

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

    //consolidate memory on REM sleep
    /*private void MemoryREM()
    {
        //first idea: all incomplete information from the LTM, cleaning both memories

        //copy from STM to LTM
        foreach (MemoryClass stm in agentShortTermMemory)
        {
            //check if this memory does not already exists in long term
            bool exists = false;
            foreach (MemoryClass lt in agentLongTermMemory)
            {
                if (lt.information == stm.information)
                {
                    exists = true;
                    break;
                }
            }

            //if does not exist, copy
            if (!exists)
            {
                agentLongTermMemory.Insert(0, stm);
            }
        }

        //clean STM
        agentShortTermMemory.Clear();

        //first: all memory nodes with low activation have their respective weights lowered
        //update: memory nodes with weight 1 are considered permanent
        foreach (MemoryClass memC in agentLongTermMemory)
        {
            if (memC.activation < 0.2f && memC.weight < 0.9)
            {
                memC.weight = Mathf.Log(memC.weight + 1);
            }
        }

        //all memory nodes with low weight are removed
        int altCount = agentLongTermMemory.Count;
        for (int i = 0; i < altCount; i++)
        {
            if (agentLongTermMemory[i].weight <= weightThreshold)
            {
                //get its ID, so we can remove from general events also
                int memId = agentLongTermMemory[i].informationID;

                //check general events with this ID
                /*for (int z = 0; z < agentGeneralEvents.Count; z++)
                {
                    for (int j = 0; j < agentGeneralEvents[z].nodes.Count; j++)
                    {
                        if (memId == agentGeneralEvents[z].nodes[j].informationID)
                        {
                            agentGeneralEvents[z].nodes.RemoveAt(j);

                            break;
                        }
                    }
                }*

                //remove the memory itself
                agentLongTermMemory.RemoveAt(i);
                altCount--;
                i--;
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
        }

        //after removing memories, check the general events which have no more nodes
        int geCount = agentGeneralEvents.Count;
        for (int z = 0; z < geCount; z++)
        {
            //if after we remove, there are no more nodes, the event itself is not important
            if (agentGeneralEvents[z].nodes.Count == 0)
            {
                agentGeneralEvents.RemoveAt(z);
                z--;
                geCount--;
            }
        }

        //save general events
        SaveGeneralEvents();*

        //delete information from LTM.
        //Basically, we save the new LTM file with just complete information.
        //SaveLTM();

        //clean LTM
        //agentLongTermMemory.Clear();
    }*/

    //consolidate memory on REM sleep (Graph mode)
    private void MemoryREM()
    {
        StartCoroutine(ConsolidationWebService());
    }

    IEnumerator ConsolidationWebService()
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
            }
        }
    }

    //retrieve a memory based on cues
    //deactivated for now
/*private GeneralEvent GenerativeRetrieval(Dictionary<string, string> cues)
{
    GeneralEvent eventFound = new GeneralEvent();

    //we find the general event which has the most cues compounding its memory nodes
    //BUT... select the general event is a bit trickier, since it can exist many events with the same memory information.
    //so, we select the event which has the most cues
    int maxCues = 0;
    foreach (GeneralEvent geez in agentGeneralEvents)
    {
        //for each general event, we count the cues found
        int eventCues = 0;
        //for each memory node which compounds this general event
        foreach (MemoryClass node in geez.nodes)
        {
            //if it exists, ++
            if (cues.ContainsKey(node.information))
            {
                eventCues++;
            }
        }

        //if it is higher than the max cues, select this general event
        if (eventCues > maxCues)
        {
            maxCues = eventCues;
            eventFound = geez;
        }
    }

    //if maxCues changed, we found an event
    if (maxCues > 0)
    {
        //add the nodes back to the STM
        foreach (MemoryClass mem in eventFound.nodes)
        {
            AddToSTM(mem.informationType, mem.information, mem.weight);
        }

        return eventFound;
    }//else, nothing was found
    else
    {
        return null;
    }
}*/

//new generative retrieval, based on the graph memory
void GenerativeRetrieval(Dictionary<string, string> cues)
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
                verbs.Add(cu.Key);
            }else if (cu.Value == "NN" || cu.Value == "NNP")
            {
                nouns.Add(cu.Key);

                //if (cu.Key == personName) whoIsIt = cu.Key;
                /*else*/ if (cu.Key == "Arthur") whoIsIt = cu.Key;
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
    }

    /*public void FollowFace(Vector3 point)
    {
        foreach (GameObject eye in eyes)
        {
            eye.GetComponent<EyeController>().FollowFace(point);
        }
    }*/

    //NOT USING SO FAR...
    /*private void DrawLine(Vector3 start, Vector3 end)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        //lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    private void DrawRectangle(Vector3 firstVertex, Vector3 lastVertex)
    {
        DrawLine(firstVertex, new Vector3(lastVertex.x, firstVertex.y, lastVertex.z));
        DrawLine(firstVertex, new Vector3(firstVertex.x, lastVertex.y, lastVertex.z));
        DrawLine(lastVertex, new Vector3(lastVertex.x, firstVertex.y, lastVertex.z));
        DrawLine(lastVertex, new Vector3(firstVertex.x, lastVertex.y, lastVertex.z));
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
        //int newId = AddToSTM(0, importantNoun, 1);

        //create a new autobiographical storage for the thing image
        //int newIdImage = AddToSTM(1, "AutobiographicalStorage/Images/" + importantNoun + ".png", 1);

        //save on Neo4j
        //on temp, we have to find 2 information later
        qntTempNodes = 2;
        string label = "name:'" + importantNoun + "',activation:1,weight:0.9,nodeType:'text',lastEmotion:'" + foundEmotion + "',image:'AutobiographicalStorage/Images/" + importantNoun + ".png'";
        StartCoroutine(CreateMemoryNodeWebService(importantNoun, "Thing", label, 0.9f));

        label = "name:'thing',image:'AutobiographicalStorage/Images/" + importantNoun + ".png',activation:1,weight:0.9,nodeType:'image',lastEmotion:'" + foundEmotion + "'";
        StartCoroutine(CreateMemoryNodeWebService("thing", "Image", label, 0.9f));

        //type of the event, to save later
        tempTypeEvent = "learn thing";
        tempRelationship = "HAS_PHOTO";
        arthurLearnsSomething = true;

        /*List<int> connectNodes = new List<int>();
        connectNodes.Add(newId);
        connectNodes.Add(newIdImage);

        //connect them both
        ConnectMemoryNodes(connectNodes);

        //create a new general event
        AddGeneralEvent("learn thing", "i learned what a " + importantNoun + " is", connectNodes);*/

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

            //so, lets try to find some general event
            MemoryClass fuck = null;

            /*foreach (GeneralEvent geez in agentGeneralEvents)
            {
                //if it exists, ding!
                if (geez.information.Contains(personName) && geez.information.Contains(target.GetType()))
                {
                    fuck = geez;
                    break;
                }
            }*/

            foreach(MemoryClass mc in agentLongTermMemory)
            {
                //if it exists, ding!
                string targetType = target.GetType();
                if (targetType == "old") targetType = "age";
                if (mc.information.Contains(personName) && mc.properties.ContainsKey(targetType))
                {
                    fuck = mc;
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

    //find next small talk
    /*private void SmallTalking(string beforeText = "")
    {
        //it is over already
        if (usingSmallTalk == -1) return;

        saveNewMemoryNode = false;
        isSmallTalking = true;

        //reset idle timer
        idleTimer = Time.time;

        SmallTalkClass actualST = smallTalk.FindSmallTalk(usingSmallTalk);

        //just follow the tree
        //if still not using any, get the first
        if (usingSmallTalk == 0)
        {
            usingSmallTalk = smallTalk.GetChild(0).GetId();
            //rootSmallTalk = smallTalk.GetChild(0).GetId();
            rootSmallTalk = 0;
        }
        else
        {
            //we get the children which corresponds with the polarity of the user's answer
            if (actualST.QntChildren() > 1)
            {
                if(actualST.GetPolarity() == true && lastPolarity > 0)
                {
                    if(actualST.GetChild(0).GetPolarity() == true)
                    {
                        //down the hill
                        usingSmallTalk = actualST.GetChild(0).GetId();
                    }else if (actualST.GetChild(1).GetPolarity() == true)
                    {
                        //down the hill
                        usingSmallTalk = actualST.GetChild(1).GetId();
                    }
                }else
                if (actualST.GetPolarity() == false && lastPolarity < 0)
                {
                    if (actualST.GetChild(0).GetPolarity() == false)
                    {
                        //down the hill
                        usingSmallTalk = actualST.GetChild(0).GetId();
                    }
                    else if (actualST.GetChild(1).GetPolarity() == false)
                    {
                        //down the hill
                        usingSmallTalk = actualST.GetChild(1).GetId();
                    }
                }else if (actualST.GetPolarity() == true && lastPolarity < 0)
                {
                    if (actualST.GetChild(0).GetPolarity() == false)
                    {
                        //down the hill
                        usingSmallTalk = actualST.GetChild(0).GetId();
                    }
                    else if (actualST.GetChild(1).GetPolarity() == false)
                    {
                        //down the hill
                        usingSmallTalk = actualST.GetChild(1).GetId();
                    }
                }
                else
                if (actualST.GetPolarity() == false && lastPolarity > 0)
                {
                    if (actualST.GetChild(0).GetPolarity() == true)
                    {
                        //down the hill
                        usingSmallTalk = actualST.GetChild(0).GetId();
                    }
                    else if (actualST.GetChild(1).GetPolarity() == true)
                    {
                        //down the hill
                        usingSmallTalk = actualST.GetChild(1).GetId();
                    }
                }
                /*else
                {
                    //otherwise, we just get next
                    int thisChild = actualST.CheckWhichChild();

                    //next one
                    thisChild++;

                    //see if the parent has more children
                    if (smallTalk.FindSmallTalk(rootSmallTalk).QntChildren() > thisChild)
                    {
                        usingSmallTalk = smallTalk.FindSmallTalk(rootSmallTalk).GetChild(thisChild).GetId();
                    }//otherwise, we are done
                    else
                    {
                        usingSmallTalk = rootSmallTalk = -1;
                    }
                }*
            }//else, we reached a leaf. Go back to root and get next
            else
            {
                rootSmallTalk++;
                if(smallTalk.QntChildren() > rootSmallTalk)
                {
                    usingSmallTalk = smallTalk.GetChild(rootSmallTalk).GetId();
                }//else, we are done
                else
                {
                    usingSmallTalk = -1;
                }
            }
        }

        //if found some small talk to still use, small talk should not be empty
        if (usingSmallTalk > 0)
        {
            //before we speak, we should check the memory to see if this questions was already answered before.
            //DEACTIVATED SO FAR
            SmallTalkClass target = smallTalk.FindSmallTalk(usingSmallTalk);

            //so, lets try to find some general event
            GeneralEvent fuck = null;

            /*foreach (GeneralEvent geez in agentGeneralEvents)
            {
                //if it exists, ding!
                if (geez.information.Contains(personName)) //&& geez.information.Contains(target.GetType()
                {
                    fuck = geez;
                    break;
                }
            }*

            //if this small talk has no children, we are done for now
            if(target.QntChildren() == 0)
            {
                //since it is the answer, done
                isSmallTalking = false;
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
                }*
                //BreakIce(beforeText);
                return;
            }//otherwise, just make the question
            else
            {
                SpeakYouFool(beforeText + target.Getsentence());
            }
        }//else, there is no more to talk about. Stop it
        else
        {
            /*if (beforeText != "")
            {
                SpeakYouFool(beforeText);
            }
            else
            {
                SpeakYouFool("Thanks! Anything else you would like to talk about?");
            }*

            isSmallTalking = false;
        }
    }*/

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
        string ct;
        idleTimer = Time.time;
        saveNewMemoryNode = false;

        if (!currentTopic.IsDialoging()) //sort new dialog
        {
            if (!currentTopic.isDialogsAvailable()) PickTopic(); //there isnt available dialogs in current topic

            currentTopic.StartNewDialog();
        }

        ct = currentTopic.RunDialog(lastPolarity);

        if(ct != null)
            SpeakYouFool(ct);
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

    /*private void LoadSmallTalk()
    {
        string smallTalkFile = "smallTalk.txt";

        StreamReader readingLTM = new StreamReader(smallTalkFile, System.Text.Encoding.Default);
        using (readingLTM)
        {
            string line;
            //aux vector just to build the tree up
            Dictionary<int, SmallTalkClass> aux = new Dictionary<int, SmallTalkClass>();
            do
            {
                line = readingLTM.ReadLine();

                if (line != "" && line != null)
                {
                    //skip comments
                    if (line.Contains("#")) continue;

                    //id;sentence;polarity;parent
                    string[] info = line.Split(';');
                    int ibId = int.Parse(info[0]);
                    string ibQuestion = info[1];
                    bool ibPolarity = bool.Parse(info[2]);
                    int ibParent = int.Parse(info[3]);

                    //if parent is 0, is one of the primary ones
                    if (ibParent == 0)
                    {
                        SmallTalkClass newST = new SmallTalkClass(ibId, ibQuestion, ibPolarity);
                        smallTalk.AddChild(newST);
                        aux.Add(ibId, newST);
                    }//otherwise, it is one of the secondary ones. Need to first find the parent and, then, add
                    else
                    {
                        SmallTalkClass aaa;
                        aux.TryGetValue(ibParent, out aaa);

                        SmallTalkClass newST = new SmallTalkClass(ibId, ibQuestion, ibPolarity);
                        aaa.AddChild(newST);
                        aux.Add(ibId, newST);
                        //smallTalk.FindSmallTalk(ibParent).AddChild(new SmallTalkClass(ibId, ibQuestion, ibPolarity));
                    }
                }
            } while (line != null);
        }
        readingLTM.Close();
    }*/

    //new parser
    public void LoadSmallTalk()
    {
        string smallTalkFile = "smallTalk.txt";

        StreamReader readingLTM = new StreamReader(smallTalkFile, System.Text.Encoding.Default);
        using (readingLTM)
        {
            string line;
            //Dictionary<int, Node> aux = new Dictionary<int, Node>();
            topics = new List<TopicGraph>();
            TopicGraph currentTopic = null;
            DialogGraph currentDialog = null;

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
                    currentTopic = new TopicGraph(line);
                    topics.Add(currentTopic);
                }
                //new dialog
                else if (command.Equals('['))
                {
                    currentDialog = new DialogGraph(line);
                }
                //dialog
                else if (command.Equals('#'))
                {

                    //id, conteúdo, polaridade, éfolha, nodo pai
                    string[] data = line.Split(';');
                    currentDialog.AddNode(int.Parse(data[0]), data[1].Trim(), double.Parse(data[2]), bool.Parse(data[3].Trim()), int.Parse(data[4]));
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

    }

    private IEnumerator MatchTopicsDialogs()
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
                    MatchesToLTM(www.downloadHandler.text);
                }//else, just normal match
                else
                {
                    MatchesFromRetrieval(www.downloadHandler.text, tokens);
                }
            }
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
    private IEnumerator CreateMemoryNodeWebService(string node, string typeNode = "", string label = "", float weight = 0.1f)
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
                int infoType = 0;
                if (typeNode == "Image") infoType = 1;

                AddToSTM(infoType, node, weight, idReturned);

                //add this on temp
                tempNodes.Add(idReturned, node);
            }
        }
    }

    //Web Service for update node in memory
    private IEnumerator UpdateMemoryNodeWebService(string node, string nodeKey = "", string nodeValue = "")
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
    }

    //Web Service for delete node in memory
    private IEnumerator DeleteMemoryNodeWebService(string node)
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
    }

    private IEnumerator CreateRelatioshipNodesWebService(int node, int node2, string relationship = "")
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
    }

    //Web Service for match (select)
    private IEnumerator MatchWebService(string match, bool addOnLTM = false, Dictionary<string,string> tokens = null)
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
                    MatchesToLTM(www.downloadHandler.text);
                }//else, just normal match
                else
                {
                    MatchesFromRetrieval(www.downloadHandler.text, tokens);
                }
            }
        }
    }

    //get all matches and place in the LTM
    private void MatchesToLTM(string results)
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
    }

    //matches from generative retrieval
    private void MatchesFromRetrieval(string results, Dictionary<string, string> tokens)
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
            string[] ohFuck = part2.Split('?');*/
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
        }*/

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
    }

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
            }
            else
            {
                sr.WriteLine(tokens[i] + ";" + tknType[i]);
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