using System;
using System.Collections.Generic;
using HR_Toolkit.Thresholds;
using UnityEditor;
using UnityEngine;

namespace HR_Toolkit
{
    public class RedirectionManager : MonoBehaviour
    {
        /// <summary>
        ///  With a static instance of this object, all other objects can get the data
        /// </summary>
        public static RedirectionManager instance;
        
        /// <summary>
        /// The virtual world parent game object. Needs to be set to the object that will be rotated
        /// with the World Warping Redirection Techniques
        /// </summary>
        public GameObject virtualWorld;
        /// <summary>
        /// The game object of the physically tracked hand
        /// </summary>
        public GameObject realHand;
        /// <summary>
        /// The game object of the virtual hand
        /// </summary>
        public GameObject virtualHand;
        /// <summary>
        /// The warp origin that is used by all redirection techniques. If it is set to NONE, it will
        /// be set to the hand's real position on the start of each redirection
        /// </summary>
        public GameObject warpOrigin;
        

        /// <summary>
        /// The disance threshold to align the real and virtual hand
        /// </summary>
        public const float handAlignmentDistance = 0.01f;
        /// <summary>
        /// Check if the next target is the reset position 
        /// </summary>
        private bool useResetPosition;
        /// <summary>
        /// The physically tracked head position
        /// </summary>
        public GameObject body;
        
        /// <summary>
        /// Reset Position is set in each RedirectionTechnique.
        /// The ResetPosition is used between two redirections. Instead of redirecting from one target to another target,
        /// the user will be redirected to the reset position first. This prevents to huge redirections. 
        /// </summary>
        [Tooltip("Note: This RedirectionObject needs an unmodified VirtualToRealConnection (realPos=virtualPos)")]
        public RedirectionObject resetPosition;
        
        [Space(20)]

        /// <summary>
        /// The movement controller is automatically added to the Redirection Manager, it tracks the actual
        /// movement options and it's parameters
        /// </summary>
        public MovementController movementController;
        /// <summary>
        /// A list which holds all redirected prefabs. On default it serves as a new target selection but
        /// can be edited manually
        /// </summary>
        public List<RedirectionObject> allRedirectedPrefabs;

        /// <summary>
        /// The Redirection Technique in the Redirection Manager serves as the default Redirection
        /// Technique, which is used by all Redirected Prefabs, that do not specify another technique 
        /// </summary>
        public HandRedirector redirectionTechnique;

        /// <summary>
        /// The active redirected prefab. It's redirection technique 'ApplyRedirection()' Method is
        /// called in the Update()
        /// </summary>
        public RedirectionObject target;
        /// <summary>
        /// The last redirected target
        /// </summary>
        public RedirectionObject lastTarget;
       

        /// <summary>
        /// The speed of the hand movement when the hand is controlled by the mouse
        /// </summary>
        [HideInInspector]
        public float speed;
        /// <summary>
        /// The speed of the height hand movement when the hand is controlled by the mouse
        /// </summary>
        [HideInInspector]
        public float mouseWheelSpeed;
        /// <summary>
        /// The selected movement option.
        /// </summary>
        [HideInInspector]
        public MovementController.Movement movement;

        public bool reachedTarget = false;

        private LineRenderer lineRenderer;

        /// <summary>
        /// On Awake we set the static instance of the Redirection Manager, so that all
        /// other objects can access it's data. The in editor selected Redirection Technique
        /// becomes the default Redirection Technique for all objects.
        /// </summary>
        private void Awake()
        {
            instance = this;
            instance.redirectionTechnique = redirectionTechnique;
        }


        /// <summary>
        /// On Start we set the virtual hand position to the real hand position,
        /// Initialize the MovementController with the real hand,
        /// Set the mesh of the real and virtual hand to the virtual/physical camera layers
        /// to render them only on the needed cameras
        /// </summary>
        private void Start()
        {
            SetVirtualHandToRealHand();
            movementController.Init(realHand);
            //SetLayerRecursively(realHand, "Physical/Hand");
            //SetLayerRecursively(virtualHand, "Virtual/Hand");
            
            if (lineRenderer!= null)
            {
                lineRenderer = GetComponentInChildren<LineRenderer>();
                lineRenderer.startWidth = 0.05f;
                lineRenderer.endWidth = 0.05f;
            }
        }

        /// <summary>
        /// In the Update we do:
        ///   - Move the hand with the movement controller
        ///   - Check for an 'space'-key input to change the target
        ///     - if it is changed, the warp origin will be set to the actual real hand pos,
        ///     - the last target is the to the actual target
        ///     - set a new target with GetNextTarget()
        ///     - update the highlighted objects
        ///   - call the Redirect() Method on the target (redirected prefab)
        /// TODO thresholds
        /// </summary>
        private void Update()
        {
            movementController.MoveHand();
            movementController.MoveBody();
            
            // check for space input -> Check for new target
            CheckForNewTarget();


            if (target == null)     // 1-to-1 hand mapping
            {
                virtualHand.transform.position = realHand.transform.position;
                //use hand rotation of real hand (can be overwritten by Redirect())
                virtualHand.transform.rotation = realHand.transform.rotation;
                reachedTarget = false;
            }
            else
            {                       // redirection
                //apply redirection
                target.Redirect();
                //use hand rotation of real hand (can be overwritten by Redirect())
                virtualHand.transform.rotation = realHand.transform.rotation;
            }
        }


