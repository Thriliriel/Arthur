using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Humelo
{
    ///@brief base class for TTS solutions implementation
    public abstract class LavstarTTSSolutionBase : MonoBehaviour
    {
        protected enum TTSSTATUS ///< for status of TTS service.
        {
            SUCCESS,
            FAIL,
            CHECKINTERNET,
            NOTREADY
        }

        protected bool isReady = false; ///< this variable is true when tts solution is ready to use.

        protected System.Collections.Generic.List<LavstarVoiceActor> allAvailableVoices = new System.Collections.Generic.List<LavstarVoiceActor>(); ///< list for available voice actors.

        [ReadOnly(true)]
        [Header("Api-key for TTS Solution")]
        [Tooltip("put valid Api-key")]
        public string apiKeyString = string.Empty; ///< for auth with cloud TTS service.

        [Header("TTS synthesize output file name")]
        [Tooltip("put name file name")]
        public string outputFileName = string.Empty; ///< As the name implies.

        protected LavstarVoiceActor defaultActor; ///< As the name implies.

        public event Action<GenerateResponse> GenerateEvent; ///< for event logic of TTS synthesize result.

        public event Func<object, EventArgs, Task> VoiceAvailable; ///< for filling voice available list.

        /*!
         * @brief A function to login cloud tts service.
         * @param[in] apikey API key string for TTS Solution (AWS Polly: Cognito Crendential, Google Cloud TTS: API Key, Prosody TTS: API Key) 
         * @return void
         */
        public async virtual void Login(string apikey)
        {
            throw new System.NotImplementedException();
        }

        /*!
        * @brief A function to get available actor voices.
        * @return List all available voices
        */
        public virtual System.Collections.Generic.List<LavstarVoiceActor> GetAllAvailableVoices()
        {
            return allAvailableVoices;
        }

        /*!
        * @brief A function for RequestAvailableActorsFromServer.
        * @return void
        */
        public async virtual void RequestAvailableActorsFromServer()
        {
            throw new System.NotImplementedException();
        }

        /*!
        * @brief A function for CheckinternetReachability.
        * @return bool true when internet is available.
        */
        protected bool CheckinternetReachability()
        {
             if(Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("Error. Check internet connection!");
                return false;
            }

            return true;
        }

        /*!
        * @brief A function for synthesize TTS.
        * @param[in] text text string for TTS.
        * @param[in] actor voice actor.
        * @param[in] outputFile for local wav file save.
        * @param[in] audioSource for playback.
        * @return IEnumerator for coroutine execution.
        */
        public virtual IEnumerator Synthesize(string text, LavstarVoiceActor actor, string outputFile, AudioSource audioSource)
        {
            throw new System.NotImplementedException();
        }

        /*!
        * @brief A function for getting available voice actors by gender.
        * @return List
        */
        public System.Collections.Generic.List<LavstarVoiceActor> VoicesForGender(VoiceActorGender gender)
        {
            System.Collections.Generic.List<LavstarVoiceActor> voices = new System.Collections.Generic.List<LavstarVoiceActor>(allAvailableVoices.Count);
            
            voices.AddRange(allAvailableVoices.Where(voice => voice.Gender == gender));
            
            return voices;
        }

        /*!
        * @brief A function for getting voice actor by name.
        * @return LavstarVoiceActor
        */
        public LavstarVoiceActor GetVoiceActorInfoByName(string actorname)
        {

            for (int i = 0; i < allAvailableVoices.Count; i++)
            {
                if (allAvailableVoices[i].Name == actorname)
                    return allAvailableVoices[i];
            }

            return null;
        }

        /*!
        * @brief A function for playing audio clip.
        * @return IEnumerator for coroutine execution.
        */
        public IEnumerator PlayAudioClip(string file_path, AudioSource source, AudioType type = AudioType.WAV)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://"+ file_path, type))
            {
                yield return www.Send();

                if (www.isNetworkError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);

                    source.Stop();
                    source.clip = myClip;
                    source.Play();

                }
            }
        }

        /*!
        * @brief A function for GenerateEvent.
        * @return void
        */
        protected void GenerateEventCall(TTSSTATUS status)
        {
            switch (status)
            {
                case TTSSTATUS.SUCCESS:
                    {
                        GenerateEvent(new GenerateResponse("TTS synthesize Success"));
                        break;
                    }
                case TTSSTATUS.FAIL:
                    {
                        GenerateEvent(new GenerateResponse("TTS synthesize Fail"));
                        break;
                    }
                case TTSSTATUS.CHECKINTERNET:
                    {
                        GenerateEvent(new GenerateResponse("Check internet connection"));
                        break;
                    }
                case TTSSTATUS.NOTREADY:
                    {
                        GenerateEvent(new GenerateResponse("TTS solution is not ready"));
                        break;
                    }
                default:
                    {
                        break;
                    }
            }            
        }

        /*!
        * @brief A function for GenerateEventCall.
        * @return void
        */
        protected void GenerateEventCall(GenerateResponse response)
        {
            Debug.Log(response.response);

            try
            {
                GenerateEvent(response);
            }
            catch (NullReferenceException e)
            {
                Debug.Log(e.ToString());
            }
    
        }

        /*!
        * @brief A function for handling RequestVoiceAvailableList.
        * @return VoiceActorInfo[]
        */
        public async Task<VoiceActorInfo[]> OnRequestVoiceAvailableList(VoiceActorInfo[] result)
        {
            for (int i = 0; i < result.Length; i++)
            {
                VoiceActorGender gender = VoiceActorGender.MALE;
                if (result[i].gender == "female")
                {
                    gender = VoiceActorGender.FEMALE;
                }

                allAvailableVoices.Add(new LavstarVoiceActor(result[i].name, gender, result[i].language));
            }

            Func<object, EventArgs, Task> handler = VoiceAvailable;


            if (handler != null)
            {
                Delegate[] invocationList = handler.GetInvocationList();
                Task[] handlerTasks = new Task[invocationList.Length];

                for (int i = 0; i < invocationList.Length; i++)
                {
                    handlerTasks[i] = ((Func<object, EventArgs, Task>)invocationList[i])(this, EventArgs.Empty);
                }

                await Task.WhenAll(handlerTasks);
            }

            return result;
        }
    }

    ///@brief a class for TTS synthesize generate response
    public class GenerateResponse
    {
        public string response; ///< As the name implies

        public GenerateResponse(string _response)
        {
            response = _response;
        }
    }
}
// © 2019-2020 Humelo Inc.