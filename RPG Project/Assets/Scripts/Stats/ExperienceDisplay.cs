using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RPG.Stats
{
    public class ExperienceDisplay : MonoBehaviour
    {
        Experience experience;

        private void Awake() {
            experience = GameObject.FindWithTag("Player").GetComponent<Experience>();
        }

        private void Update() {
            GetComponent<TextMeshProUGUI>().text = String.Format("{0:0}", experience.GetPoints());  // un decimal = 0:0.0
        }
    }

}
