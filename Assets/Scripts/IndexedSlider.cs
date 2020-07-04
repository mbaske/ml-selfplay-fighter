using UnityEngine;
using UnityEngine.UI;
using System;

namespace MBaske
{
    public class IndexedSlider : MonoBehaviour
    {
        public Action<int, float> OnValueChange { get; set; }

        private int index;

        public void OnSliderValueChange(float val)
        {
            OnValueChange?.Invoke(index, val);
        }

        public void SetIndex(int index)
        {
            this.index = index;
            name = index.ToString();
        }

        public void SetInteractable(bool b)
        {
            GetComponent<Slider>().interactable = b;
        }
    }
}