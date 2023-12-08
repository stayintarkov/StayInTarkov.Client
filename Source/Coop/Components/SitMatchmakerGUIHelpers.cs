using UnityEngine;

namespace StayInTarkov.Coop.Components
{

    public class CalculateXAxis
    {
        /// <summary>
        /// X-Axis for a checkbox
        /// </summary>
        public float Checkbox { get; private set; } = 0;
        /// <summary>
        /// X-Axis to put text besides a checkbox
        /// </summary>
        public float CheckboxText { get; private set; } = 0;
        /// <summary>
        /// X-Axis for text that's not needed to be formatted next to a checkbox
        /// </summary>
        public float Text { get; private set; } = 0;

        public CalculateXAxis(GUIContent Content, float WindowWidth, int HorizontalSpacing = 10)
        {
            var CalcSize = GUI.skin.label.CalcSize(Content);

            Checkbox = WindowWidth - CalcSize.x / 2 - HorizontalSpacing;
            CheckboxText = Checkbox + 20;
            Text = Checkbox;
        }
    }

}