        #region Called in Update
        private void CheckForNewTarget()
        {
            if (!Input.GetKeyDown("space")) return;
            if (target != null)
            {
                if (!target.thisIsAResetPosition)
                {
                    // current no resetPosition: update lastTarget 
                    // curret is resetPosition: keep the index of the lastTarget and dont override it
                    lastTarget = target;
                }
                // end current redirection
                target.EndRedirection();
            }
            
            target = GetNextTarget();
            Debug.Log("--- new target: " + target.gameObject.name + " ---");

            UpdateWarpOrigin();
            if (target != null)
            {
                target.StartRedirection();
            }
        }

        private void UpdateWarpOrigin()
        {
            warpOrigin.transform.position = this.realHand.transform.position;
        }

        public void ResetTarget()
        {
            this.target = null;
        }

        private RedirectionObject GetNextTarget()
        {
            if (allRedirectedPrefabs.Count == 0)
            {
                throw new Exception("There are no redirected prefabs that could be targeted");
            }

            // there was no previous target selected, we need to set it on first call
            if (target == null && lastTarget == null)
            {
                reachedTarget = false;
                return allRedirectedPrefabs[0];
            }
            
            if (allRedirectedPrefabs.Count == 1)
            {
                Debug.Log("There is only one target, can't choose another target");
                return allRedirectedPrefabs[0];
            }

            var index = allRedirectedPrefabs.IndexOf(lastTarget);
            var newIndex = (index + 1) % allRedirectedPrefabs.Count;

            reachedTarget = false;
            return allRedirectedPrefabs[newIndex];        
        }

        public void ReturnToResetPosition()
        {
            // return if there is no resetPosition
            if (resetPosition == null)
            {
                Debug.Log("--- waiting for new target ---");
                return;
            }

            Debug.Log("--- return to ResetPosition or choose a new target ---");
            RedirectionManager.instance.reachedTarget = false;              // tell RedMan that there is a "new target"
            lastTarget = target;
            target = resetPosition;
            UpdateWarpOrigin();
            target.StartRedirection();
        }

        #endregion

        public void SetVirtualHandToRealHand()
        {
            virtualHand.transform.position = realHand.transform.position;
            virtualHand.transform.rotation = realHand.transform.rotation;
            virtualHand.transform.localScale = realHand.transform.localScale;
        }
        

        #region Render Hands on Layer
        /// <summary>
        /// Is used to set all game objects and its children to a specific layer, is used here to set
        /// the hand meshes from the SteamVR asset to a new layer, since they spawn after 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="newLayer"></param>
        void SetLayerRecursively(GameObject obj, string newLayer)
        {
            if (null == obj)
            {
                return;
            }
           
            obj.layer = LayerMask.NameToLayer(newLayer);
           
            foreach (Transform child in obj.transform)
            {
                if (null == child)
                {
                    continue;
                }
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
        
        private static void SetHandWithChildsToLayer_(GameObject obj, string name)
        {
            obj.layer = LayerMask.NameToLayer(name);
            foreach (var child in obj.GetComponentsInChildren<Transform>(true))  
            {
                //child.gameObject.layer = LayerMask.NameToLayer (name); 
                SetHandWithChildsToLayer_(child.gameObject, name);
                Debug.Log("Changed LAyer");
            }
        }

        #endregion

        #region Getter
        public LineRenderer GetLineRenderer()
        {
            return lineRenderer;
        }
        
        /// <summary>
        /// Returns the redirection technique which was set in the inspector on the
        /// Redirection Manager object in the inspector
        /// </summary>
        /// <returns></returns>
        public HandRedirector GetDefaultRedirectionTechnique()
        {
            return redirectionTechnique;
        }

        public GameObject GetDefaultWarpOrigin()
        {
            return warpOrigin;
        }
        
        /// <summary>
        /// Checks, if the virtual hand and the real hand are aligned. Displays the result on the overview screen
        /// </summary>
        public bool HandsAreAligned()
        {
            var handDistance = Vector3.Distance(virtualHand.transform.position, realHand.transform.position);
         
            if (handDistance < handAlignmentDistance)
            {
                return true;
            }
         
            return false;
        }

        public float GetHandDistance()
        {
            return Mathf.Abs(Vector3.Distance(virtualHand.transform.position,realHand.transform.position));
        }

        public RedirectionObject GetActiveTarget()
        {
            return target;
        }

        public void SetWarpOrigin(Vector3 newOrigin)
        {
            warpOrigin.transform.position = newOrigin;
        }

      
        
        #endregion
    }
}