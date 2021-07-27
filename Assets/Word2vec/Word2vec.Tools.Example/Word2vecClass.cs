using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word2vec.Tools;
using UnityEngine;

//link: https://github.com/tmteam/Word2vec.Tools
namespace Word2vec.Tools.Example
{
    class Word2vecClass
    {
        private Vocabulary vocabulary;
        private MainController mc;

        private void Main()
        {
            mc = GameObject.Find("MainController").GetComponent<MainController>();

            Debug.Log("Reading the model...");

            //Set your w2v bin file path here:
            //(approx 6GB Ram)
            var path = mc.absPath + "Assets/Word2vec/GoogleNews-vectors-negative300.bin";
            vocabulary = new Word2VecBinaryReader().Read(path);

            //For w2v text sampling file use:
            //var vocabulary = new Word2VecTextReader().Read(path);

            Debug.Log("vectors file: " + path);
            Debug.Log("vocabulary size: " + vocabulary.Words.Length);
            Debug.Log("w2v vector dimensions count: " + vocabulary.VectorDimensionsCount);

            //Debug.Log("\""+girl+"\" relates to \""+boy+"\" as \""+woman+"\" relates to ..."); 
            //var analogies = vocabulary.Analogy(girl, boy, woman, count);
            //foreach (var neightboor in analogies)
            //    Debug.Log(neightboor.Representation.WordOrNull + "\t\t" + neightboor.DistanceValue);

      //      #region addition
      //      Debug.Log("\""+girl+"\" + \""+boy+"\" = ...");
      //      var additionRepresentation = vocabulary[girl].Add(vocabulary[boy]);
      //      var closestAdditions = vocabulary.Distance(additionRepresentation, count);
      //      foreach (var neightboor in closestAdditions)
      //           Debug.Log(neightboor.Representation.WordOrNull + "\t\t" + neightboor.DistanceValue);
      //      #endregion

      //      #region subtraction
		    //Debug.Log("\""+girl+"\" - \""+boy+"\" = ...");
      //      var subtractionRepresentation = vocabulary[girl].Substract(vocabulary[boy]);
      //      var closestSubtractions = vocabulary.Distance(subtractionRepresentation, count);
      //      foreach (var neightboor in closestSubtractions)
      //          Debug.Log(neightboor.Representation.WordOrNull + "\t\t" + neightboor.DistanceValue);
      //      #endregion
        }

        public void Start()
        {
            Main();
        }

        public List<string> MostSimilar(string word, int count = 3)
        {
            List<string> ret = new List<string>();

            //Debug.Log("top " + count + " closest to \"" + word + "\" words:");
            var closest = vocabulary.Distance(word, count);

            // Is simmilar to:
            // var closest = vocabulary[boy].GetClosestFrom(vocabulary.Words.Where(w => w != vocabulary[boy]), count);

            foreach (var neightboor in closest)
            {
                string wd = neightboor.Representation.WordOrNull;
                if (wd != null)
                {
                    ret.Add(wd);
                    //Debug.Log(wd + "\t\t" + neightboor.DistanceValue);
                }
            }

            return ret;
        }
    }
}
