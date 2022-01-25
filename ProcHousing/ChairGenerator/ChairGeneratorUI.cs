using UnityEngine;

namespace ProceduralToolkit.Examples.UI
{
    public class ChairGeneratorUI : MonoBehaviour
    {
        public MeshFilter meshFilter;
        public RectTransform leftPanel;

        public float legWidth = 0.07f;
        public float legHeight = 0.7f;
        public float seatWidth = 0.7f;
        public float seatDepth = 0.7f;
        public float seatHeight = 0.05f;
        public float backHeight = 0.8f;
        public bool hasStretchers = false;
        public bool hasArmrests = false;

        private void Start()
        {
            Generate();

        }
        private void Generate()
        {
            var draft = ChairGenerator.Chair(legWidth, legHeight, seatWidth, seatDepth, seatHeight, backHeight,
                hasStretchers, hasArmrests);
                 meshFilter.mesh = draft.ToMesh();
        }
    }
}