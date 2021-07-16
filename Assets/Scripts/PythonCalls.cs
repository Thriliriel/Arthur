using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCXR.Demo
{
    [HelpURL("")]
    public class PythonCalls : MonoBehaviour
    {
        [SerializeField] UPython2ChannelSO pyChannel;
        private MainController mc;

        private void Awake()
        {
            mc = GameObject.Find("MainController").GetComponent<MainController>();
        }

        void Callback(string data)
        {
            Debug.Log($"[Rec:] {data}");
        }

        void CallbackFR(string data)
        {
            //Debug.Log($"[Rec:] {data}");
            mc.ParseFaceResult(data);
        }

        void CallbackSP(string data)
        {
            Debug.Log($"[Rec:] {data}");
        }

        void CallbackTK(string data)
        {
            Debug.Log($"[Rec:] {data}");
            mc.ParseTokens(data);
        }

        public void HelloWorld()
        {
            pyChannel.Call("hello.py", Callback);
        }

        public void FaceRecognition(string image64, string direc, string th, string mode)
        {
            pyChannel.Call("FaceRecognition.py " + image64 + " " + direc + " " + th + " " + mode, CallbackFR);
        }

        public void SavePerson(string image64, string direc, string name)
        {
            pyChannel.Call("SavePerson.py " + image64 + " " + direc + " " + name, CallbackSP);
        }

        public void Tokenization(string text)
        {
            pyChannel.Call("Tokenization.py " + '"'+text+'"', CallbackTK);
        }

        /*public void Sum()
        {
            pyChannel.Call("sum.py 1 2 3 4", Callback);
        }

        public void SumArray()
        {
            pyChannel.Call("sumarray.py 3,4,5,6", Callback);
        }

        public void Plot()
        {
            pyChannel.Call("plot.py 1,2,3,4 3,1,2,4", null);
        }*/
    }
}