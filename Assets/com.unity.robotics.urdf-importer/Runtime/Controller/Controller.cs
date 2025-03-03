using System;
using Unity.Robotics;
using UnityEngine;

namespace Unity.Robotics.UrdfImporter.Control
{
    public enum RotationDirection { None = 0, Positive = 1, Negative = -1 };
    public enum ControlType { PositionControl };

    public class Controller : MonoBehaviour
    {
        private ArticulationBody[] articulationChain;
        private Material[][] prevMaterials; // Stores original materials
        private int previousIndex;

        [InspectorReadOnly(hideInEditMode: true)]
        public string selectedJoint;
        [HideInInspector]
        public int selectedIndex;

        public ControlType control = ControlType.PositionControl;
        public float stiffness;
        public float damping;
        public float forceLimit = 1000;
        public float speed = 5f;
        public float torque = 100f;
        public float acceleration = 5f;

        [Tooltip("Material to highlight the currently selected joint")]
        public Material highlightMaterial; // Assign this in the Inspector

        void Start()
        {
            previousIndex = selectedIndex = 1;
            this.gameObject.AddComponent<FKRobot>();
            articulationChain = this.GetComponentsInChildren<ArticulationBody>();

            if (articulationChain.Length == 0)
            {
                Debug.LogError("No ArticulationBody found in children!");
                return;
            }

            prevMaterials = new Material[articulationChain.Length][];

            int defDyanmicVal = 10;
            foreach (ArticulationBody joint in articulationChain)
            {
                joint.gameObject.AddComponent<JointControl>();
                joint.jointFriction = defDyanmicVal;
                joint.angularDamping = defDyanmicVal;
                ArticulationDrive currentDrive = joint.xDrive;
                currentDrive.forceLimit = forceLimit;
                joint.xDrive = currentDrive;
            }

            DisplaySelectedJoint(selectedIndex);
            StoreJointMaterials(selectedIndex);
        }

        void SetSelectedJointIndex(int index)
        {
            if (articulationChain.Length > 0)
            {
                selectedIndex = (index + articulationChain.Length) % articulationChain.Length;
            }
        }

        void Update()
        {
            bool SelectionInput1 = Input.GetKeyDown(KeyCode.RightArrow);
            bool SelectionInput2 = Input.GetKeyDown(KeyCode.LeftArrow);

            if (SelectionInput2)
            {
                SetSelectedJointIndex(selectedIndex - 1);
                Highlight(selectedIndex);
            }
            else if (SelectionInput1)
            {
                SetSelectedJointIndex(selectedIndex + 1);
                Highlight(selectedIndex);
            }

            UpdateDirection(selectedIndex);
        }

        /// <summary>
        /// Changes the material of the selected joint
        /// </summary>
        private void Highlight(int selectedIndex)
        {
            if (selectedIndex == previousIndex || selectedIndex < 0 || selectedIndex >= articulationChain.Length)
            {
                return;
            }

            // Reset the material of the previously selected joint
            ResetJointMaterials(previousIndex);
            // Store materials of the newly selected joint
            StoreJointMaterials(selectedIndex);
            DisplaySelectedJoint(selectedIndex);

            // Locate the "Visuals" child
            Transform visuals = articulationChain[selectedIndex].transform.Find("Visuals");

            if (selectedIndex != 0)
            {
                if (visuals != null)
                {
                    Renderer[] rendererList = visuals.GetComponentsInChildren<Renderer>(); // Find all nested renderers

                    // Replace all materials with the highlight material
                    foreach (var renderer in rendererList)
                    {
                        Material[] newMaterials = new Material[renderer.materials.Length];
                        for (int i = 0; i < renderer.materials.Length; i++)
                        {
                            newMaterials[i] = highlightMaterial;
                        }
                        renderer.materials = newMaterials;
                    }
                }
            }
            
        }

        void DisplaySelectedJoint(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= articulationChain.Length)
            {
                return;
            }
            selectedJoint = articulationChain[selectedIndex].name + " (" + selectedIndex + ")";
        }

        /// <summary>
        /// Updates the joint movement direction based on user input
        /// </summary>
        private void UpdateDirection(int jointIndex)
        {
            if (jointIndex < 0 || jointIndex >= articulationChain.Length)
            {
                return;
            }

            float moveDirection = Input.GetAxis("Vertical");
            JointControl current = articulationChain[jointIndex].GetComponent<JointControl>();

            if (previousIndex != jointIndex)
            {
                JointControl previous = articulationChain[previousIndex].GetComponent<JointControl>();
                previous.direction = RotationDirection.None;
                previousIndex = jointIndex;
            }

            if (current.controltype != control)
            {
                UpdateControlType(current);
            }

            if (moveDirection > 0)
            {
                current.direction = RotationDirection.Positive;
            }
            else if (moveDirection < 0)
            {
                current.direction = RotationDirection.Negative;
            }
            else
            {
                current.direction = RotationDirection.None;
            }
        }

        /// <summary>
        /// Stores the original materials of a joint
        /// </summary>
        private void StoreJointMaterials(int index)
        {
            Transform visuals = articulationChain[index].transform.Find("Visuals");
            if (visuals == null) return;

            Renderer[] rendererList = visuals.GetComponentsInChildren<Renderer>();
            prevMaterials[index] = new Material[rendererList.Length];

            for (int i = 0; i < rendererList.Length; i++)
            {
                prevMaterials[index][i] = rendererList[i].material; // Store the original material
            }
        }

        /// <summary>
        /// Resets the joint to its original materials
        /// </summary>
        private void ResetJointMaterials(int index)
        {
            if (index < 0 || index >= articulationChain.Length || prevMaterials[index] == null) return;

            Transform visuals = articulationChain[index].transform.Find("Visuals");
            if (visuals == null) return;

            Renderer[] rendererList = visuals.GetComponentsInChildren<Renderer>();

            if (prevMaterials[index].Length != rendererList.Length) return;

            for (int i = 0; i < rendererList.Length; i++)
            {
                rendererList[i].material = prevMaterials[index][i]; // Restore original materials
            }
        }

        public void UpdateControlType(JointControl joint)
        {
            joint.controltype = control;
            if (control == ControlType.PositionControl)
            {
                ArticulationDrive drive = joint.joint.xDrive;
                drive.stiffness = stiffness;
                drive.damping = damping;
                joint.joint.xDrive = drive;
            }
        }

        public void OnGUI()
        {
            GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment = TextAnchor.UpperCenter;
            GUI.Label(new Rect(Screen.width / 2 - 200, 10, 400, 20), "Press left/right arrow keys to select a robot joint.", centeredStyle);
            GUI.Label(new Rect(Screen.width / 2 - 200, 30, 400, 20), "Press up/down arrow keys to move " + selectedJoint + ".", centeredStyle);
        }
    }
}
