using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

   public static class ExtendFun
    {
        private static Dictionary<float, WaitForSeconds> secondDic;

        public static void DelayDoSecond(this MonoBehaviour mono, float delay, Action fun)
        {
            mono.StartCoroutine(DelayIESecond(delay, fun));
        }

        private static IEnumerator DelayIESecond(float delay, Action fun)
        {
            if (secondDic == null)
            {
                secondDic = new Dictionary<float, WaitForSeconds>();
            }
            if (secondDic.ContainsKey(delay))
            {
                yield return secondDic[delay];
            }
            else
            {
                secondDic.Add(delay, new WaitForSeconds(delay));
                yield return secondDic[delay];
            }
            fun();
        }

        public static void DelayDoFrame(this MonoBehaviour mono, int delay, Action fun)
        {
            mono.StartCoroutine(DelayIEFrame(delay, fun));
        }

        private static IEnumerator DelayIEFrame(int delay, Action fun)
        {
            for (int i = 0; i < delay; i++)
            {
                yield return null;
            }
            fun();
        }

        public static T Get2AddCompont<T>(this GameObject go) where T : Component
        {
            T re = go.GetComponent<T>();
            if (re == null)
            {
                re = go.AddComponent<T>();
            }
            return re;
        }

        public static void SetParent(this Transform tf, Transform parent, bool init)
        {
            tf.SetParent(parent);
            if (init)
            {
                tf.localScale = Vector3.one;
                tf.localPosition = Vector3.zero;
                tf.localRotation = Quaternion.identity;
            }
        }

        public static GameObject Clone(this GameObject go)
        {
            GameObject tmp = GameObject.Instantiate(go, go.transform.position, go.transform.rotation, go.transform.parent);
            tmp.transform.localScale = go.transform.localScale;
            return tmp;
        }

        public static T Clone<T>(this GameObject go) where T : Component
        {
            GameObject tmp = Clone(go);
            return tmp.GetComponent<T>();
        }
    }
