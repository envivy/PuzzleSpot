using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Level : MonoBehaviour
{
   public int levelID;
   
   public virtual void RefreshUIState(int elementID) { }
   
}
