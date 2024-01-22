using EFT.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace StayInTarkov.UI
{
    internal class PaulovTMPManager
    {
        private Canvas m_canvas;
        public HashSet<GameObject> m_gameObjects = new();
        public PaulovTMPManager() { }
        public GameObject InstantiateTarkovTextLabel(string name, string text, float fontSize, Vector3 relativePosition)
        {
            m_canvas = GameObject.FindObjectOfType<Canvas>();
            return InstantiateTarkovTextLabel(name, m_canvas.transform, text, fontSize, relativePosition);  
        }

        public GameObject InstantiateTarkovTextLabel(string name, Transform parentTransform, string text, float fontSize, Vector3 relativePosition)
        {
            var newObject = new GameObject(name);
            m_gameObjects.Add(newObject);
            newObject.GetOrAddComponent<RectTransform>();
            newObject.transform.parent = parentTransform;
            newObject.AddComponent<CustomTextMeshProUGUI>();
            newObject.GetComponent<CustomTextMeshProUGUI>().text = text;
            newObject.GetComponent<CustomTextMeshProUGUI>().fontSize = fontSize;
            newObject.GetComponent<CustomTextMeshProUGUI>().fontSizeMax = fontSize;
            newObject.GetComponent<CustomTextMeshProUGUI>().fontSizeMin = fontSize;
            newObject.transform.localPosition = relativePosition;
            return newObject;
        }

        // -----------------------------------------------
        // ------- These don't work right now ------------
        //

        //public GameObject InstantiateTarkovButton(string name, string text, float fontSize, Vector3 relativePosition)
        //{
        //    m_canvas = GameObject.FindObjectOfType<Canvas>();
        //    return InstantiateTarkovButton(name, m_canvas.transform, text, fontSize, relativePosition);
        //}


        //public GameObject InstantiateTarkovButton(string name, Transform parentTransform, string text, float fontSize, Vector3 relativePosition)
        //{
        //    var newObject = new GameObject(name);
        //    m_gameObjects.Add(newObject);

        //    newObject.GetOrAddComponent<RectTransform>();
        //    newObject.transform.parent = parentTransform;
        //    var duib = newObject.AddComponent<DefaultUIButton>();
        //    newObject.AddComponent<DefaultUIButtonAnimation>();
        //    //newObject.AddComponent<TweenAnimatedButton>();
        //    newObject.AddComponent<HorizontalLayoutGroup>();
        //    newObject.AddComponent<LayoutElement>();
        //    duib.SetRawText(text, (int)Math.Round(fontSize));
        //    newObject.transform.localPosition = relativePosition;
        //    return newObject;
        //}

        //
        // -----------------------------------------------

        public void DestroyObjects()
        {
            foreach(var go in m_gameObjects)
            {
                GameObject.DestroyImmediate(go);
            }
        }
    }
}
